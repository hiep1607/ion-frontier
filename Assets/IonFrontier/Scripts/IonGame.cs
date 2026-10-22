using System;
using UnityEngine;

namespace IonFrontier
{
    public enum FlightState { Menu, Playing, Paused, BossDying, StageClear, Defeat, Victory, UpgradeChoice }

    public sealed partial class IonGame : MonoBehaviour
    {
        public FlightState State { get; private set; }
        public int Score { get; private set; }
        public int Best { get; private set; }
        public int WeaponTier => loadout.Level(Gun);
        public IonLoadout Loadout => loadout;
        public int Sector { get; private set; } = 1;
        public int Kills { get; private set; }
        public bool AutoFire { get; private set; }
        public bool Muted { get; private set; }
        public float DashReady => 1-Mathf.Clamp01(dashCooldown/2.4f);
        public bool BossActive => boss!=null&&boss.active;
        public float BossFraction => BossActive?Mathf.Clamp01(boss.health/boss.maxHealth):0;
        public int BossPhase => !BossActive?0:BossFraction>.66f?1:BossFraction>.33f?2:3;
        public string BossName => IonCampaign.BossNames[Sector-1];
        public int SelectedHull => profile.selectedHull;
        public int UnlockedHulls => IonCampaign.UnlockedHulls(profile);
        public int UnlockedStages => IonCampaign.UnlockedStages(profile);
        public int ClearedStages => profile.clearedStages;
        public int Ammo { get; private set; }
        public int UltimateCharges { get; private set; }
        public bool SessionUnlocked { get; private set; }
        public int Wave { get; private set; }
        public GunKind Gun => loadout.Latest;
        public int AmmoSpent { get; private set; }
        public int GiftsCollected { get; private set; }
        public int BossAttacks { get; private set; }
        public int BossPhaseMask { get; private set; }
        public int BeamCasts { get; private set; }
        public float LastVolleyDamage { get; private set; }
        public float LastVolleyDelay { get; private set; }
        public int LootRolls { get; private set; }
        public int EnemyLootDrops { get; private set; }
        public bool TestFreezeEnemies;
        public bool TestCollectAmmo;
        public int LastVolleyCount { get; private set; }
        public bool TutorialActive { get; private set; }
        public int TutorialStep { get; private set; }
        public int TutorialCompletedMask { get; private set; }
        public bool ProfileFileExists => System.IO.File.Exists(profilePath);
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
            public bool active, friendly, homing,primary,fragmented,fragmentModule,arcModule,shockModule,reclaimModule; public int pierce,volley; public float preFragmentDamage; public ulong hitMask,worldHitMask; public Vector2 position, velocity; public float radius, life,acceleration; public float damage;
            public SpriteRenderer renderer, halo;
        }
        sealed class Enemy
        {
            public bool active,dying,missionMarked,escapePursuer,escapeWarning; public int kind, index,variant,bossArt,serial,escapeLane; public Vector2 escapeAim; public float health,maxHealth; public float age, fire, flash,motionTime;
            public Vector2 position, target, entry; public SpriteRenderer renderer, engine, halo;
        }
        sealed class Spark
        {
            public bool active, ring; public Vector2 position, velocity; public float life, maxLife, size;
            public Color color; public SpriteRenderer renderer;
        }
        sealed class Cell
        {
            public bool active; public PickupKind kind; public Vector2 position; public float age; public SpriteRenderer renderer, halo;
        }

        const int BulletCapacity=512, EnemyCapacity=40, SparkCapacity=220;
        readonly Projectile[] bullets=new Projectile[BulletCapacity];
        readonly Enemy[] enemies=new Enemy[EnemyCapacity];
        readonly Spark[] sparks=new Spark[SparkCapacity];
        readonly Cell[] cells=new Cell[20];
        readonly IonLoadout loadout=new IonLoadout();
        sealed class Beam {public bool active;public float age;public Vector2 origin;public SpriteRenderer body,halo;public Enemy source;public int sourceSerial;}
        readonly Beam[] beams=new Beam[4];
        PilotProfile profile;string profilePath;bool replayTutorial;
        int dashCount,tutorialDashStart,tutorialGiftStart;Vector2 tutorialOrigin;
        float tutorialClock,supplyClock,dryClock;
        Camera view; IonHud hud; Enemy boss;
        SpriteRenderer player, playerHalo, shieldRing, leftEngine, rightEngine;
        AudioSource music; AudioSource[] voices; int voiceIndex;
        Vector2 playerPosition, playerVelocity, dashDirection;
        Vector3 lastMouse;
        float fireClock, dashCooldown, dashTime, invincible, shake, waveTime, transition, exhaustClock, soundClock;
        bool keyboardMode, waveLive;
        bool runResolved;
        FlightState pausedFrom=FlightState.Playing;
        float bannerTimer;
        System.Random random;
        string bestPath;
        public string SectorName => IonCampaign.StageNames[Mathf.Clamp(Sector-1,0,IonCampaign.Stages-1)];

        void Awake()
        {
            Application.targetFrameRate=120; QualitySettings.vSyncCount=1;
            Time.fixedDeltaTime=1f/60; random=new System.Random(479);
            bool clarityTest=Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-clarity-test")>=0;bool balanceTest=Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-balance-test")>=0;
            if(clarityTest){Application.runInBackground=true;gameObject.AddComponent<IonClarityTest>();}if(balanceTest){Application.runInBackground=true;gameObject.AddComponent<IonBalanceTest>();}
            bool tacticalTest=Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-tactical-test")>=0;if(tacticalTest){Application.runInBackground=true;gameObject.AddComponent<IonTacticalTest>();}
            bool playtest=Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-playtest")>=0;
            bool ratingTest=Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-rating-test")>=0;if(ratingTest){Application.runInBackground=true;gameObject.AddComponent<IonRatingTest>();}
            bool escapeTest=Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-escape-test")>=0;if(escapeTest){Application.runInBackground=true;gameObject.AddComponent<IonEscapeTest>();}
            bool backgroundTest=Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-backdrop-test")>=0;if(backgroundTest){Application.runInBackground=true;gameObject.AddComponent<IonBackdropTest>();}
            bool vitalsTest=Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-vitals-test")>=0;if(vitalsTest){Application.runInBackground=true;gameObject.AddComponent<IonVitalsTest>();}
            if(playtest){Application.runInBackground=true;gameObject.AddComponent<IonPlaytest>();}
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-campaign-test")>=0){Application.runInBackground=true;gameObject.AddComponent<IonCampaignTest>();}
            TestAutoDraft=!clarityTest&&!balanceTest&&!tacticalTest&&(playtest||ratingTest||escapeTest||vitalsTest||backgroundTest||Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-campaign-test")>=0);
            if(clarityTest||balanceTest||tacticalTest||TestAutoDraft)gameObject.AddComponent<IonFrameMetrics>();
            IonArt.Initialize(); view=Camera.main;
            if(view==null) { var go=new GameObject("Flight camera"); view=go.AddComponent<Camera>(); go.tag="MainCamera"; go.AddComponent<AudioListener>(); }
            view.orthographic=true; view.orthographicSize=7; view.transform.position=new Vector3(0,0,-10);
            view.clearFlags=CameraClearFlags.SolidColor; view.backgroundColor=IonArt.Ink;
            HalfWidth=Mathf.Min(13.4f,7*view.aspect);
            var backdrop=new GameObject("Parallax starfield").AddComponent<IonBackdrop>(); backdrop.Initialize(HalfWidth,this);
            CreatePools(); CreatePlayer(); CreateAudio();
            profilePath=System.IO.Path.Combine(Application.persistentDataPath,playtest?"playtest-profile-v2.json":"pilot-profile-v2.json");
            bool campaignTest=Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-campaign-test")>=0||vitalsTest||backgroundTest||escapeTest||ratingTest||tacticalTest||clarityTest||balanceTest;
            if(campaignTest)profilePath=System.IO.Path.Combine(Application.persistentDataPath,clarityTest?"clarity-test-profile.json":balanceTest?"balance-test-profile.json":tacticalTest?"tactical-test-profile.json":ratingTest?"rating-test-profile.json":escapeTest?"escape-test-profile.json":backgroundTest?"backdrop-test-profile.json":vitalsTest?"vitals-test-profile.json":"campaign-test-profile.json");
            profile=playtest||campaignTest?new PilotProfile():IonCampaign.Load(profilePath);
            SessionUnlocked=!playtest&&!campaignTest&&Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-unlocked")>=0;
            if(SessionUnlocked)
            {
                // This launch gets its own disposable profile; the real campaign save is never overwritten.
                profilePath=System.IO.Path.Combine(Application.temporaryCachePath,"unlocked-session-"+Guid.NewGuid().ToString("N")+".json");
                profile=new PilotProfile{clearedStages=IonCampaign.Stages,tutorialDone=true};
            }
            bestPath=System.IO.Path.Combine(Application.persistentDataPath,playtest?"playtest-best.txt":"best-score.txt");
            if(SessionUnlocked)bestPath=System.IO.Path.ChangeExtension(profilePath,"best.txt");
            if(campaignTest)bestPath=System.IO.Path.Combine(Application.persistentDataPath,clarityTest?"clarity-test-best.txt":balanceTest?"balance-test-best.txt":tacticalTest?"tactical-test-best.txt":ratingTest?"rating-test-best.txt":escapeTest?"escape-test-best.txt":backgroundTest?"backdrop-test-best.txt":vitalsTest?"vitals-test-best.txt":"campaign-test-best.txt");
            try { if(System.IO.File.Exists(bestPath)&&int.TryParse(System.IO.File.ReadAllText(bestPath),out int saved))Best=saved; } catch(System.IO.IOException) { Best=0; }
            hud=new GameObject("Flight interface").AddComponent<IonHud>(); hud.Initialize(this);
            State=FlightState.Menu; SetPlayerVisible(false); hud.Refresh();
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
                var sr=IonArt.Sprite("Hostile spacecraft",IonArt.EnemyShips[0],Vector2.zero,Vector2.one,Color.white,10,pool);
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
                var sr=IonArt.Sprite("Energy core",IonArt.GiftIcons[0],Vector2.zero,Vector2.one*.64f,IonArt.Gold,20,pool);
                var halo=IonArt.Sprite("Core halo",IonArt.Ring,Vector2.zero,Vector2.one*.6f,IonArt.Gold,19,pool);
                cells[i]=new Cell{renderer=sr,halo=halo}; sr.gameObject.SetActive(false); halo.gameObject.SetActive(false);
            }
        }

        void CreatePlayer()
        {
            player=IonArt.Sprite("Vanguard interceptor",IonArt.PlayerShips[0],Vector2.zero,Vector2.one*1.95f,Color.white,20,transform);
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

        public bool SelectHull(int index)
        {
            if(State!=FlightState.Menu||index<0||index>=UnlockedHulls)return false;
            profile.selectedHull=index;IonCampaign.Save(profilePath,profile);hud.Refresh();return true;
        }
        public bool SelectStage(int stage)
        {
            if(State!=FlightState.Menu||stage<1||stage>UnlockedStages)return false;
            Sector=stage;hud.Refresh();return true;
        }
        public void BeginRun()
        {
            if(Sector>UnlockedStages)return;
            ResetCombat();runResolved=false;ResetDamageSources();ResetRating();ResetDraft();ResetModules();Score=0;Health=MaxHealth;damageFeedbackClock=damageContactClock=0;Kills=0;loadout.Reset();Wave=0;Elapsed=0;
            Ammo=IonCampaign.StartingAmmo(Sector);UltimateCharges=SessionUnlocked?1:0;AmmoSpent=0;GiftsCollected=0;BossAttacks=0;BossPhaseMask=0;BeamCasts=0;AutoFire=false;LootRolls=0;EnemyLootDrops=0;TestFreezeEnemies=false;TestCollectAmmo=false;
            player.sprite=IonArt.PlayerShips[SelectedHull];player.transform.localScale=Vector3.one*IonArt.ScaleFor(player.sprite,1.85f);
            playerPosition=new Vector2(0,-4.7f);playerVelocity=Vector2.zero;fireClock=0;dashCooldown=0;invincible=2;
            lastMouse=Input.mousePosition;keyboardMode=false;waveTime=0;transition=2;bannerTimer=2.8f;supplyClock=12;
            TutorialActive=Sector==1&&(!profile.tutorialDone||replayTutorial);TutorialStep=0;TutorialCompletedMask=0;
            tutorialOrigin=playerPosition;tutorialClock=0;replayTutorial=false;
            State=FlightState.Playing;SetPlayerVisible(true);StartMission();hud.Refresh();Play(IonArt.Pickup);UpdatePlayerArt(0);
        }
        public void ReplayTutorial(){if(State==FlightState.Menu){Sector=1;replayTutorial=true;BeginRun();}}
        public void NextStageHangar()
        {
            if(State!=FlightState.StageClear)return;
            int next=Sector+1;ReturnToMenu();Sector=Mathf.Min(IonCampaign.Stages,next);profile.selectedHull=UnlockedHulls-1;IonCampaign.Save(profilePath,profile);hud.Refresh();
        }

        public void ReturnToMenu()
        {
            SaveBest(); ResetCombat();ResetModules(); State=FlightState.Menu; SetPlayerVisible(false); hud.Refresh();
        }
        public void TogglePause()
        {
            if(State==FlightState.Playing||State==FlightState.BossDying||State==FlightState.UpgradeChoice) {pausedFrom=State;State=FlightState.Paused;}
            else if(State==FlightState.Paused) {State=pausedFrom; lastMouse=Input.mousePosition;}
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
        void OnApplicationFocus(bool focus){if(!focus&&(State==FlightState.Playing||State==FlightState.BossDying)&&!TestInput)TogglePause();}

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.M)) ToggleAudio();
            if(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.P)) TogglePause();
            if(State==FlightState.Menu)
            {
                if(Input.GetKeyDown(KeyCode.Alpha1))SelectHull(0);if(Input.GetKeyDown(KeyCode.Alpha2))SelectHull(1);if(Input.GetKeyDown(KeyCode.Alpha3))SelectHull(2);
                for(int i=3;i<8;i++)if(Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1+i)))SelectHull(i);
                if(Input.GetKeyDown(KeyCode.Return))BeginRun();
            }
            if((State==FlightState.Defeat||State==FlightState.Victory)&&Input.GetKeyDown(KeyCode.R)) BeginRun();
            if(State==FlightState.Playing&&Input.GetKeyDown(KeyCode.F)) ToggleAuto();
            if(State==FlightState.Playing&&Input.GetKeyDown(KeyCode.Q))UseUltimate();
            float realDt=Mathf.Min(Time.unscaledDeltaTime,.05f);
            if(State==FlightState.Paused) { view.transform.position=new Vector3(0,0,-10); return; }
            if(State==FlightState.UpgradeChoice){TickDraft(Time.unscaledDeltaTime);return;}
            UpdateSparks(realDt);
            if(State==FlightState.Playing)UpdateBossDeath(realDt);
            if(State==FlightState.BossDying){UpdateBossDeath(realDt);hud.Tick(realDt);return;}
            if(State!=FlightState.Playing) { view.transform.position=new Vector3(0,0,-10); return; }
            float dt=Mathf.Min(Time.deltaTime,.04f);TickRating(dt);TickModules(dt);TickModuleVisuals(dt);TickCounterWreck(dt);UpdateVitals(dt); Elapsed+=dt; waveTime+=dt;
            invincible=Mathf.Max(0,invincible-dt); dashCooldown=Mathf.Max(0,dashCooldown-dt); soundClock-=dt;dryClock-=dt;
            bannerTimer=Mathf.Max(0,bannerTimer-dt); hud.ShowBanner(bannerTimer>0, Sector, SectorName);
            TickEncounters(dt);UpdatePlayer(dt);UpdateEnemies(dt);
            if(State!=FlightState.Playing)return;
            UpdateBullets(dt);
            if(State!=FlightState.Playing)return;
            UpdateUltimate(dt);
            if(State!=FlightState.Playing)return;
            UpdateCells(dt);UpdateBeams(dt);
            if(State!=FlightState.Playing)return;
            if(Sector>=4){UpdateMission(dt);UpdateExpandedBoss(dt);}
            if(State!=FlightState.Playing)return;
            if(TutorialActive)UpdateTutorial(dt);
            else if(Sector>=4){UpdateMissionDirector(dt);}
            else
            {
                supplyClock-=dt;
                if(supplyClock<=0)
                {
                    supplyClock=BossActive?14:18;
                    bool ammoOnField=false;foreach(var c in cells)if(c.active&&c.kind==PickupKind.Ammo){ammoOnField=true;break;}
                    if(Ammo<35&&!ammoOnField)SpawnCell(new Vector2(Mathf.Clamp(playerPosition.x,-7,7),2),PickupKind.Ammo);
                }
                if(!waveLive){transition-=dt;if(transition<=0)SpawnWave();}
                else if(ActiveEnemies==0)
                {
                    waveLive=false;boss=null;ClearHostileBullets();ClearBeams();
                    if(Wave>IonCampaign.Waves){if(PendingBossWrecks==0)Finish(true);}
                    else
                    {
                        transition=2;waveTime=0;bannerTimer=2.5f;
                        if(Wave<=2)RequestUpgradeChoice(Wave-1);
                        else {PickupKind reward=Ammo<60?PickupKind.Ammo:
                            !loadout.Has(GunKind.Spread)?PickupKind.Spread:
                            loadout.PatternCount<2?(random.Next(2)==0?PickupKind.Pierce:PickupKind.Homing):
                            (PickupKind)((int)PickupKind.Damage+random.Next(3));
                        SpawnCell(new Vector2(0,2.2f),reward);}hud.Refresh();
                    }
                }
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
                desired=Vector2.ClampMagnitude((TestTarget-playerPosition)*7,MoveSpeed);
            }
            else if(input.sqrMagnitude>.01f) {keyboardMode=true; desired=input*MoveSpeed;}
            else
            {
                if(moved) keyboardMode=false;
                if(!keyboardMode)
                {
                    Vector2 target=view.ScreenToWorldPoint(new Vector3(mouse.x,mouse.y,10));
                    // Keep the ship where it is until the pilot actually moves the mouse.
                    if(moved||playerVelocity.sqrMagnitude>.02f) desired=Vector2.ClampMagnitude((ClampPlayer(target)-playerPosition)*14,MoveSpeed);
                }
            }
            lastMouse=mouse;
            bool dashPressed=Input.GetKeyDown(KeyCode.LeftShift)||Input.GetKeyDown(KeyCode.RightShift)||Input.GetMouseButtonDown(1)||(TestInput&&TestDashRequest);
            TestDashRequest=false;
            if(dashPressed && dashCooldown<=0)
            {
                dashDirection=desired.sqrMagnitude>.1f?desired.normalized:Vector2.up;
                dashCount++;ModuleDash();dashTime=.16f; dashCooldown=2.4f; invincible=Mathf.Max(invincible,.38f); Play(IonArt.Dash); Ring(playerPosition,IonArt.Cyan,.45f,.4f);
            }
            if(dashTime>0){dashTime-=dt;playerVelocity=dashDirection*27*IonHullStats.Speed[SelectedHull];}
            else playerVelocity=Vector2.Lerp(playerVelocity,desired,1-Mathf.Exp(-18*dt));
            playerPosition=ClampPlayer(playerPosition+playerVelocity*dt);
            fireClock-=dt;
            if((TestInput?TestFire:(AutoFire||Input.GetKey(KeyCode.Space)||(Input.GetMouseButton(0)&&!(UnityEngine.EventSystems.EventSystem.current!=null&&UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()))))&&fireClock<=0)
            {
                TryFire();
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
            player.color=invincible>0?Color.Lerp(Color.white,IonCampaign.HullColors[SelectedHull],.2f+Mathf.Sin(Elapsed*28)*.15f):Color.white;
            playerHalo.transform.position=playerPosition+Vector2.down*.35f;
            leftEngine.transform.position=playerPosition+new Vector2(-.24f,-.53f);
            rightEngine.transform.position=playerPosition+new Vector2(.24f,-.53f);
            float flame=.65f+Mathf.Sin(Elapsed*48)*.09f+(dashTime>0?.6f:0);
            leftEngine.transform.localScale=rightEngine.transform.localScale=new Vector3(.28f,flame,1);
            leftEngine.color=rightEngine.color=IonCampaign.HullColors[SelectedHull];playerHalo.color=new Color(leftEngine.color.r,leftEngine.color.g,leftEngine.color.b,.3f);
            shieldRing.transform.position=playerPosition; shieldRing.enabled=invincible>0;
            shieldRing.color=new Color(.25f,.85f,1,.2f+Mathf.Sin(Elapsed*8)*.1f);
        }
        void SetPlayerVisible(bool visible)
        {
            player.enabled=visible;playerHalo.enabled=visible;leftEngine.enabled=visible;rightEngine.enabled=visible;shieldRing.enabled=visible;
        }

        bool TryFire()
        {
            if(Ammo<=0){fireClock=.2f;if(dryClock<=0){dryClock=3;hud.Toast("HẾT ĐẠN / NHẶT HỘP ĐẠN — TIẾP TẾ ĐANG ĐẾN");}return false;}
            bool free=false;foreach(var b in bullets)if(!b.active){free=true;break;}if(!free){fireClock=.08f;return false;}
            // Hull multipliers combine with the same two-slot upgrade system.
            Ammo--;AmmoSpent++;fireClock=GunDelay;LastVolleyDelay=fireClock;
            float volleyDamage=GunDamage*BeginModuleVolley();int n=loadout.ShotCount;LastVolleyCount=0;LastVolleyDamage=volleyDamage;
            Vector2 muzzle=playerPosition+Vector2.up*.64f;
            for(int i=0;i<n;i++)
            {
                float offset=i-(n-1)*.5f,angle=loadout.Has(GunKind.Spread)?offset*Mathf.Min(.17f,1.1f/Mathf.Max(1,(n-1)*.5f))*(ModuleActive(CombatModule.FocusedSpread)?.6f:1):0;
                var shot=Bolt(muzzle+Vector2.right*offset*Mathf.Min(.23f,.95f/Mathf.Max(1,(n-1)*.5f)),new Vector2(Mathf.Sin(angle),Mathf.Cos(angle))*(loadout.Has(GunKind.Homing)?16:24),true,loadout.Has(GunKind.Pierce)?.14f:.1f);ConfigureModuleBolt(shot,volleyDamage);if(shot!=null)LastVolleyCount++;
            }
            Emit(muzzle,Vector2.zero,IonCampaign.HullColors[SelectedHull],.12f,.42f);
            if(soundClock<=0){Play(IonArt.Laser,1+(int)Gun*.08f);soundClock=.1f;}return true;
        }
        void UpdateTutorial(float dt)
        {
            tutorialClock+=dt;
            switch(TutorialStep)
            {
                case 0:if((playerPosition-tutorialOrigin).sqrMagnitude>1){TutorialCompletedMask|=1;TutorialStep=1;SpawnEnemy(0,1,new Vector2(0,1.8f),2);tutorialClock=0;}break;
                case 1:if(ActiveEnemies==0){TutorialCompletedMask|=2;TutorialStep=2;tutorialDashStart=dashCount;tutorialClock=0;}break;
                case 2:if(dashCount>tutorialDashStart){TutorialCompletedMask|=4;TutorialStep=3;tutorialGiftStart=GiftsCollected;SpawnCell(new Vector2(Mathf.Clamp(playerPosition.x+3,-7,7),-2.6f),PickupKind.Ammo);tutorialClock=0;}break;
                case 3:if(GiftsCollected>tutorialGiftStart){TutorialCompletedMask|=8;TutorialStep=4;tutorialClock=0;}break;
                case 4:if(tutorialClock>1.7f)FinishTutorial();break;
            }
            invincible=Mathf.Max(invincible,.5f);hud.RefreshTutorial();
        }
        public void SkipTutorial(){if(TutorialActive)FinishTutorial();}
        void FinishTutorial()
        {
            ResetCombat();TutorialActive=false;profile.tutorialDone=true;IonCampaign.Save(profilePath,profile);
            Ammo=IonCampaign.StartingAmmo(Sector);AmmoSpent=0;Score=Kills=0;Wave=0;
            playerPosition=new Vector2(0,-4.7f);playerVelocity=Vector2.zero;dashCooldown=0;transition=2;waveTime=0;supplyClock=10;invincible=2;
            hud.Refresh();hud.Toast("SẴN SÀNG / "+Ammo+" ĐẠN KHỞI ĐẦU");
        }

        void SpawnWave()
        {
            Wave++;waveLive=true;waveTime=0;boss=null;
            if(Wave>IonCampaign.Waves)
            {
                boss=SpawnEnemy(0,7+Sector,new Vector2(0,1.9f),IonCampaign.BossHealth[Sector-1]);
                ratingBossSeen=true;boss.fire=2.6f;ClearHostileBullets();hud.Toast("CẢNH BÁO / "+BossName+" ĐÃ TIẾP CẬN");hud.Refresh();return;
            }
            int count=IonCampaign.WaveCount(Sector,Wave);
            for(int i=0;i<count;i++)
            {
                int cols=Sector==1?7:8;
                Vector2 target=new Vector2((i%cols-(cols-1)*.5f)*(Sector==1?2:1.9f),4.5f-(i/cols)*1.35f);
                int kind=IonCampaign.EnemyKind(Sector,Wave,i);SpawnEnemy(i,kind+1,target,IonCampaign.EnemyHealth[kind]);
            }
            ConfigureOpeningEncounter();hud.Toast("MÀN "+Sector+" / ĐỢT "+Wave+" TRÊN 3");hud.Refresh();
        }
        Enemy SpawnEnemy(int i,int kind,Vector2 target,int hp)
        {
            var e=enemies[i]; CancelSourceBeams(e);e.serial++;e.active=true;e.dying=false;e.missionMarked=false;e.escapePursuer=e.escapeWarning=false;e.kind=kind;e.variant=-1;e.bossArt=Sector-1;e.health=e.maxHealth=hp;e.target=target;e.position=target+Vector2.up*8;e.entry=e.position;
            e.index=i;e.age=-i*.045f;e.fire=RandomRange(1.6f,4.8f);e.flash=0;e.motionTime=waveTime;
            e.renderer.sprite=kind>=8?IonArt.BossFleet[Sector-1]:IonArt.EnemyShips[kind-1];e.renderer.color=Color.white;e.renderer.gameObject.SetActive(true);
            e.engine.gameObject.SetActive(true);e.halo.gameObject.SetActive(true);
            e.renderer.transform.localScale=Vector3.one*IonArt.ScaleFor(e.renderer.sprite,kind>=8?5:kind==4||kind==6?1.85f:1.65f);
            e.engine.color=kind==2?IonArt.Gold:IonArt.Orange;e.halo.sprite=IonArt.Glow;
            if(kind<8)SetEnemyVariant(e,IonHostileFleet.Variant(Sector,i+kind));
            return e;
        }
        void UpdateEnemies(float dt)
        {
            foreach(var e in enemies)
            {
                if(State!=FlightState.Playing||runResolved)return;
                if(!e.active||TestFreezeEnemies) continue;
                if(UpdateCounterEnemy(e,dt)||UpdateEncounterEnemy(e,dt))continue;
                if(e.escapePursuer){UpdateEscapePursuer(e,dt);continue;}
                float step=dt*EnemySlow(e.position);e.age+=step;e.motionTime+=step; e.flash=Mathf.Max(0,e.flash-dt);
                Vector2 target=e.target;
                if(e.kind>=8) target+=BossMovement(e);
                else if(Sector==2) target+=new Vector2(Mathf.Sin(e.motionTime*1.1f+e.index*.19f)*1.6f,Mathf.Sin(e.motionTime*1.3f+e.index*.25f)*.5f);
                else target+=new Vector2(Mathf.Sin(e.motionTime*.65f)*1.25f,Mathf.Sin(e.motionTime*.9f)*.2f);
                if(e.variant>=7)target=ReinforcementMotion(e,target);
                if(e.kind==3 && e.variant<7 && e.age>6)
                {
                    float dive=(e.age-4+e.index*.7f)%11;
                    if(dive<3)target+=new Vector2(Mathf.Sin(dive/3*Mathf.PI*2)*2,-Mathf.Sin(dive/3*Mathf.PI)*7);
                }
                float entry=Mathf.Clamp01(e.age/1.35f);entry=entry*entry*(3-2*entry);
                Vector2 previous=e.position;e.position=Vector2.Lerp(e.entry,target,entry);
                e.renderer.transform.position=e.position;
                float tilt=Mathf.Clamp((e.position.x-previous.x)/Mathf.Max(dt,.001f)*-2,-16,16);
                e.renderer.transform.rotation=Quaternion.Euler(0,0,tilt);
                e.renderer.color=e.flash>0?new Color(1,.55f,.4f):EnemyTint(e);
                e.engine.transform.position=e.position+Vector2.up*(e.kind>=8?1.45f:.43f);
                e.engine.transform.localScale=e.kind>=8?new Vector3(2,.8f,1):new Vector3(.4f,.62f+Mathf.Sin(e.age*20)*.07f,1);
                e.halo.transform.position=e.position;e.halo.transform.localScale=Vector3.one*(e.kind>=8?5.1f:1.8f);
                e.halo.color=new Color(1,.2f,.05f,e.kind>=8?.15f:.055f);
                if(e.variant>=7)UpdateReinforcement(e,step);
                if(e.missionMarked){e.halo.sprite=IonArt.Ring;e.halo.color=new Color(1,.2f,.1f,.7f);e.halo.transform.localScale=Vector3.one*2.15f;}
                if(e.age<1.7f||TutorialActive) continue;
                e.fire-=step;
                if(e.kind>=8)
                {
                    if(e.fire<.8f)e.halo.color=new Color(1,.25f,.1f,.22f+Mathf.Sin(e.age*30)*.08f);
                    if(e.fire<=0){BossAttack(e);int phase=IndividualBossPhase(e);e.fire=(Sector>=3?2.1f:2.5f)*(phase==3?.65f:phase==2?.8f:1)*(Sector==11&&ActiveBossCount==1?.72f:1);}
                }
                else if(e.fire<=0)
                {
                    e.fire=RandomRange(5.3f,8.5f);EnemyAttack(e);
                }
                if((e.position-playerPosition).sqrMagnitude<(e.kind>=8?3.1f:.75f) && invincible<=0)
                {
                    HurtPlayer(e.kind>=8?32:20,false,"VA CHẠM TÀU"); if(e.kind<8) DamageEnemy(e,99);
                }
            }
        }
        void Fan(Vector2 p,Vector2 direction,int count,float degrees,float speed,float radius=.16f)
        {
            for(int i=0;i<count;i++)Bolt(p,(Vector2)(Quaternion.Euler(0,0,(i-(count-1)*.5f)*degrees)*direction)*speed,false,radius);
        }
        void EnemyAttack(Enemy e)
        {
            if(e.position.y<-.6f)return;Vector2 p=e.position+Vector2.down*.4f,aim=(playerPosition-p).normalized;
            if(Sector>=4&&MissionConvoyActive&&e.index%2==0)aim=(ConvoyPosition-p).normalized;
            if(e.variant>=7){ReinforcementAttack(e,p,aim);return;}
            if(e.kind==4)Fan(p,Vector2.down,3,24,3.6f,.23f);
            else if(e.kind==6){Fan(p+Vector2.left*.3f,Vector2.down,1,0,4);Fan(p+Vector2.right*.3f,Vector2.down,1,0,4);}
            else Fan(p,e.kind==1||e.kind==3?Vector2.down:aim,e.kind==7?3:1,12,e.kind==5?7:4.3f,.14f);
        }
        void BossAttack(Enemy e)
        {
            int phase=IndividualBossPhase(e);BossPhaseMask|=1<<phase;BossAttacks++;Vector2 p=e.position+Vector2.down*1.1f,aim=(playerPosition-p).normalized;
            if(Sector>=4){ExpandedBossAttack(e,p,aim,phase);return;}
            if(Sector==1)
            {if(BossAttacks%2==1)Fan(p,Vector2.down,5+phase*2,13,3.7f);else Fan(p,aim,phase+1,13,4.8f);}
            else if(Sector==2)
            {
                if(BossAttacks%2==1){int n=12+phase*2;for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n+e.age*.35f;Bolt(p,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*3.8f,false,.15f);}}
                else Fan(p,aim,3+phase*2,10,4.8f);
            }
            else
            {
                int pattern=BossAttacks%4;
                if(pattern==1)Fan(p,Vector2.down,9+phase*2,12,4.3f,.17f);
                else if(pattern==2){for(int side=-1;side<=1;side+=2)Fan(p+Vector2.right*side,aim,phase+2,12,5.1f,.18f);}
                else if(pattern==3){for(int side=-1;side<=1;side+=2)SpawnBeam(p+Vector2.right*side*1.6f,e);hud.Toast("PHÁO HẠT NẶNG / TRÁNH VẠCH ĐỎ ĐANG NẠP!");}
                else
                {
                    int gap=BossAttacks%7+3;for(int i=0;i<15;i++)if(Mathf.Abs(i-gap)>1)Bolt(new Vector2((i-7)*1.5f,e.position.y-1),Vector2.down*3.8f,false,.18f);
                    if(phase==3)Fan(p,aim,3,14,4.5f);
                }
            }
            Ring(p,IonArt.Orange,1.2f,.35f);Play(IonArt.Impact,.65f);
        }
        int IndividualBossPhase(Enemy e)=>e.health/e.maxHealth>.66f?1:e.health/e.maxHealth>.33f?2:3;
        void SpawnBeam(Vector2 origin,Enemy source=null)
        {
            for(int i=0;i<beams.Length;i++)
            {
                if(beams[i]==null)beams[i]=new Beam{body=IonArt.Sprite("Heavy beam",IonArt.Pixel,Vector2.zero,Vector2.one,Color.white,16,transform),halo=IonArt.Sprite("Beam bloom",IonArt.Glow,Vector2.zero,Vector2.one,Color.white,15,transform)};
                var beam=beams[i];if(beam.active)continue;
                beam.active=true;beam.age=0;beam.origin=origin;beam.source=source;beam.sourceSerial=source==null?0:source.serial;beam.body.color=beam.halo.color=Color.clear;beam.body.gameObject.SetActive(true);beam.halo.gameObject.SetActive(true);BeamCasts++;return;
            }
        }
        void ReleaseBeam(Beam beam){beam.active=false;beam.source=null;beam.body.gameObject.SetActive(false);beam.halo.gameObject.SetActive(false);}
        void CancelSourceBeams(Enemy source){foreach(var beam in beams)if(beam!=null&&beam.active&&beam.source==source&&beam.sourceSerial==source.serial)ReleaseBeam(beam);}
        void ClearBeams(){foreach(var beam in beams)if(beam!=null)ReleaseBeam(beam);}
        void UpdateBeams(float dt)
        {
            foreach(var beam in beams)
            {
                if(State!=FlightState.Playing||runResolved)return;
                if(beam==null||!beam.active)continue;
                if(beam.source!=null&&(!beam.source.active||beam.source.serial!=beam.sourceSerial)){ReleaseBeam(beam);continue;}
                float previous=beam.age;beam.age+=dt;
                Vector2 end=new Vector2(beam.origin.x,-6.2f);bool firing=beam.age>=1.15f;float length=(end-beam.origin).magnitude;
                beam.body.transform.position=beam.halo.transform.position=(end+beam.origin)*.5f;
                beam.body.transform.localScale=new Vector3(firing?.32f:.05f,length,1);beam.halo.transform.localScale=new Vector3(firing?1.1f:.28f,length*1.4f,1);
                beam.body.color=firing?new Color(1,.85f,.65f):new Color(1,.2f,.1f,.45f+Mathf.Sin(Elapsed*25)*.2f);beam.halo.color=new Color(1,.12f,.03f,firing?.65f:.15f);
                float slice=Mathf.Max(0,Mathf.Min(beam.age,1.9f)-Mathf.Max(previous,1.15f));
                if(slice>0&&SegmentDistance(beam.origin,end,playerPosition)<.38f*.38f)HurtPlayer(40*slice,true,"LASER");
                if(beam.age>=1.9f)ReleaseBeam(beam);
            }
        }

        Projectile Bolt(Vector2 position,Vector2 velocity,bool friendly,float radius=.1f)
        {
            foreach(var b in bullets)
            {
                if(b.active)continue;
                b.primary=b.fragmented=b.fragmentModule=b.arcModule=b.shockModule=b.reclaimModule=false;b.volley=0;b.preFragmentDamage=0;b.active=true;b.friendly=friendly;b.position=position;b.velocity=velocity;b.radius=radius;b.damage=friendly?GunDamage:HostileShotDamage(radius);b.life=6;b.acceleration=0;b.hitMask=0;b.worldHitMask=0;b.pierce=friendly?loadout.PierceCount:1;b.homing=friendly&&loadout.Has(GunKind.Homing);
                b.renderer.sprite=friendly?IonArt.BoltIcons[(int)loadout.Visual,SelectedHull]:IonArt.HostileBolt;
                b.renderer.transform.localScale=friendly?new Vector3(loadout.Has(GunKind.Pierce)?.3f:.2f,loadout.Has(GunKind.Pierce)?1.1f:.68f,1):Vector3.one*(radius*2);
                b.renderer.color=friendly?IonCampaign.HullColors[SelectedHull]:Sector==2?new Color(.8f,.4f,1):IonArt.Orange;
                b.renderer.transform.position=position;
                b.renderer.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(-velocity.x,velocity.y)*Mathf.Rad2Deg);
                b.halo.transform.localPosition=Vector3.zero;b.halo.transform.localScale=friendly?new Vector3(5,1.6f,1):Vector3.one*3.8f;
                b.halo.color=friendly?new Color(.1f,.75f,1,.5f):new Color(1,.18f,.04f,.7f);
                b.renderer.gameObject.SetActive(true);return b;
            }
            return null;
        }
        void UpdateBullets(float dt)
        {
            foreach(var b in bullets)
            {
                if(State!=FlightState.Playing||runResolved)return;
                if(!b.active)continue;
                if(b.homing&&CounterHomingTarget(b,out Vector2 counterAim))b.velocity=Vector2.Lerp(b.velocity,(counterAim-b.position).normalized*18,1-Mathf.Exp(-5*dt));
                else if(b.homing)
                {
                    Enemy nearest=null;float distance=100;
                    foreach(var e in enemies)if(e.active&&e.age>1&&e.position.y>b.position.y-.5f&&(b.hitMask&(1UL<<e.index))==0){float d=(e.position-b.position).sqrMagnitude;if(d<distance){nearest=e;distance=d;}}
                    if(nearest!=null)b.velocity=Vector2.Lerp(b.velocity,(nearest.position-b.position).normalized*18,1-Mathf.Exp(-5*dt));
                }
                if(b.acceleration>0)b.velocity=Vector2.ClampMagnitude(b.velocity*(1+b.acceleration*dt),9);
                Vector2 old=b.position;b.position+=b.velocity*dt*(b.friendly?1:EnemySlow(b.position));b.life-=dt;
                if(b.life<=0||Mathf.Abs(b.position.y)>8.4f||Mathf.Abs(b.position.x)>HalfWidth+2){Release(b);continue;}
                b.renderer.transform.position=b.position;
                if(b.friendly&&HitCounterattack(b,old))continue;
                if(Sector>=4&&HitMissionObject(b,old))continue;
                if(b.friendly)
                {
                    foreach(var e in enemies)
                    {
                        ulong bit=1UL<<e.index;if(!e.active||e.age<.1f||(b.hitMask&bit)!=0)continue;
                        float r=e.kind>=8?1.65f:.44f;
                        if(SegmentDistance(old,b.position,e.position)<(r+b.radius)*(r+b.radius))
                        {b.hitMask|=bit;DamageEnemy(e,b.damage,b);if(--b.pierce<=0){Release(b);break;}}
                    }
                }
                else if(invincible<=0&&SegmentDistance(old,b.position,playerPosition)<(.21f+b.radius)*(.21f+b.radius))
                {float impact=b.damage;Release(b);HurtPlayer(impact,false,"ĐẠN ĐỊCH");}
            }
        }
        static float SegmentDistance(Vector2 a,Vector2 b,Vector2 p)
        {var d=b-a;float t=d.sqrMagnitude>.00001f?Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude):0;return (p-a-d*t).sqrMagnitude;}
        void Release(Projectile b){b.active=false;b.renderer.gameObject.SetActive(false);}
        void ClearHostileBullets(){foreach(var b in bullets)if(b.active&&!b.friendly)Release(b);}
        void DamageEnemy(Enemy e,float damage,Projectile source=null)
        {
            if(State!=FlightState.Playing||runResolved||!e.active)return;
            damage=ModifyCounterDamage(e,damage);damage=EncounterDamage(e,damage);if(Sector>=4)damage=ModifyEnemyDamage(e,damage);TrackAttackTarget(e,damage<=0);if(damage<=0)return;
            float accepted=Mathf.Min(e.health,damage);e.health-=damage;e.flash=.07f;OnModuleHit(source,e,accepted);
            Emit(e.position+Vector2.down*.2f,new Vector2(RandomRange(-1,1),-1),IonArt.Gold,.14f,.22f);
            if(e.health>0)return;
            e.active=false;CancelSourceBeams(e);
            if(OnCounterEnemyKilled(e))return;
            OnEncounterKilled(e);OnModuleKill(source,e);
            if(e.escapePursuer)OnEscapePursuerKilled(e);
            Score+=e.kind>=8?5000:e.kind==2?180:100;Kills++;
            if(e.kind>=8){BeginBossDeath(e);return;}
            e.renderer.gameObject.SetActive(false);e.engine.gameObject.SetActive(false);e.halo.gameObject.SetActive(false);
            Explosion(e.position,e.kind>=8?3:1);shake=Mathf.Max(shake,e.kind>=8?.18f:.045f);
            Play(IonArt.Blast,e.kind>=8?.65f:RandomRange(.9f,1.2f));
            if(!TutorialActive&&e.kind<8)
            {
                LootRolls++;
                if(IonCampaign.RollLoot(random,out PickupKind kind)){EnemyLootDrops++;SpawnCell(e.position,kind);}
            }
            if(Sector>=4)OnExpandedEnemyKilled(e);
            hud.Refresh();
        }
        void Finish(bool won)
        {
            if(runResolved||won&&(Health<=0||State==FlightState.Defeat))return;
            runResolved=true;
            EndUltimate();
            if(won)
            {
                CompleteMissionRewards();
                profile.clearedStages=Mathf.Max(profile.clearedStages,Sector);IonCampaign.Save(profilePath,profile);
                State=Sector==IonCampaign.Stages?FlightState.Victory:FlightState.StageClear;
            }
            else State=FlightState.Defeat;
            SetPlayerVisible(won);SaveBest();ClearHostileBullets();ClearBeams();HideMissionObjects();hud.ShowBanner(false,Sector,SectorName);hud.Refresh();
        }
        public bool UseUltimate()
        {
            if(State!=FlightState.Playing||TutorialActive||UltimateCharges<=0||UltimateActive)return false;
            UltimateCharges--;ClearHostileBullets();ClearBeams();invincible=Mathf.Max(invincible,SelectedHull==2?4:2);shake=.18f;
            CounterUltimatePulse(40);UltimateWorldPulse(40);
            if(State!=FlightState.Playing){hud.Refresh();return true;}
            StartUltimate();hud.Toast(UltimateName+" / "+UltimateDescription);hud.Refresh();return true;
        }
        void SpawnCell(Vector2 p,PickupKind kind)
        {
            Vector2 origin=new Vector2(Mathf.Clamp(p.x,-HalfWidth+1,HalfWidth-1),Mathf.Clamp(p.y,-4.3f,4.1f));p=origin;
            for(int attempt=0;attempt<14;attempt++)
            {
                bool crowded=false;foreach(var other in cells)if(other.active&&(other.position-p).sqrMagnitude<2.6f){crowded=true;break;}
                if(!crowded)break;float angle=attempt*2.4f;
                p=new Vector2(Mathf.Clamp(origin.x+Mathf.Sin(angle)*(1.7f+attempt*.2f),-HalfWidth+1,HalfWidth-1),Mathf.Clamp(origin.y+Mathf.Cos(angle)*1.7f,-4.3f,4.1f));
            }
            foreach(var c in cells)if(!c.active)
            {c.active=true;c.position=p;c.age=0;c.kind=kind;c.renderer.sprite=IonArt.GiftIcons[(int)kind];c.renderer.color=IonCampaign.GiftColors[(int)kind];c.halo.color=c.renderer.color;c.renderer.gameObject.SetActive(true);c.halo.gameObject.SetActive(true);return;}
        }
        void UpdateCells(float dt)
        {
            for(int i=0;i<cells.Length;i++)
            {
                var c=cells[i];if(!c.active){hud.PickupLabel(i,"",Vector2.zero,false);continue;}
                c.age+=dt;float distance=(playerPosition-c.position).magnitude;
                float magnet=ModuleActive(CombatModule.SalvageMagnet)?3.68f:2.3f;
                c.position+=distance<magnet?(playerPosition-c.position).normalized*(4+Mathf.Max(0,2.3f-distance)*4)*(ModuleActive(CombatModule.SalvageMagnet)?1.2f:1)*dt:Vector2.down*.9f*dt;
                c.renderer.transform.position=c.halo.transform.position=c.position;c.halo.transform.localScale=Vector3.one*(.92f+Mathf.Sin(c.age*5)*.1f);
                hud.PickupLabel(i,IonCampaign.GiftNames[(int)c.kind],c.position+Vector2.down*.53f,true);
                if(distance<.6f){ApplyPickup(c.kind);ReleaseCell(i);}
                else if(c.position.y<-6.7f||c.age>20)ReleaseCell(i);
            }
        }
        void ReleaseCell(int index){var c=cells[index];c.active=false;c.renderer.gameObject.SetActive(false);c.halo.gameObject.SetActive(false);hud.PickupLabel(index,"",Vector2.zero,false);}
        void ApplyPickup(PickupKind kind)
        {
            ApplyPickupEffect(kind,true);
        }
        void ApplyPickupEffect(PickupKind kind,bool collected)
        {
            if(collected){GiftsCollected++;Score+=100;}string message=IonCampaign.GiftNames[(int)kind];
            if(kind==PickupKind.Ammo)Ammo=Mathf.Min(IonCampaign.MaxAmmo,Ammo+60);
            else if(kind==PickupKind.Shield){if(Health>=MaxHealth)Score+=150;HealPlayer(30);RepairConvoy();}
            else if(kind==PickupKind.Upgrade){if(!loadout.UpgradePatterns()){Ammo=Mathf.Min(IonCampaign.MaxAmmo,Ammo+30);message="KIỂU BẮN ĐÃ TỐI ĐA / ĐẠN +30";}}
            else if(kind==PickupKind.Ultimate){if(UltimateCharges==1)Ammo=Mathf.Min(IonCampaign.MaxAmmo,Ammo+30);UltimateCharges=1;}
            else if(kind>=PickupKind.Damage)
            {
                if(!loadout.AddStat(kind)){Ammo=Mathf.Min(IonCampaign.MaxAmmo,Ammo+30);message="CHỈ SỐ ĐÃ TỐI ĐA / ĐẠN +30";}
                else message+=" / CỘNG DỒN";
            }
            else
            {
                message=loadout.AddPattern(kind==PickupKind.Spread?GunKind.Spread:kind==PickupKind.Pierce?GunKind.Pierce:GunKind.Homing);
                Ammo=Mathf.Min(IonCampaign.MaxAmmo,Ammo+20);
            }
            Ring(playerPosition,IonCampaign.GiftColors[(int)kind],1.4f,.4f);Play(IonArt.Pickup);hud.Toast(message);hud.Refresh();
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
            CancelDraft();ResetEncounters();
            foreach(var b in bullets)Release(b);
            foreach(var e in enemies){e.active=false;e.dying=false;e.renderer.gameObject.SetActive(false);e.engine.gameObject.SetActive(false);e.halo.gameObject.SetActive(false);}
            foreach(var s in sparks){s.active=false;s.renderer.gameObject.SetActive(false);}
            foreach(var c in cells){c.active=false;c.renderer.gameObject.SetActive(false);c.halo.gameObject.SetActive(false);}
            ClearAttackTarget();ResetSpecialEffects();ResetMission();for(int i=0;i<cells.Length;i++)hud.PickupLabel(i,"",Vector2.zero,false);
            ClearBeams();TutorialActive=false;boss=null;waveLive=false;dashTime=0;shake=0;view.transform.position=new Vector3(0,0,-10);
        }

        // Explicit test hooks exercise real transitions and combat without changing normal runs.
        public Vector2 TestAimTarget()
        {
            if(TestFreezeEnemies)return new Vector2(0,-4.7f);
            if(TestCollectAmmo)foreach(var c in cells)if(c.active&&c.kind==PickupKind.Ammo)return c.position;
            if(Sector>=4&&State==FlightState.Playing&&!MissionBossStarted)return MissionTestAim();
            if(TutorialActive&&TutorialStep==0)return new Vector2(-3,-4.7f);
            if(TutorialActive&&TutorialStep==2){TestDashRequest=DashReady>.99f;return new Vector2(2,-4.7f);}
            if(TutorialActive&&TutorialStep==3)foreach(var c in cells)if(c.active)return c.position;
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
        public void TestClearWave(){foreach(var e in enemies)if(e.active)DamageEnemy(e,10000);}
        public void TestRemoveInvulnerability(){invincible=0;}
        public void TestProtection(float seconds){invincible=seconds;}
        public void TestGivePickup(PickupKind kind)=>ApplyPickup(kind);
        public bool TestFireVolley()=>TryFire();
        public void TestSetAmmo(int value){Ammo=Mathf.Clamp(value,0,IonCampaign.MaxAmmo);}
        public void TestDamageBoss(int damage){if(BossActive)DamageEnemy(boss,damage);}
        public void TestReloadProgress(){profile=IonCampaign.Load(profilePath);hud.Refresh();}
        public void TestGiftDisplay()
        {
            for(int i=0;i<cells.Length;i++)ReleaseCell(i);
            for(int i=0;i<IonCampaign.GiftNames.Length;i++)SpawnCell(new Vector2((i%5-2)*3.5f,i<5?1.8f:-.7f),(PickupKind)i);
        }
        public void TestResetLoadout(){loadout.Reset();hud.Refresh();}
        public void TestClearProjectiles(){foreach(var b in bullets)Release(b);}
        public int TestFriendlyCount {get{int count=0;foreach(var b in bullets)if(b.active&&b.friendly)count++;return count;}}
        public int TestCombinedProjectileCount {get{int count=0;foreach(var b in bullets)if(b.active&&b.friendly&&b.homing&&b.pierce>1)count++;return count;}}
        public int TestPiercingProjectileCount {get{int count=0;foreach(var b in bullets)if(b.active&&b.friendly&&b.pierce>1)count++;return count;}}
        public bool TestFanVelocities {get{bool left=false,right=false;foreach(var b in bullets)if(b.active&&b.friendly){left|=b.velocity.x<-.1f;right|=b.velocity.x>.1f;}return left&&right;}}
        public float TestMaxProjectileDamage {get{float damage=0;foreach(var b in bullets)if(b.active&&b.friendly)damage=Mathf.Max(damage,b.damage);return damage;}}
        public float TestTargetHealth(int index)=>enemies[index].health;
        public void TestTargets()
        {
            ResetCombat();TestFreezeEnemies=true;waveLive=true;Wave=1;transition=1000;playerPosition=new Vector2(0,-4.7f);playerVelocity=Vector2.zero;
            for(int i=0;i<3;i++){var e=SpawnEnemy(i,1,new Vector2(0,-1+i*1.6f),20);SetEnemyVariant(e,0);e.position=e.target;e.entry=e.target;e.age=2;e.renderer.transform.position=e.position;}
        }
        public void TestSpecialTargets()
        {
            ResetCombat();TestFreezeEnemies=true;waveLive=true;Wave=1;transition=1000;playerPosition=new Vector2(0,-4.7f);playerVelocity=Vector2.zero;
            var positions=new[]{new Vector2(0,-1),new Vector2(4,1),new Vector2(0,2.2f)};
            for(int i=0;i<3;i++){var e=SpawnEnemy(i,1,positions[i],200);SetEnemyVariant(e,0);e.position=e.target;e.entry=e.target;e.age=2;e.renderer.transform.position=e.position;}
        }
    }
}
