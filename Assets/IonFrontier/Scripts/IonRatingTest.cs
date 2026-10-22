using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace IonFrontier
{
    public sealed class IonRatingTest : MonoBehaviour
    {
        [Serializable] public sealed class Report {public bool passed;public string[] checks,errors;public float averageFps;public int maxProjectiles;}
        readonly List<string> checks=new List<string>(),errors=new List<string>();
        IonGame game;string destination;int frames,maxBolts;float elapsed;
        void OnEnable(){Application.logMessageReceived+=Log;}
        void OnDisable(){Application.logMessageReceived-=Log;}
        void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(text);}
        void Update(){if(Time.unscaledDeltaTime>0){frames++;elapsed+=Time.unscaledDeltaTime;}if(game!=null)maxBolts=Mathf.Max(maxBolts,game.ActiveBullets);}
        IEnumerator Start()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-wait-focus")>=0)while(!Application.isFocused)yield return null;
            game=GetComponent<IonGame>();game.TestAutoDraft=true;game.TestInput=true;game.TestFire=false;var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-ion-captures");destination=at>=0?args[at+1]:Application.persistentDataPath;Directory.CreateDirectory(destination);
            yield return new WaitForSecondsRealtime(.4f);Check(game.UnlockedStages==1&&game.StarsAt(4)==0,"Rating QA uses an isolated fresh profile");
            string legacy=Path.Combine(destination,"legacy-v3.json");File.WriteAllText(legacy,"{\"version\":3,\"clearedStages\":7,\"selectedHull\":4,\"tutorialDone\":true,\"stars\":[3,2,1,3]}");
            var migrated=IonCampaign.Load(legacy);Check(migrated.version==4&&migrated.clearedStages==7&&migrated.selectedHull==4&&migrated.tutorialDone,"Version3 migration preserves progression, hull and tutorial");
            Check(migrated.stars.Length==13&&migrated.stars[0]==3&&migrated.stars[3]==3&&migrated.stars[12]==0,"Old three-star awards remain earned counts without inventing extra stars");
            migrated.stars[3]=5;IonCampaign.Save(legacy,migrated);Check(IonCampaign.Load(legacy).stars[3]==5,"A five-star record survives a full save and reload");
            migrated.stars[0]=99;migrated.stars[1]=-2;IonCampaign.Save(legacy,migrated);migrated=IonCampaign.Load(legacy);Check(migrated.stars[0]==5&&migrated.stars[1]==0,"Version4 ratings clamp corrupt counts to the valid0–5 range");
            game.TestSetProgress(13);
            for(int stage=1;stage<=13;stage++){game.SelectStage(stage);Check(!string.IsNullOrEmpty(game.RatingSecondaryGoal)&&game.BossParSeconds>=75,"Stage "+stage+" exposes a secondary goal and boss time target");}
            game.SelectStage(4);yield return Capture("00-hangar-five-star-button.png");game.TestShowRatingGuide();yield return Capture("01-stage4-five-star-goals.png");game.TestHideRatingGuide();game.BeginRun();game.TestFire=false;game.TestFreezeEnemies=true;game.TestProtection(10000);
            yield return new WaitForSecondsRealtime(.3f);Check(game.BossCombatSeconds==0&&game.DamageTaken==0&&game.LastStars==0,"A new run resets rating counters and does not time the pre-boss pursuit");
            game.TestEscapeHitAsteroid();Check(game.DamageTaken==18&&game.EscapeRocksCleared==0,"Asteroid impact records real HP lost without awarding an offensive asteroid clear");
            game.TestProtection(10000);game.HealPlayer(999);Check(game.DamageTaken==18&&game.Health==game.MaxHealth,"Repair restores HP while cumulative damage remains recorded");
            game.TestRatingShootRock();yield return new WaitForSecondsRealtime(.2f);Check(game.EscapeRocksCleared==1,"A real friendly projectile earns asteroid objective credit");
            game.TestRatingClearRocks(17);Check(game.EscapeRocksCleared==18&&game.RatingSecondaryMet,"Weapon/Q asteroid clears reach the eighteen-rock side objective");
            game.TestRatingBeginBoss();yield return new WaitForSecondsRealtime(.65f);float combat=game.BossCombatSeconds;Check(combat>.5f&&combat<1,"The boss clock starts only when the actual encounter begins");
            game.TogglePause();yield return new WaitForSecondsRealtime(.35f);Check(game.BossCombatSeconds==combat,"Pause freezes boss rating time");game.TogglePause();
            game.TestRatingBossTime(game.BossParSeconds);Check(game.RatingCriterionMet(4),"The exact boss time threshold still qualifies");game.TestRatingBossTime(game.BossParSeconds+.01f);Check(!game.RatingCriterionMet(4),"Exceeding the boss time threshold loses only that criterion");
            game.ReturnToMenu();game.SelectStage(4);game.BeginRun();game.TestFreezeEnemies=true;game.TestProtection(10000);game.TestRatingClearRocks(18);game.TestRatingBeginBoss();yield return new WaitForSecondsRealtime(.2f);game.TestDamageBoss(10000);
            Check(game.State==FlightState.BossDying&&game.StarsAt(4)==0,"Stars are not saved while the final boss wreck is still exploding");float stopped=game.BossCombatSeconds;yield return new WaitForSecondsRealtime(5.1f);
            Check(game.State==FlightState.StageClear&&game.LastStars==5&&game.LastRatingMask==31,"A complete run satisfying all five independent goals earns exactly five stars");
            Check(game.StarsAt(4)==5&&game.TestReloadRating(4)==5&&game.LastWasNewRating,"The new five-star record is saved and can be reloaded");
            Check(game.BossCombatSeconds==stopped,"The final collapse animation is excluded from boss rating time");yield return Capture("02-five-star-debrief.png");
            game.BeginRun();game.TestFreezeEnemies=true;game.TestProtection(10000);Check(game.DamageTaken==0&&game.BossCombatSeconds==0&&game.LastRatingMask==0&&game.EscapeRocksCleared==0,"Replaying from the result resets all rating and secondary counters");
            game.TestRatingClearRocks(18);game.TestRatingBeginBoss();game.TestRemoveInvulnerability();game.HurtPlayer(60);game.HealPlayer(999);game.TestProtection(10000);game.TestDamageBoss(10000);yield return new WaitForSecondsRealtime(5.1f);
            Check(game.LastStars==4&&game.LastRatingEarned(1)&&!game.LastRatingEarned(2),"Healing back to full HP cannot restore the lost cumulative-damage star");
            Check(game.StarsAt(4)==5&&game.TestReloadRating(4)==5&&!game.LastWasNewRating,"A weaker replay keeps the historical five-star best");yield return Capture("03-four-stars-best-preserved.png");
            game.BeginRun();game.TestFreezeEnemies=true;game.TestProtection(10000);game.TestRatingBeginBoss();game.TestRemoveInvulnerability();game.HurtPlayer(70);game.TestProtection(10000);game.TestRatingBossTime(game.BossParSeconds+1);game.TestDamageBoss(10000);yield return new WaitForSecondsRealtime(5.1f);
            Check(game.LastStars==1&&game.LastRatingMask==1,"Completion alone earns one star when all four bonus criteria are missed");yield return Capture("04-one-star-debrief.png");
            game.BeginRun();game.TestRemoveInvulnerability();game.HurtPlayer(10000);Check(game.State==FlightState.Defeat&&game.LastStars==0&&game.TestReloadRating(4)==5,"Defeat grants no new stars and preserves the previous best");
            game.ReturnToMenu();game.SelectStage(1);game.BeginRun();game.TestFreezeEnemies=true;game.TestProtection(10000);game.TestRatingBeginBoss();game.TestDamageBoss(10000);yield return new WaitForSecondsRealtime(5.1f);Check(game.LastStars==5&&game.StarsAt(1)==5,"The opening chapter also uses the five-star rating system");
            var report=new Report{passed=errors.Count==0,checks=checks.ToArray(),errors=errors.ToArray(),averageFps=elapsed>0?frames/elapsed:0,maxProjectiles=maxBolts};File.WriteAllText(Path.Combine(destination,"rating-report.json"),JsonUtility.ToJson(report,true));Debug.Log("ION_RATING_TEST_"+(report.passed?"PASS":"FAIL"));yield return new WaitForSecondsRealtime(.2f);Application.Quit(report.passed?0:1);
        }
        IEnumerator Capture(string name){yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(destination,name));yield return new WaitForSecondsRealtime(.1f);}
        void Check(bool pass,string text){checks.Add((pass?"PASS: ":"FAIL: ")+text);if(!pass){errors.Add(text);Debug.LogWarning("RATING_FAIL: "+text);}}
    }
    public sealed partial class IonGame
    {
        public void TestRatingBeginBoss(){if(Sector<=3){Wave=3;SpawnWave();}else SpawnExpandedBoss();}
        public void TestRatingBossTime(float seconds){BossCombatSeconds=seconds;}
        public int TestReloadRating(int stage)=>IonCampaign.Load(profilePath).stars[stage-1];
        public void TestRatingClearRocks(int count){for(int i=0;i<count;i++)SpawnWorld(WorldKind.Rock,new Vector2(5,2),5,.6f);UltimateWorldPulse(40);}
        public void TestRatingShootRock(){foreach(var o in world)if(o.active&&o.kind==WorldKind.Rock)ReleaseWorld(o);SpawnWorld(WorldKind.Rock,new Vector2(0,1),5,.6f);var shot=Bolt(new Vector2(0,-.5f),Vector2.up*16,true);if(shot!=null){shot.damage=10;shot.pierce=1;shot.homing=false;}}
        public void TestShowRatingGuide(){hud.ShowRatingGuide();}
        public void TestHideRatingGuide(){hud.HideRatingGuide();}
    }
}
