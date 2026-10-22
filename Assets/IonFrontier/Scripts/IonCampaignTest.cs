using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace IonFrontier
{
    // Explicit opt-in harness. A separate profile is selected by IonGame before any save.
    public sealed class IonCampaignTest : MonoBehaviour
    {
        [Serializable] public sealed class Report {public bool passed;public string[] checks,errors;public int maxProjectiles;public float averageFps;}
        readonly List<string> checks=new List<string>(),errors=new List<string>();
        IonGame game;string destination;bool autoPilot=true;float elapsed;int frames,maxBullets;
        void OnEnable(){Application.logMessageReceived+=Log;}
        void OnDisable(){Application.logMessageReceived-=Log;}
        void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
        void Update(){if(game==null)return;if(autoPilot&&game.TestInput)game.TestTarget=game.TestAimTarget();if(Time.unscaledDeltaTime>0){frames++;elapsed+=Time.unscaledDeltaTime;}maxBullets=Mathf.Max(maxBullets,game.ActiveBullets);}
        IEnumerator Start()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-wait-focus")>=0)while(!Application.isFocused)yield return null;
            game=GetComponent<IonGame>();game.TestAutoDraft=true;game.TestInput=true;game.TestFire=false;
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-ion-captures");destination=at>=0?args[at+1]:Application.persistentDataPath;Directory.CreateDirectory(destination);
            yield return new WaitForSecondsRealtime(.5f);yield return Capture("01-hangar-13-stages.png");
            Check(IonArt.PlayerShips.Length==8&&IonCampaign.Stages==13,"Eight complete player sprites and thirteen stage definitions resolve");
            Check(game.UnlockedHulls==1&&game.UnlockedStages==1&&!game.SelectStage(4),"Fresh profile still locks later ships and missions");
            string migration=Path.Combine(destination,"legacy-profile.json");File.WriteAllText(migration,"{\"version\":2,\"clearedStages\":3,\"selectedHull\":2,\"tutorialDone\":true}");
            var old=IonCampaign.Load(migration);Check(old.clearedStages==3&&old.selectedHull==2&&old.stars.Length==13&&IonCampaign.UnlockedStages(old)==4&&IonCampaign.UnlockedHulls(old)==3,"Version-two save migrates without losing three-stage progress or granting bonus ships");
            Check(IonArt.RegularFleet.Length==19&&IonArt.BossFleet.Length==16,"Nineteen regular hulls and sixteen boss hulls resolve");
            var uniqueBosses=new HashSet<Sprite>(IonArt.BossFleet);Check(uniqueBosses.Count==16,"Every boss artwork is a distinct sprite");
            yield return FleetGallery(false);yield return FleetGallery(true);
            game.TestSetProgress(13);game.SelectStage(4);
            for(int variant=7;variant<19;variant++)
            {
                game.BeginRun();game.TestProtection(10000);Check(game.TestFreshEnemyAttack(variant),"New enemy "+IonHostileFleet.Names[variant]+" uses its own sprite and emits its real attack");game.ReturnToMenu();
            }
            game.TestSetProgress(3);
            for(int stage=4;stage<=13;stage++)
            {
                game.ReturnToMenu();Check(game.SelectStage(stage),"Stage "+stage+" unlocks sequentially");game.BeginRun();game.TestProtection(10000);game.TestFire=false;
                Check(game.Ammo==300&&!game.TutorialActive&&!game.MissionBossStarted,"Stage "+stage+" begins its own objective with 300 rounds");
                if(stage==11){yield return Capture("stage-11-route-choice.png");Check(game.ChooseRoute(1)&&!game.ChooseRoute(2),"Route choice accepts one supply route and rejects switching mid-mission");}
                if(stage==4)
                {
                    game.TestFire=true;yield return new WaitForSecondsRealtime(12);game.TestFire=false;
                    Check(game.AmmoSpent>0&&game.EscapeRockWaves>0&&game.EscapeSalvos>0&&game.EscapeFraction>0,"Escape mission naturally advances while spawning moving asteroids and rear pursuit salvos");
                }
                yield return Capture("stage-"+stage+"-mission.png");
                if(stage==5||stage==11)
                {
                    float hp=game.ConvoyHealth;game.TestHitConvoy(30);Check(game.ConvoyHealth<hp,"Escort enemy impacts damage the convoy separately from the player");
                    game.TestGivePickup(PickupKind.Shield);Check(game.ConvoyHealth>hp-30,"Shield gifts repair the escort ship");
                }
                if(stage==6||stage==8||stage==9||stage==13)
                {
                    game.TestMissionClock(220);yield return null;
                    Check(!game.MissionBossStarted,"Stage "+stage+" timer alone cannot bypass its required objective");
                }
                if(stage==6||stage==13)
                {
                    for(int i=0;i<3;i++){game.TestShootObjective();yield return new WaitForSecondsRealtime(.25f);}
                    Check(game.MissionObjectives==3,"Stage "+stage+" generators are destroyed by real pooled projectile collisions");
                }
                if(stage==8||stage==9)
                {
                    autoPilot=false;
                    int count=stage==8?4:3;
                    for(int i=0;i<count;i++)game.TestCaptureNextObjective();
                    Check(game.MissionObjectives==count,"Stage "+stage+" nearby holding interactions complete every rescue or capture objective");autoPilot=true;
                }
                if(stage==7)
                {
                    game.TestMissionClock(220);yield return null;Check(!game.MissionBossStarted,"Chase timer does not replace staying on the target signal");
                    game.TestTrackSignal();Check(game.TestChaseProgress>=55,"Staying in the target lane accumulates actual pursuit progress");
                }
                if(stage==10)
                {
                    game.TestMineChain();yield return new WaitForSecondsRealtime(.3f);Check(game.MissionHazardsDestroyed>=3,"A real bolt triggers a multi-mine chain explosion");
                }
                if(stage==12)
                {
                    game.TestRemoveInvulnerability();float hpBefore=game.Health;game.TestCollapseHit();Check(game.Health<hpBefore,"Collapse front damages a ship below the visible boundary");game.TestProtection(10000);
                }
                game.TestMissionClock(0);
                int beatMask=1|1<<game.MissionBeat;
                for(int work=0;work<80&&!game.MissionBossStarted&&game.State==FlightState.Playing;work++)
                {
                    beatMask|=1<<game.MissionBeat;game.TestWorkMissionBeat();yield return new WaitForSecondsRealtime(.16f);
                }
                Check(beatMask==7&&game.MissionBossStarted&&game.BossActive&&game.DraftOpenCount==2&&game.DraftChoicesMade==2,"Stage "+stage+" requires three objective beats and exactly two checkpoint drafts before its boss");
                int bossCount=stage==5||stage==9?3:stage==11?2:1;
                Check(game.ActiveBossCount==bossCount&&game.TestBossArtMatches,"Stage "+stage+" spawns "+bossCount+" actual bosses with their own artwork");
                yield return new WaitForSecondsRealtime(2.2f);
                game.TestAttackAllBosses();game.TestAttackAllBosses();yield return new WaitForSecondsRealtime(.2f);
                Check(game.BossAttacks>=bossCount&&(game.ActiveBullets>0||game.BeamCasts>0),"Stage "+stage+" every live boss emits actual attacks");
                Check(game.TestClosedBossBlocks(),"Stage "+stage+" closed individual boss shields block damage");
                yield return Capture("stage-"+stage+"-boss.png");
                if(stage==13)
                {
                    yield return BreakBossShield();
                    for(int phase=2;phase<=3;phase++)
                    {
                        game.TestDamageBoss(10000);yield return null;Check(game.BossActive&&game.BossShielded,"Final flagship cannot skip shield phase "+phase+" even with overkill");yield return BreakBossShield();
                    }
                }
                for(int hit=0;hit<70&&game.ActiveBossCount>0;hit++)
                {
                    game.TestRevealBoss();game.TestShootObjective();yield return new WaitForSecondsRealtime(.16f);
                    int previous=game.ActiveBossCount;game.TestHitOpenBoss();
                    if(game.ActiveBossCount<previous&&game.ActiveBossCount>0)
                    {
                        Check(game.State==FlightState.Playing&&game.PendingBossWrecks>0,"Stage "+stage+" surviving bosses fight while a defeated hull collapses");
                        if(stage==11){int beams=game.BeamCasts;game.TestAttackAllBosses();Check(game.BeamCasts>beams,"Surviving twin gains its enraged laser attack");}
                        yield return Capture("stage-"+stage+"-simultaneous-wreck.png");
                    }
                    if(game.ActiveBossCount==previous)game.TestOpenBossClock();
                }
                Check(game.ActiveBossCount==0&&game.State==FlightState.BossDying&&game.ClearedStages==stage-1,"Stage "+stage+" delays progression until all boss wreck sequences finish");
                yield return new WaitForSecondsRealtime(2.5f);yield return Capture("stage-"+stage+"-fragments.png");
                game.TogglePause();float death=game.BossDeathTime;yield return new WaitForSecondsRealtime(.2f);Check(death==game.BossDeathTime,"Stage "+stage+" destruction freezes on pause");game.TogglePause();
                yield return new WaitForSecondsRealtime(2.7f);
                Check(game.BossDestructionsStarted==bossCount&&game.BossDestructionsCompleted==bossCount&&game.PendingBossWrecks==0,"Stage "+stage+" every boss completes its own break-apart and explosion");
                Check(game.State==(stage==13?FlightState.Victory:FlightState.StageClear)&&game.ClearedStages==stage,"Stage "+stage+" saves progression after its full destruction sequence");
                int expected=stage<5?3:stage<7?4:stage<9?5:stage<11?6:stage<13?7:8;
                Check(game.UnlockedHulls==expected,"Stage "+stage+" obeys the every-two-new-stages ship reward schedule");
                Check(game.StarsAt(stage)>=1,"Stage "+stage+" records a persistent completion rating");yield return Capture("stage-"+stage+"-clear.png");
            }
            game.TestReloadProgress();Check(game.ClearedStages==13&&game.UnlockedHulls==8&&game.UnlockedStages==13,"Full campaign, all eight hulls and all thirteen stages survive a save reload");
            game.ReturnToMenu();game.SelectStage(11);game.BeginRun();game.TestProtection(10000);Check(game.ChooseRoute(2),"The alternate rescue route remains selectable on replay");
            for(int i=0;i<30&&!game.MissionBossStarted;i++){game.TestWorkMissionBeat();yield return new WaitForSecondsRealtime(.16f);}game.TestCaptureNextObjective();Check(game.MissionObjectives==4&&game.TestRescueAllies==4,"Rescue branch creates four live support allies");yield return Capture("stage-11-rescue-route.png");
            game.ReturnToMenu();game.SelectStage(5);game.BeginRun();game.TestHitConvoy(1000);Check(game.State==FlightState.Defeat&&game.ClearedStages==13,"Losing the convoy fails the mission without erasing campaign unlocks");
            game.ReturnToMenu();game.SelectStage(4);
            float baseDamage=-1,baseDelay=-1;int baseCount=-1;
            for(int hull=0;hull<8;hull++)
            {
                Check(game.SelectHull(hull),"Unlocked player hull "+hull+" is selectable");game.BeginRun();game.TestFire=false;game.TestSpecialTargets();game.TestFireVolley();
                if(hull==0){baseDamage=game.LastVolleyDamage;baseDelay=game.LastVolleyDelay;baseCount=game.LastVolleyCount;}
                Check(Mathf.Abs(game.LastVolleyDamage-baseDamage*IonHullStats.Damage[hull])<.001f&&Mathf.Abs(game.LastVolleyDelay-baseDelay/IonHullStats.Rate[hull])<.001f&&game.LastVolleyCount==baseCount,"Hull "+hull+" applies its own damage and attack-speed multipliers");
                game.TestClearProjectiles();game.TestGivePickup(PickupKind.Ultimate);int ammo=game.Ammo;float hp=game.TestTotalEnemyHealth;
                Check(game.UseUltimate()&&game.LastUltimateHull==hull,"Hull "+hull+" starts its distinct ultimate");
                if(hull==5)game.TestMagnetBolts();
                if(hull==7)game.TestSlowBolts();
                yield return new WaitForSecondsRealtime(.7f);yield return Capture("ultimate-"+hull+".png");
                if(hull==7)Check(game.TestSlowRatio<.6f,"Void field slows a real hostile bolt inside the field while an outside control bolt keeps full speed");
                game.TogglePause();float time=game.UltimateElapsed;yield return new WaitForSecondsRealtime(.2f);Check(time==game.UltimateElapsed,"Hull "+hull+" ultimate freezes on pause");game.TogglePause();
                yield return new WaitForSecondsRealtime(4.2f);
                Check(game.TestTotalEnemyHealth<hp&&game.Ammo==ammo&&!game.UltimateActive,"Hull "+hull+" ultimate deals real damage without gun ammo and terminates");
                if(hull==5)Check(game.AbsorbedBolts>0,"Magnetic ultimate absorbs real hostile projectiles before retaliation");
                game.ReturnToMenu();Check(!game.UltimateActive&&game.ActiveWorldCount==0&&game.ActiveBullets==0,"Hull "+hull+" releases all effects, mission objects and projectiles on return");
            }
            yield return Capture("all-eight-ships-unlocked.png");
            Check(maxBullets<512,"Expanded campaign remains below pooled projectile capacity");
            var report=new Report{passed=errors.Count==0,checks=checks.ToArray(),errors=errors.ToArray(),averageFps=elapsed>0?frames/elapsed:0,maxProjectiles=maxBullets};
            File.WriteAllText(Path.Combine(destination,"campaign-report.json"),JsonUtility.ToJson(report,true));Debug.Log("ION_CAMPAIGN_TEST_"+(report.passed?"PASS":"FAIL"));yield return new WaitForSecondsRealtime(.3f);Application.Quit(report.passed?0:1);
        }
        IEnumerator FleetGallery(bool bosses)
        {
            var overlay=new GameObject("QA fleet gallery");var canvas=overlay.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
            var scaler=overlay.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);
            var background=new GameObject("Gallery background",typeof(RectTransform),typeof(Image));background.transform.SetParent(overlay.transform,false);
            var bg=(RectTransform)background.transform;bg.anchorMin=Vector2.zero;bg.anchorMax=Vector2.one;bg.offsetMin=bg.offsetMax=Vector2.zero;background.GetComponent<Image>().color=new Color(.015f,.025f,.05f);
            int count=bosses?16:12,cols=bosses?8:6;var font=Resources.Load<Font>("Interface");
            for(int i=0;i<count;i++)
            {
                float x=100+(i%cols)*(1400f/(cols-1)),y=i<cols?560:170;
                var item=new GameObject("Hull",typeof(RectTransform),typeof(Image));item.transform.SetParent(overlay.transform,false);var r=(RectTransform)item.transform;r.anchorMin=r.anchorMax=Vector2.zero;r.anchoredPosition=new Vector2(x,y+60);r.sizeDelta=new Vector2(bosses?165:200,295);var im=item.GetComponent<Image>();im.sprite=bosses?IonArt.BossFleet[i]:IonArt.RegularFleet[i+7];im.preserveAspect=true;
                var label=new GameObject("Name",typeof(RectTransform),typeof(Text));label.transform.SetParent(overlay.transform,false);r=(RectTransform)label.transform;r.anchorMin=r.anchorMax=Vector2.zero;r.anchoredPosition=new Vector2(x,y-120);r.sizeDelta=new Vector2(190,50);var text=label.GetComponent<Text>();text.font=font;text.fontSize=16;text.alignment=TextAnchor.MiddleCenter;text.color=Color.white;text.text=bosses?(i<13?"MÀN "+(i+1)+" / "+IonCampaign.BossNames[i]:new[]{"KÌM HÃM","LÕI VỆ","SONG SÁT LAM"}[i-13]):IonHostileFleet.Names[i+7];
            }
            yield return Capture(bosses?"fleet-16-new-bosses.png":"fleet-12-new-enemies.png");Destroy(overlay);yield return null;
        }
        IEnumerator BreakBossShield(){for(int i=0;i<10&&game.BossShielded;i++){game.TestRevealBoss();game.TestShootObjective();yield return new WaitForSecondsRealtime(.22f);}}
        IEnumerator Capture(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(destination,name));yield return new WaitForSecondsRealtime(.1f);}
        void Check(bool pass,string message){checks.Add((pass?"PASS: ":"FAIL: ")+message);if(!pass){errors.Add(message);Debug.LogWarning("CAMPAIGN_CHECK_FAIL: "+message);}}
    }
}
