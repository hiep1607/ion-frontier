using UnityEngine;

namespace IonFrontier
{
    public sealed partial class IonGame
    {
        public const float EscapeRouteLength=3600;
        float escapeTravel,escapeRockClock,escapeSpawnClock,escapeArrivalTime;
        int escapeWave,escapeLastGap=-1;
        bool escapeFlankAnnounced,escapeCounterWindow,escapeFinalShot;
        float escapeLastSalvo=-10;
        readonly SpriteRenderer[] escapeWarnings=new SpriteRenderer[2],escapeMuzzles=new SpriteRenderer[2];
        public bool EscapeRunActive=>Sector==4&&!missionBossStarted&&(State==FlightState.Playing||State==FlightState.Paused||State==FlightState.UpgradeChoice);
        public float EscapeDistance=>escapeTravel;
        public float EscapeFraction=>Mathf.Clamp01(escapeTravel/EscapeRouteLength);
        public int EscapeRockWaves {get;private set;}
        public int EscapeSalvos {get;private set;}
        public int EscapeWarningEvents {get;private set;}
        public int EscapePursuerKills {get;private set;}
        public int EscapeRocksCleared {get;private set;}
        public int EscapeCrossingWaves {get;private set;}
        public int EscapePursuersAlive{get{int n=0;foreach(var e in enemies)if(e.active&&e.escapePursuer)n++;return n;}}
        public int EscapeWarningsAlive{get{int n=0;foreach(var e in enemies)if(e.active&&e.escapePursuer&&e.escapeWarning)n++;return n;}}
        public string EscapeProgressText=>EscapeFraction>=1?"100% / ĐÃ TỚI ĐIỂM NHẢY":Mathf.FloorToInt(EscapeFraction*100)+"% / "+Mathf.FloorToInt(escapeTravel)+" / 3600 m";
        void StartEscapeMission()
        {
            ResetEscapeMission();escapeRockClock=.45f;escapeSpawnClock=.35f;
            playerPosition=new Vector2(0,-2.2f);playerVelocity=Vector2.zero;
            for(int i=0;i<2;i++)if(escapeWarnings[i]==null)
            {
                escapeWarnings[i]=IonArt.Sprite("Pursuit firing warning",IonArt.Pixel,Vector2.zero,Vector2.one,Color.clear,7,transform);
                escapeMuzzles[i]=IonArt.Sprite("Pursuit charging muzzle",IonArt.Ring,Vector2.zero,Vector2.one,Color.clear,14,transform);
            }
            HideEscapeWarnings();
        }
        void HideEscapeWarnings(){for(int i=0;i<2;i++){if(escapeWarnings[i]!=null)escapeWarnings[i].gameObject.SetActive(false);if(escapeMuzzles[i]!=null)escapeMuzzles[i].gameObject.SetActive(false);}}
        void ResetEscapeMission()
        {
            ResetCounterattack();HideEscapeWarnings();escapeTravel=escapeArrivalTime=0;escapeRockClock=escapeSpawnClock=0;escapeWave=0;escapeLastGap=-1;
            escapeFlankAnnounced=escapeCounterWindow=escapeFinalShot=false;escapeLastSalvo=-10;
            EscapeRockWaves=EscapeSalvos=EscapeWarningEvents=EscapePursuerKills=EscapeRocksCleared=EscapeCrossingWaves=0;
            foreach(var e in enemies)if(e!=null){e.escapePursuer=false;e.escapeWarning=false;}
        }
        void TickEscapeMission(float dt)
        {
            if(!EscapeRunActive||State!=FlightState.Playing)return;
            if(escapeTravel>=EscapeRouteLength)
            {
                if(escapeArrivalTime==0)
                {
                    HideEscapeWarnings();RetireCounterForBoss();ClearHostileBullets();
                    foreach(var o in world)if(o.active&&o.kind==WorldKind.Rock)ReleaseWorld(o);
                    foreach(var e in enemies)if(e.active&&e.escapePursuer){e.active=false;e.escapeWarning=false;e.renderer.gameObject.SetActive(false);e.engine.gameObject.SetActive(false);e.halo.gameObject.SetActive(false);}
                    hud.Toast("ĐÃ TỚI ĐIỂM NHẢY / KẺ NGHIỀN ĐÁ ĐANG CHẶN LỐI!");
                }
                escapeArrivalTime+=dt;if(escapeArrivalTime>=1.2f)SpawnExpandedBoss();return;
            }
            escapeTravel=Mathf.Min(EscapeRouteLength,escapeTravel+50*dt);
            int beat=Mathf.Min(2,Mathf.FloorToInt(EscapeFraction*3));if(beat>flowBeat)EnterMissionBeat(beat);
            if(State!=FlightState.Playing)return;
            if(escapeTravel>=600&&!escapeFlankAnnounced){escapeFlankAnnounced=true;hud.Toast("TRUY KÍCH ĐANG ĐÁNH SƯỜN / CHÚ Ý ĐẠN TỪ HAI PHÍA!");}
            if(escapeTravel>=2000&&!counterStarted)BeginCounterFlypast();
            if(escapeTravel>=2400&&!escapeCounterWindow){escapeCounterWindow=true;EnterCounterWindow();}
            if(escapeTravel>=3200&&!counterSucceeded&&!counterExpired)ExpireCounterattack();
            if(counterExpired&&escapeTravel>=3300&&!escapeFinalShot)
            {
                escapeFinalShot=true;hud.Toast("PHÁO TRUY KÍCH CUỐI / TRÁNH VẠCH CAM!");
                // One final warned salvo; the visible warning lasts .85 route-seconds.
                var warning=escapeWarnings[0];Vector2 origin=new Vector2(-7,5.8f);counterAim[0]=(playerPosition-origin).normalized;
                DrawLine(warning,origin,origin+counterAim[0]*14,.045f,new Color(1,.5f,.1f,.55f));
            }
            if(counterExpired&&escapeFinalShot&&escapeTravel>=3342.5f&&escapeWarnings[0].gameObject.activeSelf)
            {Vector2 origin=new Vector2(-7,5.8f);for(int i=-1;i<=1;i++){var shot=Bolt(origin,(Vector2)(Quaternion.Euler(0,0,i*10)*counterAim[0])*4.5f,false,.13f);if(shot!=null)shot.damage=10;}escapeWarnings[0].gameObject.SetActive(false);EscapeSalvos++;}
            escapeRockClock-=dt;escapeSpawnClock-=dt;
            if(escapeRockClock<=0){SpawnEscapeRocks();escapeRockClock=escapeTravel>=2400?3.2f:flowBeat==0?2.05f:1.8f;}
            if(escapeTravel<2400&&escapeSpawnClock<=0&&EscapePursuersAlive<2){SpawnEscapePursuer();escapeSpawnClock=.9f;}
            EmergencyMissionAmmo(dt);
        }
        void SpawnEscapeRocks()
        {
            int columns=7,gap=(int)RandomRange(1,columns-1);
            if(gap==escapeLastGap)gap=1+(gap+1)%(columns-2);escapeLastGap=gap;float half=HalfWidth-.85f;
            bool crossing=escapeWave%3==2;float drift=crossing?(escapeWave%2==0?.65f:-.65f):0;
            if(crossing){EscapeCrossingWaves++;hud.Toast("MẢNH VỠ CẮT NGANG / ĐỔI LÀN QUA KHE TRỐNG!");}
            for(int i=0;i<columns;i++)if(i!=gap)
            {
                float radius=RandomRange(.55f,.76f);float x=Mathf.Lerp(-half,half,i/(float)(columns-1));
                SpawnWorld(WorldKind.Rock,new Vector2(x,7.8f+(crossing?i*.13f:0)),5+flowBeat,radius,new Vector2(drift,-RandomRange(3.9f+flowBeat*.35f,4.25f+flowBeat*.4f)));
            }
            escapeWave++;EscapeRockWaves++;
        }
        void SpawnEscapePursuer()
        {
            if(escapeTravel>=2400)return;
            int slot=FreeEnemy();if(slot<0)return;int lane=0;foreach(var other in enemies)if(other.active&&other.escapePursuer&&other.escapeLane==0){lane=1;break;}
            float side=lane==0?-1:1;var e=SpawnEnemy(slot,1,new Vector2(side*7.8f,-4.8f),7+flowBeat*2);
            SetEnemyVariant(e,flowBeat==2?12:7);e.escapePursuer=true;e.escapeLane=lane;e.escapeWarning=false;e.age=0;e.fire=1.15f+lane*.6f;
            e.entry=e.position=new Vector2(side*(HalfWidth+2),-5.8f);e.renderer.transform.position=e.position;e.renderer.transform.rotation=Quaternion.Euler(0,0,180);
        }
        void UpdateEscapePursuer(Enemy e,float dt)
        {
            float step=dt*EnemySlow(e.position);e.age+=step;e.motionTime+=step;e.flash=Mathf.Max(0,e.flash-dt);
            float side=e.escapeLane==0?-1:1;Vector2 target=new Vector2(side*Mathf.Min(8,HalfWidth-2)+playerPosition.x*.15f+Mathf.Sin(e.motionTime*.8f+e.escapeLane)*.45f,-4.75f+Mathf.Sin(e.motionTime*.7f)*.15f);
            float routeTime=escapeTravel/50;
            if(routeTime>=12&&routeTime<24&&e.escapeLane==0)
            {
                float sweep=Mathf.Sin(Mathf.Clamp01((routeTime-12)/12)*Mathf.PI);
                target=new Vector2(-Mathf.Min(HalfWidth-1.4f,8.5f),Mathf.Lerp(-4.75f,2.5f,sweep));
            }
            e.position=Vector2.Lerp(e.entry,target,Mathf.SmoothStep(0,1,Mathf.Clamp01(e.age/.95f)));
            e.renderer.transform.position=e.position;e.renderer.transform.rotation=Quaternion.Euler(0,0,180+Mathf.Sin(e.motionTime)*5);
            e.renderer.color=e.flash>0?new Color(1,.55f,.4f):Color.white;e.engine.transform.position=e.position+Vector2.down*.55f;e.engine.transform.localScale=new Vector3(.3f,.65f,1);
            e.halo.transform.position=e.position;e.halo.transform.localScale=Vector3.one*1.8f;e.halo.color=new Color(1,.25f,.08f,.13f);
            if(e.age<.95f)return;e.fire-=step;Vector2 muzzle=e.position+Vector2.up*.55f;
            if(!e.escapeWarning&&e.fire<=.85f)
            {
                e.escapeWarning=true;Vector2 delta=playerPosition-muzzle;e.escapeAim=delta.sqrMagnitude>.4f?delta.normalized:Vector2.up;EscapeWarningEvents++;
                hud.Toast("PHÁO TRUY KÍCH KHÓA VỊ TRÍ / ĐỔI LÀN ĐỂ NÉ!");
            }
            if(e.escapeWarning)
            {
                var line=escapeWarnings[e.escapeLane];var glow=escapeMuzzles[e.escapeLane];line.gameObject.SetActive(true);glow.gameObject.SetActive(true);
                DrawLine(line,muzzle,muzzle+e.escapeAim*14,.045f,new Color(1,.48f,.12f,.42f+Mathf.Sin(e.age*20)*.08f));
                glow.transform.position=muzzle;glow.transform.localScale=Vector3.one*(.7f+Mathf.Sin(e.age*20)*.12f);glow.color=new Color(1,.6f,.18f,.65f);
                if(e.fire<=0)
                {
                    if(routeTime>=12&&routeTime-escapeLastSalvo<.75f){e.fire=.12f;return;}
                    int count=flowBeat==2?5:3;for(int i=0;i<count;i++)
                    {
                        var shot=Bolt(muzzle,(Vector2)(Quaternion.Euler(0,0,(i-(count-1)*.5f)*10)*e.escapeAim)*(6.1f+flowBeat*.45f),false,.13f);
                        if(shot!=null)shot.damage=flowBeat==2?12:10;
                    }
                    EscapeSalvos++;escapeLastSalvo=routeTime;Play(IonArt.Impact,1.2f);e.fire=3.35f-flowBeat*.4f+RandomRange(-.15f,.25f);e.escapeWarning=false;line.gameObject.SetActive(false);glow.gameObject.SetActive(false);
                }
            }
            if((e.position-playerPosition).sqrMagnitude<.75f){HurtPlayer(20,false,"VA CHẠM TÀU");DamageEnemy(e,99);}
        }
        void OnEscapePursuerKilled(Enemy e)
        {
            EscapePursuerKills++;e.escapeWarning=false;escapeSpawnClock=Mathf.Max(escapeSpawnClock,8);
            if(escapeWarnings[e.escapeLane]!=null){escapeWarnings[e.escapeLane].gameObject.SetActive(false);escapeMuzzles[e.escapeLane].gameObject.SetActive(false);}
        }
    }
}
