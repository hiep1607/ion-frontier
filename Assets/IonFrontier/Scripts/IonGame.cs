using System;
using UnityEngine;

namespace IonFrontier
{
    public enum FlightState { Menu, Playing, Paused, Defeat, Victory }

    public sealed class IonGame : MonoBehaviour
    {
        public FlightState State { get; private set; }
        public int Score { get; private set; }
        public int Best { get; private set; }
        public int Shield { get; private set; } = 5;
        public int WeaponTier { get; private set; } = 1;
        public int Sector { get; private set; } = 1;
        public int Kills { get; private set; }
        public bool AutoFire { get; private set; } = true;
        public bool Muted { get; private set; }
        public float DashReady => 1-Mathf.Clamp01(dashCooldown/2.4f);
        public float BossFraction => boss == null || !boss.active ? 0 : Mathf.Clamp01(boss.health/280f);
        public float Elapsed { get; private set; }
        public int ActiveEnemies { get { int n=0; foreach(var e in enemies) if(e.active)n++; return n; } }
        public int ActiveBullets { get { int n=0; foreach(var b in bullets) if(b.active)n++; return n; } }
        public Vector2 PlayerPosition => playerPosition;
        public float HalfWidth { get; private set; }
        public bool TestInput;
        public Vector2 TestTarget;
        public bool TestFire = true;
        public bool TestDashRequest;
        public bool ScoreFileExists => !string.IsNullOrEmpty(bestPath) && System.IO.File.Exists(bestPath);

        sealed class Projectile
        {
            public bool active, friendly; public Vector2 position, velocity; public float radius, life; public int damage;
            public SpriteRenderer renderer, halo;
        }
        sealed class Enemy
        {
            public bool active; public int kind, health, index; public float age, fire, flash;
            public Vector2 position, target, entry; public SpriteRenderer renderer, engine, halo;
        }
        sealed class Spark
        {
            public bool active, ring; public Vector2 position, velocity; public float life, maxLife, size;
            public Color color; public SpriteRenderer renderer;
        }
        sealed class Cell
        {
            public bool active; public Vector2 position; public float age; public SpriteRenderer renderer, halo;
        }

        const int BulletCapacity=320, EnemyCapacity=40, SparkCapacity=220;
        readonly Projectile[] bullets=new Projectile[BulletCapacity];
        readonly Enemy[] enemies=new Enemy[EnemyCapacity];
        readonly Spark[] sparks=new Spark[SparkCapacity];
        readonly Cell[] cells=new Cell[12];
        readonly string[] waveNames={"VÀNH ĐAI NGOÀI", "PHI ĐỘI ĐÁNH CHẶN", "ĐỘI HÌNH ĐỘT PHÁ", "PHONG TỎA QUỸ ĐẠO", "THIẾT GIÁP HẠM"};
        Camera view; IonHud hud; Enemy boss;
        SpriteRenderer player, playerHalo, shieldRing, leftEngine, rightEngine;
        AudioSource music; AudioSource[] voices; int voiceIndex;
        Vector2 playerPosition, playerVelocity, dashDirection;
        Vector3 lastMouse;
        float fireClock, dashCooldown, dashTime, invincible, shake, waveTime, transition, exhaustClock, soundClock;
        bool keyboardMode, waveLive;
        float bannerTimer;
        System.Random random;
        string bestPath;
        public string SectorName => waveNames[Mathf.Clamp(Sector-1,0,4)];

        void Awake()
        {
            Application.targetFrameRate=120; QualitySettings.vSyncCount=1;
            Time.fixedDeltaTime=1f/60; random=new System.Random(479);
            IonArt.Initialize(); view=Camera.main;
            if(view==null) { var go=new GameObject("Flight camera"); view=go.AddComponent<Camera>(); go.tag="MainCamera"; go.AddComponent<AudioListener>(); }
            view.orthographic=true; view.orthographicSize=7; view.transform.position=new Vector3(0,0,-10);
            view.clearFlags=CameraClearFlags.SolidColor; view.backgroundColor=IonArt.Ink;
            HalfWidth=Mathf.Min(13.4f,7*view.aspect);
            var backdrop=new GameObject("Parallax starfield").AddComponent<IonBackdrop>(); backdrop.Initialize(HalfWidth);
            CreatePools(); CreatePlayer(); CreateAudio();
            bool playtest=Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-playtest")>=0;
            bestPath=System.IO.Path.Combine(Application.persistentDataPath,playtest?"playtest-best.txt":"best-score.txt");
            try { if(System.IO.File.Exists(bestPath)&&int.TryParse(System.IO.File.ReadAllText(bestPath),out int saved))Best=saved; } catch(System.IO.IOException) { Best=0; }
            hud=new GameObject("Flight interface").AddComponent<IonHud>(); hud.Initialize(this);
            State=FlightState.Menu; SetPlayerVisible(false); hud.Refresh();
            if(playtest) { Application.runInBackground=true; gameObject.AddComponent<IonPlaytest>(); }
        }

        void CreatePools()
        {
            var pool=new GameObject("Reusable combat pool").transform;
            for(int i=0;i<bullets.Length;i++)
            {
                var sr=IonArt.Sprite("Energy bolt",IonArt.Pixel,Vector2.zero,Vector2.one,Color.white,15,pool);
                var halo=IonArt.Sprite("Bolt bloom",IonArt.Glow,Vector2.zero,Vector2.one,Color.white,14,sr.transform);
                bullets[i]=new Projectile{renderer=sr,halo=halo}; sr.gameObject.SetActive(false);
            }
            for(int i=0;i<enemies.Length;i++)
            {
                var sr=IonArt.Sprite("Hostile spacecraft",IonArt.Ships[1],Vector2.zero,Vector2.one,Color.white,10,pool);
                var engine=IonArt.Sprite("Hostile thruster",IonArt.Glow,Vector2.zero,Vector2.one,IonArt.Orange,9,pool);
                var halo=IonArt.Sprite("Target energy",IonArt.Glow,Vector2.zero,Vector2.one,IonArt.Orange,8,pool);
                enemies[i]=new Enemy{renderer=sr,engine=engine,halo=halo};
                sr.gameObject.SetActive(false); engine.gameObject.SetActive(false); halo.gameObject.SetActive(false);
            }
            for(int i=0;i<sparks.Length;i++)
            {
                var sr=IonArt.Sprite("Pooled impact",IonArt.Glow,Vector2.zero,Vector2.one,Color.white,25,pool);
                sparks[i]=new Spark{renderer=sr}; sr.gameObject.SetActive(false);
            }
            for(int i=0;i<cells.Length;i++)
            {
                var sr=IonArt.Sprite("Energy core",IonArt.Disc,Vector2.zero,Vector2.one*.2f,IonArt.Gold,20,pool);
                var halo=IonArt.Sprite("Core halo",IonArt.Ring,Vector2.zero,Vector2.one*.6f,IonArt.Gold,19,pool);
                cells[i]=new Cell{renderer=sr,halo=halo}; sr.gameObject.SetActive(false); halo.gameObject.SetActive(false);
            }
        }

        void CreatePlayer()
        {
            player=IonArt.Sprite("Vanguard interceptor",IonArt.Ships[0],Vector2.zero,Vector2.one*1.95f,Color.white,20,transform);
            playerHalo=IonArt.Sprite("Ion drive glow",IonArt.Glow,Vector2.zero,new Vector2(1.6f,1),new Color(.1f,.65f,1,.35f),17,transform);
            shieldRing=IonArt.Sprite("Invulnerability field",IonArt.Ring,Vector2.zero,Vector2.one*1.7f,IonArt.Cyan,22,transform);
            leftEngine=IonArt.Sprite("Port ion exhaust",IonArt.Glow,Vector2.zero,new Vector2(.25f,.75f),IonArt.Cyan,19,transform);
            rightEngine=IonArt.Sprite("Starboard ion exhaust",IonArt.Glow,Vector2.zero,new Vector2(.25f,.75f),IonArt.Cyan,19,transform);
        }

        void CreateAudio()
        {
            music=gameObject.AddComponent<AudioSource>(); music.clip=IonArt.Music; music.loop=true; music.volume=.7f; music.Play();
            voices=new AudioSource[10]; for(int i=0;i<voices.Length;i++){ voices[i]=gameObject.AddComponent<AudioSource>(); voices[i].playOnAwake=false; voices[i].volume=.6f; }
        }

        public void BeginRun()
        {
            ResetCombat(); Score=0; Shield=5; Kills=0; WeaponTier=1; Sector=1; Elapsed=0;
            playerPosition=new Vector2(0,-4.7f); playerVelocity=Vector2.zero; fireClock=0; dashCooldown=0; invincible=2;
            lastMouse=Input.mousePosition; keyboardMode=false; waveTime=0; transition=1.8f; bannerTimer=2.8f;
            State=FlightState.Playing; SetPlayerVisible(true); hud.Refresh(); Play(IonArt.Pickup); UpdatePlayerArt(0);
        }

        public void ReturnToMenu()
        {
            SaveBest(); ResetCombat(); State=FlightState.Menu; SetPlayerVisible(false); hud.Refresh();
        }
        public void TogglePause()
        {
            if(State==FlightState.Playing) State=FlightState.Paused;
            else if(State==FlightState.Paused) {State=FlightState.Playing; lastMouse=Input.mousePosition;}
            hud.Refresh();
        }
        public void ToggleAudio(){Muted=!Muted; music.mute=Muted; foreach(var voice in voices) voice.mute=Muted; hud.Refresh();}
        public void Quit(){ SaveBest(); Application.Quit(); }
        public void ToggleAuto(){AutoFire=!AutoFire;hud.Refresh();}
        void Play(AudioClip clip,float pitch=1)
        {
            if(Muted) return; var a=voices[voiceIndex++%voices.Length]; a.pitch=pitch; a.clip=clip; a.Play();
        }
        void SaveBest()
        {
            if(Score<=Best)return;Best=Score;
            try { System.IO.Directory.CreateDirectory(Application.persistentDataPath);System.IO.File.WriteAllText(bestPath,Best.ToString()); }
            catch(System.IO.IOException e){Debug.LogWarning("Score kept for this session: "+e.Message);}
            catch(UnauthorizedAccessException e){Debug.LogWarning("Score kept for this session: "+e.Message);}
        }
        void OnApplicationQuit(){ SaveBest(); }
        void OnApplicationFocus(bool focus){if(!focus&&State==FlightState.Playing&&!TestInput)TogglePause();}

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.M)) ToggleAudio();
            if(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.P)) TogglePause();
            if(State==FlightState.Menu && Input.GetKeyDown(KeyCode.Return)) BeginRun();
            if((State==FlightState.Defeat||State==FlightState.Victory)&&Input.GetKeyDown(KeyCode.R)) BeginRun();
            if(State==FlightState.Playing&&Input.GetKeyDown(KeyCode.F)) ToggleAuto();
            float realDt=Mathf.Min(Time.unscaledDeltaTime,.05f);
            if(State==FlightState.Paused) { view.transform.position=new Vector3(0,0,-10); return; }
            UpdateSparks(realDt);
            if(State!=FlightState.Playing) { view.transform.position=new Vector3(0,0,-10); return; }
            float dt=Mathf.Min(Time.deltaTime,.04f); Elapsed+=dt; waveTime+=dt;
            invincible=Mathf.Max(0,invincible-dt); dashCooldown=Mathf.Max(0,dashCooldown-dt); soundClock-=dt;
            bannerTimer=Mathf.Max(0,bannerTimer-dt); hud.ShowBanner(bannerTimer>0, Sector, SectorName);
            UpdatePlayer(dt); UpdateEnemies(dt); UpdateBullets(dt); UpdateCells(dt);
            if(!waveLive)
            {
                transition-=dt; if(transition<=0) SpawnWave();
            }
            else if(ActiveEnemies==0)
            {
                waveLive=false; boss=null; ClearHostileBullets();
                if(Sector==5) Finish(true);
                else { Sector++; WeaponTier=Mathf.Min(3,WeaponTier+1); Shield=Mathf.Min(5,Shield+1); transition=2; waveTime=0; bannerTimer=3; Play(IonArt.Pickup); hud.Refresh(); }
            }
            shake=Mathf.MoveTowards(shake,0,dt*.8f);
            view.transform.position=new Vector3(Mathf.Sin(Elapsed*83)*shake,Mathf.Sin(Elapsed*67)*shake*.65f,-10);
            hud.Tick(realDt);
        }

        void UpdatePlayer(float dt)
        {
            Vector2 input=new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
            if(input.sqrMagnitude>1) input.Normalize();
            Vector2 desired=Vector2.zero; Vector3 mouse=Input.mousePosition;
            bool moved=(mouse-lastMouse).sqrMagnitude>2;
            if(TestInput)
            {
                desired=Vector2.ClampMagnitude((TestTarget-playerPosition)*7,10.5f);
            }
            else if(input.sqrMagnitude>.01f) {keyboardMode=true; desired=input*10.5f;}
            else
            {
                if(moved) keyboardMode=false;
                if(!keyboardMode)
                {
                    Vector2 target=view.ScreenToWorldPoint(new Vector3(mouse.x,mouse.y,10));
                    // Keep the ship where it is until the pilot actually moves the mouse.
                    if(moved||playerVelocity.sqrMagnitude>.02f) desired=Vector2.ClampMagnitude((ClampPlayer(target)-playerPosition)*14,15);
                }
            }
            lastMouse=mouse;
            bool dashPressed=Input.GetKeyDown(KeyCode.LeftShift)||Input.GetKeyDown(KeyCode.RightShift)||Input.GetMouseButtonDown(1)||(TestInput&&TestDashRequest);
            TestDashRequest=false;
            if(dashPressed && dashCooldown<=0)
            {
                dashDirection=desired.sqrMagnitude>.1f?desired.normalized:Vector2.up;
                dashTime=.16f; dashCooldown=2.4f; invincible=Mathf.Max(invincible,.38f); Play(IonArt.Dash); Ring(playerPosition,IonArt.Cyan,.45f,.4f);
            }
            if(dashTime>0){dashTime-=dt;playerVelocity=dashDirection*27;}
            else playerVelocity=Vector2.Lerp(playerVelocity,desired,1-Mathf.Exp(-18*dt));
            playerPosition=ClampPlayer(playerPosition+playerVelocity*dt);
            fireClock-=dt;
            if((TestInput?TestFire:(AutoFire||Input.GetKey(KeyCode.Space)||Input.GetMouseButton(0)))&&fireClock<=0)
            {
                FirePlayer(); fireClock=.13f;
            }
            UpdatePlayerArt(dt);
            exhaustClock-=dt;
            if(exhaustClock<=0)
            {
                exhaustClock=.035f;
                Emit(playerPosition+new Vector2(-.24f,-.5f),new Vector2(RandomRange(-.1f,.1f),-1.9f),IonArt.Cyan,.22f,.18f);
                Emit(playerPosition+new Vector2(.24f,-.5f),new Vector2(RandomRange(-.1f,.1f),-1.9f),IonArt.Cyan,.22f,.18f);
            }
        }
        Vector2 ClampPlayer(Vector2 p) => new Vector2(Mathf.Clamp(p.x,-HalfWidth+.75f,HalfWidth-.75f),Mathf.Clamp(p.y,-5.6f,1.4f));
        void UpdatePlayerArt(float dt)
        {
            player.transform.position=playerPosition;
            player.transform.rotation=Quaternion.Euler(0,0,Mathf.Clamp(-playerVelocity.x*1.5f,-13,13));
            player.color=invincible>0?Color.Lerp(Color.white,IonArt.Cyan,.2f+Mathf.Sin(Elapsed*28)*.15f):Color.white;
            playerHalo.transform.position=playerPosition+Vector2.down*.35f;
            leftEngine.transform.position=playerPosition+new Vector2(-.24f,-.53f);
            rightEngine.transform.position=playerPosition+new Vector2(.24f,-.53f);
            float flame=.65f+Mathf.Sin(Elapsed*48)*.09f+(dashTime>0?.6f:0);
            leftEngine.transform.localScale=rightEngine.transform.localScale=new Vector3(.28f,flame,1);
            shieldRing.transform.position=playerPosition; shieldRing.enabled=invincible>0;
            shieldRing.color=new Color(.25f,.85f,1,.2f+Mathf.Sin(Elapsed*8)*.1f);
        }
        void SetPlayerVisible(bool visible)
        {
            player.enabled=visible;playerHalo.enabled=visible;leftEngine.enabled=visible;rightEngine.enabled=visible;shieldRing.enabled=visible;
        }

        void FirePlayer()
        {
            Vector2 muzzle=playerPosition+Vector2.up*.57f;
            if(WeaponTier==1)
            {
                Bolt(muzzle+Vector2.left*.19f,Vector2.up*23,true); Bolt(muzzle+Vector2.right*.19f,Vector2.up*23,true);
            }
            else
            {
                int n=WeaponTier==2?3:5;
                for(int i=0;i<n;i++){float a=(i-(n-1)*.5f)*.09f;Bolt(muzzle+new Vector2(a*.8f,0),new Vector2(Mathf.Sin(a),Mathf.Cos(a))*23,true);}
            }
            Emit(muzzle,Vector2.up*.3f,IonArt.Cyan,.12f,.28f);
            if(soundClock<=0){Play(IonArt.Laser,RandomRange(.94f,1.06f));soundClock=.1f;}
        }

        void SpawnWave()
        {
            waveLive=true; waveTime=0; boss=null;
            if(Sector==5){boss=SpawnEnemy(0,3,new Vector2(0,1.9f),280);hud.Refresh();return;}
            int count=Sector==2?22:24;
            for(int i=0;i<count;i++)
            {
                Vector2 target;
                if(Sector==3)
                {
                    int j=i%12; float x=(j-5.5f)*1.5f; target=new Vector2(x,4.9f-Mathf.Abs(x)*.27f-(i/12)*1.5f);
                }
                else if(Sector==4)
                {
                    float a=i*Mathf.PI*2/count; target=new Vector2(Mathf.Cos(a)*7.1f,Mathf.Sin(a)*1.7f+3.1f);
                }
                else target=new Vector2((i%8-3.5f)*2.1f,4.75f-(i/8)*1.25f);
                int kind=(Sector>=2&&i%3==0)?2:1;
                SpawnEnemy(i,kind,target,kind==2?4:2);
            }
        }
        Enemy SpawnEnemy(int i,int kind,Vector2 target,int hp)
        {
            var e=enemies[i]; e.active=true;e.kind=kind;e.health=hp;e.target=target;e.position=target+Vector2.up*8;e.entry=e.position;
            e.index=i;e.age=-i*.045f;e.fire=RandomRange(1.6f,4.8f);e.flash=0;
            e.renderer.sprite=IonArt.Ships[kind];e.renderer.color=Color.white;e.renderer.gameObject.SetActive(true);
            e.engine.gameObject.SetActive(true);e.halo.gameObject.SetActive(true);
            e.renderer.transform.localScale=Vector3.one*(kind==3?4.9f:1.65f);
            e.engine.color=kind==2?IonArt.Gold:IonArt.Orange;
            return e;
        }
        void UpdateEnemies(float dt)
        {
            foreach(var e in enemies)
            {
                if(!e.active) continue;
                e.age+=dt; e.flash=Mathf.Max(0,e.flash-dt);
                Vector2 target=e.target;
                if(e.kind==3) target+=new Vector2(Mathf.Sin(waveTime*.53f)*4.5f,Mathf.Cos(waveTime*.8f)*.35f);
                else if(Sector==2) target+=new Vector2(Mathf.Sin(waveTime*1.1f+e.index*.19f)*1.6f,Mathf.Sin(waveTime*1.3f+e.index*.25f)*.5f);
                else if(Sector==4)
                {
                    float a=e.index*Mathf.PI*2/24+waveTime*.24f;target=new Vector2(Mathf.Cos(a)*7.1f,Mathf.Sin(a)*1.7f+3.1f);
                }
                else target+=new Vector2(Mathf.Sin(waveTime*.65f)*1.25f,Mathf.Sin(waveTime*.9f)*.2f);
                if(Sector==3 && e.age>4)
                {
                    float dive=(e.age-4+e.index*.7f)%11;
                    if(dive<3)target+=new Vector2(Mathf.Sin(dive/3*Mathf.PI*2)*2,-Mathf.Sin(dive/3*Mathf.PI)*7);
                }
                float entry=Mathf.Clamp01(e.age/1.35f);entry=entry*entry*(3-2*entry);
                Vector2 previous=e.position;e.position=Vector2.Lerp(e.entry,target,entry);
                e.renderer.transform.position=e.position;
                float tilt=Mathf.Clamp((e.position.x-previous.x)/Mathf.Max(dt,.001f)*-2,-16,16);
                e.renderer.transform.rotation=Quaternion.Euler(0,0,tilt);
                e.renderer.color=e.flash>0?new Color(1,.55f,.4f):Color.white;
                e.engine.transform.position=e.position+Vector2.up*(e.kind==3?1.45f:.43f);
                e.engine.transform.localScale=e.kind==3?new Vector3(2,.8f,1):new Vector3(.4f,.62f+Mathf.Sin(e.age*20)*.07f,1);
                e.halo.transform.position=e.position;e.halo.transform.localScale=Vector3.one*(e.kind==3?5.1f:1.8f);
                e.halo.color=new Color(1,.2f,.05f,e.kind==3?.15f:.055f);
                if(e.age<1.7f) continue;
                e.fire-=dt;
                if(e.kind==3)
                {
                    if(e.fire<.45f) e.halo.color=new Color(1,.35f,.1f,.23f+Mathf.Sin(e.age*30)*.06f);
                    if(e.fire<=0){BossAttack(e);e.fire=e.health<140?.9f:1.35f;}
                }
                else if(e.fire<=0)
                {
                    e.fire=RandomRange(3.8f,7)-Sector*.2f;
                    if(e.position.y>-.6f)
                    {
                        Vector2 dir=e.kind==2?(playerPosition-e.position).normalized:Vector2.down;
                        Bolt(e.position+Vector2.down*.42f,dir*(e.kind==2?5.6f:4.6f),false,e.kind==2?.16f:.13f);
                        Emit(e.position,Vector2.zero,IonArt.Orange,.15f,.32f);
                    }
                }
                if((e.position-playerPosition).sqrMagnitude<(e.kind==3?3.1f:.75f) && invincible<=0)
                {
                    HurtPlayer(); if(e.kind!=3) DamageEnemy(e,99);
                }
            }
        }
        void BossAttack(Enemy e)
        {
            int phase=(int)(e.age/5)%3; Vector2 origin=e.position+Vector2.down*1.1f;
            if(phase==0)
            {
                int n=e.health<140?11:7;for(int i=0;i<n;i++){float a=(i-(n-1)*.5f)*.18f;Bolt(origin,new Vector2(Mathf.Sin(a),-Mathf.Cos(a))*5.2f,false,.16f);}
            }
            else if(phase==1)
            {
                Vector2 aim=(playerPosition-origin).normalized;
                for(int i=-1;i<=1;i++){Vector2 d=Quaternion.Euler(0,0,i*13)*aim;Bolt(origin+Vector2.right*i*.9f,d*6.2f,false,.18f);}
            }
            else for(int i=0;i<12;i++){float a=i*Mathf.PI*2/12+e.age*.35f;Bolt(origin,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*4.5f,false,.15f);}
            Ring(origin,IonArt.Orange,.7f,.25f);Play(IonArt.Impact,.65f);
        }

        void Bolt(Vector2 position,Vector2 velocity,bool friendly,float radius=.1f)
        {
            foreach(var b in bullets)
            {
                if(b.active)continue;
                b.active=true;b.friendly=friendly;b.position=position;b.velocity=velocity;b.radius=radius;b.damage=1;b.life=6;
                b.renderer.sprite=friendly?IonArt.Pixel:IonArt.Disc;
                b.renderer.transform.localScale=friendly?new Vector3(.075f,.55f,1):Vector3.one*(radius*2);
                b.renderer.color=friendly?new Color(.7f,1,1):IonArt.Orange;
                b.renderer.transform.position=position;
                b.renderer.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(-velocity.x,velocity.y)*Mathf.Rad2Deg);
                b.halo.transform.localPosition=Vector3.zero;b.halo.transform.localScale=friendly?new Vector3(5,1.6f,1):Vector3.one*3.8f;
                b.halo.color=friendly?new Color(.1f,.75f,1,.5f):new Color(1,.18f,.04f,.7f);
                b.renderer.gameObject.SetActive(true);return;
            }
        }
        void UpdateBullets(float dt)
        {
            foreach(var b in bullets)
            {
                if(!b.active)continue;Vector2 old=b.position;b.position+=b.velocity*dt;b.life-=dt;
                if(b.life<=0||Mathf.Abs(b.position.y)>8.4f||Mathf.Abs(b.position.x)>HalfWidth+2){Release(b);continue;}
                b.renderer.transform.position=b.position;
                if(b.friendly)
                {
                    foreach(var e in enemies)
                    {
                        if(!e.active||e.age<.1f)continue;
                        float r=e.kind==3?1.65f:.44f;
                        if(SegmentDistance(old,b.position,e.position)<(r+b.radius)*(r+b.radius))
                        {DamageEnemy(e,b.damage);Release(b);break;}
                    }
                }
                else if(invincible<=0&&SegmentDistance(old,b.position,playerPosition)<(.21f+b.radius)*(.21f+b.radius))
                {Release(b);HurtPlayer();}
            }
        }
        static float SegmentDistance(Vector2 a,Vector2 b,Vector2 p)
        {var d=b-a;float t=d.sqrMagnitude>.00001f?Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude):0;return (p-a-d*t).sqrMagnitude;}
        void Release(Projectile b){b.active=false;b.renderer.gameObject.SetActive(false);}
        void ClearHostileBullets(){foreach(var b in bullets)if(b.active&&!b.friendly)Release(b);}
        void DamageEnemy(Enemy e,int damage)
        {
            if(!e.active)return;e.health-=damage;e.flash=.07f;
            Emit(e.position+Vector2.down*.2f,new Vector2(RandomRange(-1,1),-1),IonArt.Gold,.14f,.22f);
            if(e.health>0)return;
            e.active=false;e.renderer.gameObject.SetActive(false);e.engine.gameObject.SetActive(false);e.halo.gameObject.SetActive(false);
            Score+=e.kind==3?5000:e.kind==2?180:100;Kills++;
            Explosion(e.position,e.kind==3?3:1);shake=Mathf.Max(shake,e.kind==3?.18f:.045f);
            Play(IonArt.Blast,e.kind==3?.65f:RandomRange(.9f,1.2f));
            if(Kills%7==0||e.kind==3)SpawnCell(e.position);hud.Refresh();
        }
        public void HurtPlayer()
        {
            if(State!=FlightState.Playing||invincible>0)return;
            Shield--;invincible=1.65f;shake=.15f;Explosion(playerPosition,.7f);Ring(playerPosition,IonArt.Cyan,1.8f,.45f);Play(IonArt.Blast,.8f);
            hud.FlashDamage();hud.Refresh();
            if(Shield<=0)Finish(false);
        }
        void Finish(bool won)
        {
            State=won?FlightState.Victory:FlightState.Defeat;
            SetPlayerVisible(won);SaveBest();ClearHostileBullets();hud.ShowBanner(false,Sector,SectorName);hud.Refresh();
        }

        void SpawnCell(Vector2 p)
        {
            foreach(var c in cells)if(!c.active)
            {c.active=true;c.position=p;c.age=0;c.renderer.gameObject.SetActive(true);c.halo.gameObject.SetActive(true);return;}
        }
        void UpdateCells(float dt)
        {
            foreach(var c in cells)
            {
                if(!c.active)continue;c.age+=dt;float distance=(playerPosition-c.position).magnitude;
                c.position+=distance<3?(playerPosition-c.position).normalized*(4+(3-distance)*5)*dt:Vector2.down*1.25f*dt;
                c.renderer.transform.position=c.halo.transform.position=c.position;
                c.halo.transform.localScale=Vector3.one*(.6f+Mathf.Sin(c.age*5)*.1f);
                c.halo.transform.Rotate(0,0,90*dt);
                if(distance<.6f)
                {
                    c.active=false;c.renderer.gameObject.SetActive(false);c.halo.gameObject.SetActive(false);
                    Score+=250;WeaponTier=Mathf.Min(3,WeaponTier+1);Shield=Mathf.Min(5,Shield+1);
                    Ring(playerPosition,IonArt.Gold,1.3f,.4f);Play(IonArt.Pickup);hud.Toast("LÕI NĂNG LƯỢNG  /  NÂNG SÚNG + HỒI KHIÊN");hud.Refresh();
                }
                else if(c.position.y<-7.2f){c.active=false;c.renderer.gameObject.SetActive(false);c.halo.gameObject.SetActive(false);}
            }
        }

        float RandomRange(float min,float max)=>min+(float)random.NextDouble()*(max-min);
        void Emit(Vector2 pos,Vector2 velocity,Color color,float life,float size,bool ring=false)
        {
            foreach(var s in sparks)if(!s.active)
            {
                s.active=true;s.position=pos;s.velocity=velocity;s.color=color;s.life=s.maxLife=life;s.size=size;s.ring=ring;
                s.renderer.sprite=ring?IonArt.Ring:IonArt.Glow;s.renderer.color=color;s.renderer.transform.position=pos;
                s.renderer.transform.localScale=Vector3.one*size;s.renderer.gameObject.SetActive(true);return;
            }
        }
        void Ring(Vector2 pos,Color color,float size,float life)=>Emit(pos,Vector2.zero,color,life,size,true);
        void Explosion(Vector2 pos,float scale)
        {
            Ring(pos,IonArt.Gold,scale*1.5f,.4f);Emit(pos,Vector2.zero,new Color(1,.7f,.3f),.22f,scale*2.2f);
            int count=scale>2?36:12;
            for(int i=0;i<count;i++)
            {
                float a=RandomRange(0,Mathf.PI*2);Vector2 dir=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                Emit(pos,dir*RandomRange(1.8f,5)*scale,Color.Lerp(IonArt.Orange,IonArt.Gold,RandomRange(0,1)),RandomRange(.25f,.7f),RandomRange(.12f,.32f)*Mathf.Sqrt(scale));
            }
        }
        void UpdateSparks(float dt)
        {
            foreach(var s in sparks)
            {
                if(!s.active)continue;s.life-=dt;
                if(s.life<=0){s.active=false;s.renderer.gameObject.SetActive(false);continue;}
                float t=s.life/s.maxLife;s.position+=s.velocity*dt;s.velocity*=Mathf.Exp(-2*dt);
                s.renderer.transform.position=s.position;
                s.renderer.color=new Color(s.color.r,s.color.g,s.color.b,t*.8f);
                s.renderer.transform.localScale=Vector3.one*s.size*(s.ring?1+(1-t)*2:.4f+t*.6f);
            }
        }
        void ResetCombat()
        {
            foreach(var b in bullets)Release(b);
            foreach(var e in enemies){e.active=false;e.renderer.gameObject.SetActive(false);e.engine.gameObject.SetActive(false);e.halo.gameObject.SetActive(false);}
            foreach(var s in sparks){s.active=false;s.renderer.gameObject.SetActive(false);}
            foreach(var c in cells){c.active=false;c.renderer.gameObject.SetActive(false);c.halo.gameObject.SetActive(false);}
            boss=null;waveLive=false;dashTime=0;shake=0;view.transform.position=new Vector3(0,0,-10);
        }

        // Explicit test hooks exercise real transitions and combat without changing normal runs.
        public Vector2 TestAimTarget()
        {
            Enemy closest=null;float dist=float.MaxValue;
            foreach(var e in enemies)if(e.active&&e.age>1){float d=Mathf.Abs(e.position.x-playerPosition.x);if(d<dist){closest=e;dist=d;}}
            Vector2 target=closest==null?new Vector2(0,-4.7f):new Vector2(closest.position.x,-4.7f);
            foreach(var b in bullets)if(b.active&&!b.friendly&&b.velocity.y<-.5f)
            {
                float t=(-4.7f-b.position.y)/b.velocity.y;
                float impact=b.position.x+b.velocity.x*t;
                if(t>0&&t<.85f&&Mathf.Abs(impact-playerPosition.x)<.9f)
                {target.x=ClampPlayer(new Vector2(impact+(playerPosition.x>=impact?2.2f:-2.2f),-4.7f)).x;break;}
            }
            return target;
        }
        public void TestClearWave(){foreach(var e in enemies)if(e.active)DamageEnemy(e,1000);}
        public void TestRemoveInvulnerability(){invincible=0;}
    }
}
