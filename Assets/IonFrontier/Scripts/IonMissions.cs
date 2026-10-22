using UnityEngine;

namespace IonFrontier
{
    public sealed partial class IonGame
    {
        enum WorldKind { Rock, Mine, Generator, Beacon, Zone, Convoy, Tether, Guard }
        sealed class WorldObject
        {
            public bool active,done;public WorldKind kind;public Vector2 position,velocity;public float health,maxHealth,radius,progress,age;
            public SpriteRenderer body,ring,link;public int id;
        }
        readonly WorldObject[] world=new WorldObject[32];
        readonly SpriteRenderer[] bossOrnaments=new SpriteRenderer[8];
        SpriteRenderer pursuitSignal,collapseWall;
        float missionTime,missionSpawnClock,hazardClock,missionSupplyClock,allyClock,convoyHitClock,worldRepairClock;
        int objectiveCount,worldDestroyed,missionRescues,captureCount,route,expandedPhase,playerHits,optionalCount;
        float chaseProgress;bool missionBossStarted,optionalAwarded;
        public float MissionTime=>missionTime;
        public bool MissionBossStarted=>missionBossStarted;
        public bool RoutePending=>Sector==11&&!missionBossStarted&&route==0;
        public int MissionRoute=>route;
        public int MissionObjectives=>objectiveCount;
        public int LastStars {get;private set;}
        public int StarsAt(int stage)=>stage>=1&&stage<=profile.stars.Length?profile.stars[stage-1]:0;
        public int MissionHazardsDestroyed=>worldDestroyed;
        public bool MissionConvoyActive{get{foreach(var o in world)if(o!=null&&o.active&&o.kind==WorldKind.Convoy)return true;return false;}}
        public Vector2 ConvoyPosition{get{foreach(var o in world)if(o!=null&&o.active&&o.kind==WorldKind.Convoy)return o.position;return Vector2.zero;}}
        public float ConvoyHealth{get{foreach(var o in world)if(o!=null&&o.active&&o.kind==WorldKind.Convoy)return o.health;return 0;}}
        public string MissionTitle=>Sector<4?"":RoutePending?"HAI LỜI CẦU CỨU / CHỌN TUYẾN":missionBossStarted?"PHÁ HỦY "+BossName:"CHẶNG "+(flowBeat+1)+"/3 / "+MissionFlowName;
        public string MissionObjectiveText=>Sector<4?"":RoutePending?"E / CỨU TIẾP TẾ     R / CỨU PHI ĐỘI":missionBossStarted?BossShielded?(Sector==8?"U LINH ĐANG ẨN / TỚI ĐÈN HIỆU XANH":"LÕI ĐƯỢC CHE / PHÁ ĐỘI HỘ TỐNG HOẶC NÚT KHIÊN"):"BOSS CÒN "+ActiveBossCount+" / TẤN CÔNG ĐIỂM YẾU":MissionFlowText+"\nPHỤ: "+MissionOptionalText;
        string MissionOptionalText=>Sector==4?"PHÁ ĐÁ BẰNG HỎA LỰC "+EscapeRocksCleared+"/18":Sector==5?"VẬN TẢI ≥70%":Sector==6?"XONG TRƯỚC 95 GIÂY":Sector==7?"BÁM TÍN HIỆU 35 GIÂY":Sector==8?"CỨU ĐỦ 4 TRINH SÁT":Sector==9?"TRÚNG KHÔNG QUÁ 2 ĐÒN":Sector==10?"PHÁ 15 MÌN":Sector==11?(route==1?"VẬN TẢI ≥70%":"CỨU ĐỦ 4 TRINH SÁT"):Sector==12?"TRÚNG DƯỚI 3 ĐÒN":"XONG TRƯỚC 120 GIÂY";
        public string MissionTip=>missionBossStarted?BossHint:MissionFlowTip;
        public string DefeatReason {get;private set;}="Phi thuyền đã bị hạ.";

        void EnsureMissionObjects()
        {
            if(world[0]!=null)return;
            for(int i=0;i<world.Length;i++)
            {
                world[i]=new WorldObject{id=i,body=IonArt.Sprite("Mission object "+i,IonArt.Disc,Vector2.zero,Vector2.one,Color.white,12,transform),ring=IonArt.Sprite("Objective marker "+i,IonArt.Ring,Vector2.zero,Vector2.one,IonArt.Cyan,13,transform)};
                world[i].link=IonArt.Sprite("Tractor tether "+i,IonArt.Pixel,Vector2.zero,Vector2.one,IonArt.Orange,7,transform);world[i].link.gameObject.SetActive(false);
                world[i].body.gameObject.SetActive(false);world[i].ring.gameObject.SetActive(false);
            }
            pursuitSignal=IonArt.Sprite("Pursuit signal",IonArt.PlayerShips[1],Vector2.zero,Vector2.one,IonArt.Gold,9,transform);
            collapseWall=IonArt.Sprite("Collapse front",IonArt.Pixel,Vector2.zero,Vector2.one,Color.red,7,transform);
            for(int i=0;i<bossOrnaments.Length;i++)bossOrnaments[i]=IonArt.Sprite("Boss subsystem "+i,i%2==0?IonArt.EnemyShips[5]:IonArt.Ring,Vector2.zero,Vector2.one,Color.white,11,transform);
            HideMissionObjects();
        }
        void StartMission()
        {
            DefeatReason="Phi thuyền đã bị hạ.";playerHits=0;LastStars=0;
            if(Sector<4)return;EnsureMissionObjects();missionSpawnClock=3;hazardClock=1;missionSupplyClock=25;worldRepairClock=8;allyClock=1;
            if(Sector==5)SpawnConvoy();
            if(Sector==6||Sector==13)for(int i=0;i<3;i++)SpawnWorld(WorldKind.Generator,new Vector2((i-1)*6,2.2f),40,1.1f);
            if(Sector==8)SpawnBeacons();
            if(Sector==9)for(int i=0;i<3;i++)SpawnWorld(WorldKind.Zone,new Vector2((i-1)*6,i==1?1.5f:-2),1,1.5f);
            if(Sector==7){pursuitSignal.transform.position=new Vector2(0,4.6f);pursuitSignal.transform.localScale=Vector3.one*IonArt.ScaleFor(pursuitSignal.sprite,1.5f);pursuitSignal.gameObject.SetActive(true);}
            if(Sector==12){collapseWall.transform.position=new Vector2(0,-6.75f);collapseWall.transform.localScale=new Vector3(HalfWidth*2,2.5f,1);collapseWall.color=new Color(1,.12f,.05f,.2f);collapseWall.gameObject.SetActive(true);}
            if(Sector==4)StartEscapeMission();
            StartMissionFlow();hud.Toast(IonCampaign.Briefings[Sector-1]);
        }
        void SpawnConvoy()=>SpawnWorld(WorldKind.Convoy,new Vector2(-4,-2.6f),100,1);
        void SpawnBeacons(){for(int i=0;i<4;i++)SpawnWorld(WorldKind.Beacon,new Vector2(i%2==0?-6:6,i<2?2:-2),1,1.2f);}
        public bool ChooseRoute(int choice)
        {
            if(State!=FlightState.Playing||!RoutePending||choice<1||choice>2)return false;
            route=choice;if(choice==1)SpawnConvoy();else SpawnBeacons();EnterMissionBeat(0);
            hud.Toast(choice==1?"TUYẾN TIẾP TẾ / BẢO VỆ TÀU VẬN TẢI":"TUYẾN PHI ĐỘI / KÍCH HOẠT CÁC ĐÈN HIỆU");hud.Refresh();return true;
        }
        WorldObject SpawnWorld(WorldKind kind,Vector2 p,float health,float radius,Vector2 velocity=default)
        {
            if(kind==WorldKind.Mine&&Sector==10&&!missionBossStarted&&WorldAlive(WorldKind.Mine)>=12)return null;
            EnsureMissionObjects();foreach(var o in world)if(!o.active)
            {
                o.active=true;o.done=false;o.kind=kind;o.position=p;o.velocity=velocity;o.health=o.maxHealth=health;o.radius=radius;o.age=o.progress=0;
                o.body.sprite=kind==WorldKind.Guard?IonArt.RegularFleet[11]:IonArt.MissionProps[kind==WorldKind.Rock?0:kind==WorldKind.Mine?1:kind==WorldKind.Generator?2:kind==WorldKind.Beacon?3:kind==WorldKind.Zone?4:kind==WorldKind.Convoy?5:6];
                o.body.transform.rotation=Quaternion.identity;o.body.gameObject.SetActive(true);o.ring.gameObject.SetActive(true);DrawWorld(o);return o;
            }
            return null;
        }
        void DrawWorld(WorldObject o)
        {
            Color tint=o.kind==WorldKind.Convoy?IonArt.Cyan:o.kind==WorldKind.Rock?new Color(.55f,.45f,.36f):o.kind==WorldKind.Mine?IonArt.Orange:o.kind==WorldKind.Beacon?new Color(.4f,.7f,1):o.kind==WorldKind.Zone?new Color(.3f,1,.65f):IonArt.Gold;
            if(Sector==4&&o.kind==WorldKind.Rock)tint=IonArt.Orange;
            if(o.done)tint=new Color(.25f,1,.65f);
            if(o.kind==WorldKind.Tether&&MissionConvoyActive)DrawLine(o.link,o.position,ConvoyPosition,.045f,new Color(1,.3f,.1f,.8f));else o.link.gameObject.SetActive(false);
            o.body.transform.position=o.ring.transform.position=o.position;
            float size=o.kind==WorldKind.Convoy?2.6f:o.kind==WorldKind.Zone?o.radius*.95f:o.kind==WorldKind.Mine?o.radius*2:o.radius*1.85f;
            if(o.kind==WorldKind.Beacon&&o.done)size=1;
            o.body.transform.localScale=Vector3.one*IonArt.ScaleFor(o.body.sprite,size);o.body.color=o.done?new Color(.65f,1,.8f):Color.white;
            o.ring.transform.localScale=Vector3.one*(o.radius*2.4f+.1f*Mathf.Sin(o.age*5));
            o.ring.color=new Color(tint.r,tint.g,tint.b,o.kind==WorldKind.Zone?.65f:Sector==4&&o.kind==WorldKind.Rock?.5f:.3f);
            if(o.kind==WorldKind.Zone||o.kind==WorldKind.Beacon)o.body.color=Color.Lerp(Color.white,new Color(.5f,1,.75f),Mathf.Clamp01(o.progress/(o.kind==WorldKind.Zone?6:4)));
            if(o.kind==WorldKind.Beacon&&o.done){o.body.color=new Color(.6f,1,.8f,.7f);o.ring.color=new Color(tint.r,tint.g,tint.b,.18f);}
        }
        void ReleaseWorld(WorldObject o){o.active=false;o.body.gameObject.SetActive(false);o.ring.gameObject.SetActive(false);o.link.gameObject.SetActive(false);}
        void HideMissionObjects()
        {
            foreach(var o in world)if(o!=null){o.body.gameObject.SetActive(false);o.ring.gameObject.SetActive(false);o.link.gameObject.SetActive(false);}
            if(pursuitSignal!=null){pursuitSignal.gameObject.SetActive(false);collapseWall.gameObject.SetActive(false);}
            foreach(var sr in bossOrnaments)if(sr!=null)sr.gameObject.SetActive(false);
        }
        void HideBossOrnaments(){foreach(var sr in bossOrnaments)if(sr!=null)sr.gameObject.SetActive(false);}
        public string MissionObjectLabel(int index,out Vector2 position)
        {
            position=Vector2.zero;if(index<0)return "";
            if(index>=world.Length)
            {
                int slot=0;foreach(var e in enemies)if(e.active&&e.kind>=8)
                {
                    if(slot++!=index-world.Length)continue;
                    position=e.position+Vector2.down*1.9f;
                    string title=e.bossArt==13?"KÌM HÃM":e.bossArt==14?"LÕI VỆ":e.bossArt==15?"SONG SÁT LAM":BossName;
                    return ActiveBossCount>1?title+" / "+(BossCanTakeDamage(e)?"LÕI MỞ":"KHIÊN ĐÓNG"):"";
                }
                return "";
            }
            var o=world[index];if(o==null||!o.active)return "";
            position=o.position+Vector2.down*(o.radius+.3f);
            if(o.kind==WorldKind.Convoy)return "VẬN TẢI / "+Mathf.CeilToInt(o.health)+"%";
            if(o.kind==WorldKind.Generator||o.kind==WorldKind.Guard||o.kind==WorldKind.Tether)return (o.kind==WorldKind.Generator?"MÁY PHÁT":o.kind==WorldKind.Tether?"MÓC GIỮ":"HỘ VỆ")+" / "+Mathf.CeilToInt(o.health);
            if(o.kind==WorldKind.Beacon||o.kind==WorldKind.Zone)return o.done?"ĐÃ HOÀN TẤT":(o.kind==WorldKind.Beacon?"GIẢI CỨU ":"CHIẾM LÕI ")+Mathf.FloorToInt(o.progress/(o.kind==WorldKind.Beacon?4:6)*100)+"%";
            return "";
        }
        void ResetMission()
        {
            HideMissionObjects();foreach(var o in world)if(o!=null)o.active=false;
            missionTime=chaseProgress=0;objectiveCount=worldDestroyed=missionRescues=captureCount=route=expandedPhase=optionalCount=0;missionBossStarted=optionalAwarded=false;ResetMissionFlow();ResetEscapeMission();
        }
        void UpdateMission(float dt)
        {
            if(State!=FlightState.Playing||runResolved)return;
            if(RoutePending){if(Input.GetKeyDown(KeyCode.E))ChooseRoute(1);if(Input.GetKeyDown(KeyCode.R))ChooseRoute(2);return;}
            if(!missionBossStarted)missionTime+=dt;
            convoyHitClock=Mathf.Max(0,convoyHitClock-dt);allyClock-=dt;worldRepairClock-=dt;
            foreach(var o in world)
            {
                if(State!=FlightState.Playing||runResolved)return;
                if(!o.active)continue;o.age+=dt;Vector2 old=o.position;o.position+=o.velocity*dt;
                if(o.kind==WorldKind.Rock||o.kind==WorldKind.Mine)
                {
                    o.body.transform.Rotate(0,0,dt*(o.id%2==0?25:-25));
                    if((o.position-playerPosition).sqrMagnitude<(o.radius+.25f)*(o.radius+.25f)){HurtPlayer(o.kind==WorldKind.Mine?26:18,false,o.kind==WorldKind.Mine?"MÌN":"THIÊN THẠCH");if(State!=FlightState.Playing)return;DamageWorld(o,50,false);}
                    if(o.active&&(o.position.y<-7.8f||Mathf.Abs(o.position.x)>HalfWidth+2)){ReleaseWorld(o);continue;}
                }
                else if(o.kind==WorldKind.Convoy)
                {
                    o.position=new Vector2(-4+Mathf.Sin(missionTime*.13f)*2,-2.3f+Mathf.Sin(missionTime*.21f)*.5f);
                    if(worldRepairClock<=0)o.health=Mathf.Min(100,o.health+2);
                }
                else if((o.kind==WorldKind.Beacon||o.kind==WorldKind.Zone)&&!o.done)
                {
                    if(o.kind==WorldKind.Beacon&&RescueFlow&&flowBeat==1&&MissionMarkedAlive>0)continue;
                    if((o.position-playerPosition).sqrMagnitude<o.radius*o.radius){bool contested=false;foreach(var e in enemies)if(e.active&&e.kind<8&&(e.position-o.position).sqrMagnitude<6){contested=true;break;}o.progress+=dt*(contested?.4f:1);}
                    float target=o.kind==WorldKind.Zone?6:4;
                    if(o.progress>=target)
                    {
                        o.done=true;objectiveCount++;if(o.kind==WorldKind.Beacon)missionRescues++;else captureCount++;
                        Ring(o.position,IonArt.Cyan,3,.7f);SpawnCell(o.position,objectiveCount%2==0?PickupKind.Damage:PickupKind.Ammo);
                        hud.Toast((o.kind==WorldKind.Zone?"ĐÃ CHIẾM LÕI ":"ĐÃ CỨU TRINH SÁT ")+objectiveCount);
                    }
                }
                if(o.active)DrawWorld(o);
            }
            if(worldRepairClock<=0)
            {
                worldRepairClock=8;
                if(!missionBossStarted)foreach(var healer in enemies)if(healer.active&&healer.kind==4&&!EncounterOwns(healer))
                {foreach(var e in enemies)if(e.active&&e.kind<8&&(e.position-healer.position).sqrMagnitude<12)e.health=Mathf.Min(e.maxHealth,e.health+1);Ring(healer.position,IonArt.Gold,2,.5f);}
            }
            if(allyClock<=0)
            {
                allyClock=1.8f;
                int allies=missionRescues+(MissionConvoyActive&&ConvoyHealth>=70?1:0);
                foreach(var e in enemies)if(e.active&&e.age>1&&allies>0)
                {
                    Vector2 origin=MissionConvoyActive?ConvoyPosition:playerPosition+Vector2.right*2;
                    foreach(var o in world)if(o.active&&o.done&&o.kind==WorldKind.Beacon){origin=o.position;break;}
                    var shot=Bolt(origin,(e.position-origin).normalized*18,true);if(shot!=null){shot.damage=Mathf.Min(4,allies)*1.5f;shot.homing=true;shot.renderer.color=IonArt.Cyan;}Ring(origin,IonArt.Cyan,.8f,.25f);break;
                }
            }
            if(Sector==7&&!missionBossStarted)
            {
                pursuitSignal.transform.position=new Vector2(Mathf.Sin(missionTime*.25f)*6,4.6f);pursuitSignal.transform.localScale=Vector3.one*IonArt.ScaleFor(pursuitSignal.sprite,1.5f);
                if(Mathf.Abs(playerPosition.x-pursuitSignal.transform.position.x)<2.5f&&playerPosition.y>-4.7f)chaseProgress+=dt;
            }
            if(Sector==12&&!missionBossStarted)
            {
                float front=-5.8f+flowBeat*.4f+Mathf.Sin(missionTime*.14f)*.9f;
                collapseWall.transform.position=new Vector2(0,(front-8)*.5f);collapseWall.transform.localScale=new Vector3(HalfWidth*2,front+8,1);collapseWall.color=new Color(1,.12f,.05f,.2f+.05f*Mathf.Sin(missionTime*12));
                if(playerPosition.y<front)HurtPlayer(22*dt,true,"VÙNG SỤP ĐỔ");
            }
        }
        void UpdateMissionDirector(float dt)
        {
            if(TestFreezeEnemies||RoutePending)return;
            if(missionBossStarted){EmergencyMissionAmmo(dt);return;}
            if(Sector==4){TickEscapeMission(dt);return;}
            TickMissionFlow(dt);if(missionBossStarted||State!=FlightState.Playing)return;
            missionSpawnClock-=dt;hazardClock-=dt;missionSupplyClock-=dt;
            if(missionSpawnClock<=0){if(!TrySpawnEncounter())SpawnMissionSquad();missionSpawnClock=flowBeat==2?9:12;Wave++;}
            if(hazardClock<=0){SpawnMissionHazards();hazardClock=Sector==7?2.6f:Sector==10||Sector==12?3.8f:9;}
            if(missionSupplyClock<=0)
            {
                missionSupplyClock=32;SpawnCell(new Vector2(playerPosition.x,3.5f),PickupKind.Ammo);
                SpawnCell(new Vector2(-playerPosition.x,2.5f),(PickupKind)((int)PickupKind.Damage+(Wave%3)));
                foreach(var o in world)if(o.active&&o.kind==WorldKind.Convoy)o.health=Mathf.Min(100,o.health+12);
                hud.Toast("ĐIỂM TIẾP TẾ / ĐẠN, NÂNG CẤP VÀ SỬA TÀU");
            }
            EmergencyMissionAmmo(dt);
        }
        void EmergencyMissionAmmo(float dt)
        {
            if(Ammo>=35)return;supplyClock-=dt;if(supplyClock>0)return;supplyClock=missionBossStarted?14:18;
            foreach(var c in cells)if(c.active&&c.kind==PickupKind.Ammo)return;
            SpawnCell(new Vector2(Mathf.Clamp(playerPosition.x,-8,8),1.5f),PickupKind.Ammo);
        }
        void SpawnMissionSquad()
        {
            int count=flowBeat==0?6:flowBeat==1?5:7;if(ActiveEnemies>18)return;
            for(int n=0;n<count;n++)
            {
                int slot=FreeEnemy();if(slot<0)return;int variant=IonHostileFleet.Variant(Sector,n+Wave+flowBeat);
                Vector2 target=flowBeat==1?new Vector2((n%2==0?-1:1)*(5+n*.3f),4-n*.6f):flowBeat==2?new Vector2((n-(count-1)*.5f)*2,4.5f-Mathf.Abs(n-(count-1)*.5f)*.55f):new Vector2((n-(count-1)*.5f)*2.5f,4.5f);
                var e=SpawnEnemy(slot,1,target,IonHostileFleet.Health[variant]+Mathf.Min(2,Sector/5));SetEnemyVariant(e,variant);
                if(flowBeat==1)e.entry=new Vector2((n%2==0?-1:1)*(HalfWidth+2),target.y+1);
            }
        }
        int FreeEnemy(){for(int i=0;i<enemies.Length;i++)if(!enemies[i].active&&!enemies[i].dying)return i;return -1;}
        void SpawnMissionHazards()
        {
            if(Sector==7||Sector==12)
            {
                int gap=(int)Mathf.Clamp(Mathf.Round(playerPosition.x/4.3f)+2,0,4);
                for(int i=0;i<5;i++)if(i!=gap)SpawnWorld(WorldKind.Rock,new Vector2((i-2)*4.3f,7.5f),Sector==7?4:7,.6f,new Vector2(Mathf.Sin(missionTime)*.15f,Sector==7?-3.3f:-1.6f));
            }
            else if(Sector==10)
            {int mines=0;foreach(var o in world)if(o.active&&o.kind==WorldKind.Mine)mines++;for(int i=0;i<5&&mines<12;i++)if(SpawnWorld(WorldKind.Mine,new Vector2((i-2)*2.2f,7.3f),3,.45f,Vector2.down*1.1f)!=null)mines++;}
            else if(Sector==6||Sector==13)
            {foreach(var o in world)if(o.active&&o.kind==WorldKind.Generator){if(o.id%3==0)Fan(o.position,(playerPosition-o.position).normalized,3,16,3.4f);else if(o.id%3==1)Fan(o.position,Vector2.down,5,22,3);else SpawnBeam(new Vector2(playerPosition.x,5));}}
        }
        bool HitMissionObject(Projectile b,Vector2 old)
        {
            foreach(var o in world)
            {
                if(!o.active)continue;
                bool destructible=o.kind==WorldKind.Rock||o.kind==WorldKind.Mine||o.kind==WorldKind.Generator||o.kind==WorldKind.Tether||o.kind==WorldKind.Guard;
                if(b.friendly&&!destructible||!b.friendly&&o.kind!=WorldKind.Convoy)continue;
                ulong bit=1UL<<o.id;if((b.worldHitMask&bit)!=0)continue;
                if(SegmentDistance(old,b.position,o.position)>(o.radius+b.radius)*(o.radius+b.radius))continue;
                b.worldHitMask|=bit;
                if(b.friendly)DamageWorld(o,b.damage);else DamageConvoy(o,b.damage*.33f);
                if(--b.pierce<=0){Release(b);return true;}
            }
            return false;
        }
        void DamageConvoy(WorldObject o,float amount)
        {
            if(convoyHitClock>0||!o.active)return;convoyHitClock=.5f;o.health-=amount;Ring(o.position,IonArt.Orange,2,.2f);
            if(o.health<=0){DefeatReason="Tàu vận tải đã bị phá hủy. Hãy ưu tiên bảo vệ đoàn tàu.";Explosion(o.position,2);ReleaseWorld(o);Finish(false);}
        }
        void DamageWorld(WorldObject o,float damage,bool playerAttack=true)
        {
            if(State!=FlightState.Playing||runResolved||!o.active)return;o.health-=damage;if(o.health>0){Emit(o.position,Vector2.zero,IonArt.Gold,.1f,.3f);return;}
            Vector2 p=o.position;WorldKind kind=o.kind;ReleaseWorld(o);Explosion(p,kind==WorldKind.Mine?1.8f:.8f);
            if(kind==WorldKind.Generator){objectiveCount++;hud.Toast("ĐÃ PHÁ MÁY PHÁT / "+objectiveCount+"/3");}
            if(kind==WorldKind.Rock||kind==WorldKind.Mine)
            {
                worldDestroyed++;if(Sector==4&&kind==WorldKind.Rock&&playerAttack)EscapeRocksCleared++;
                CounterAreaDamage(p,kind==WorldKind.Mine?4:Mathf.Sqrt(5),kind==WorldKind.Mine?18:6);
                foreach(var e in enemies)if(e.active&&(e.position-p).sqrMagnitude<(kind==WorldKind.Mine?16:5))DamageEnemy(e,kind==WorldKind.Mine?18:6);
                if(kind==WorldKind.Mine)foreach(var other in world)if(other.active&&(other.kind==WorldKind.Mine||other.kind==WorldKind.Generator)&&(other.position-p).sqrMagnitude<7)DamageWorld(other,other.kind==WorldKind.Mine?50:12,playerAttack);
            }
        }
        void UltimateWorldPulse(float damage)
        {
            if(Sector<4)return;foreach(var o in world)if(o!=null&&o.active&&(o.kind==WorldKind.Rock||o.kind==WorldKind.Mine||o.kind==WorldKind.Generator||o.kind==WorldKind.Tether||o.kind==WorldKind.Guard))DamageWorld(o,damage);
        }
        void RepairConvoy(){foreach(var o in world)if(o!=null&&o.active&&o.kind==WorldKind.Convoy)o.health=Mathf.Min(100,o.health+15);}
        bool OptionalGoal=>Sector==4?EscapeRocksCleared>=18:Sector==5?ConvoyHealth>=70:Sector==6||Sector==13?missionTime<=(Sector==13?120:95):Sector==7?chaseProgress>=35:Sector==8?missionRescues==4:Sector==9?playerHits<=2:Sector==10?worldDestroyed>=15:Sector==11?route==1?ConvoyHealth>=70:missionRescues==4:Sector==12?playerHits<3:false;
        void CompleteMissionRewards()
        {
            CompleteRating();
            if(Sector>=4&&OptionalGoal&&!optionalAwarded){optionalAwarded=true;Score+=1500;}
        }
        Vector2 MissionTestAim()
        {
            if(RoutePending)return playerPosition;
            if(Sector==4&&!missionBossStarted)return new Vector2(Mathf.Sin(missionTime*.8f)*6,-1.8f);
            foreach(var o in world)if(o!=null&&o.active&&!o.done&&(o.kind==WorldKind.Beacon||o.kind==WorldKind.Zone))return o.position;
            foreach(var o in world)if(o!=null&&o.active&&o.kind==WorldKind.Generator)return new Vector2(o.position.x,-2);
            if(Sector==7)return new Vector2(Mathf.Sin(missionTime*.25f)*6,-2.5f);
            if(Sector==12)return new Vector2(Mathf.Sin(missionTime*.2f)*5,-2);
            Enemy target=null;foreach(var e in enemies)if(e.active){target=e;break;}return new Vector2(target==null?0:target.position.x,-4);
        }
    }
}
