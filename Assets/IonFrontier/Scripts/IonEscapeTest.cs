using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace IonFrontier
{
    public sealed class IonEscapeTest : MonoBehaviour
    {
        [Serializable] public sealed class Report {public bool passed;public string[] checks,errors;public float averageFps;public int maxProjectiles;}
        readonly List<string> checks=new List<string>(),errors=new List<string>();
        IonGame game;string destination;int frames,maxBolts;float elapsed;bool dodge=true;
        void OnEnable(){Application.logMessageReceived+=Log;}
        void OnDisable(){Application.logMessageReceived-=Log;}
        void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(text);}
        void Update()
        {
            if(Time.unscaledDeltaTime>0){frames++;elapsed+=Time.unscaledDeltaTime;}
            if(game==null)return;maxBolts=Mathf.Max(maxBolts,game.ActiveBullets);
            if(dodge&&game.EscapeRunActive&&game.State==FlightState.Playing)game.TestTarget=new Vector2(Mathf.Sin(game.MissionTime*.9f)*5,-1.8f);
        }
        IEnumerator Start()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-wait-focus")>=0)while(!Application.isFocused)yield return null;
            game=GetComponent<IonGame>();game.TestAutoDraft=true;game.TestInput=true;game.TestFire=false;var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-ion-captures");destination=at>=0?args[at+1]:Application.persistentDataPath;Directory.CreateDirectory(destination);
            yield return new WaitForSecondsRealtime(.4f);Check(game.UnlockedStages==1,"Escape QA starts with an isolated fresh profile");game.TestSetProgress(3);Check(game.SelectStage(4),"Stage4 is available after clearing the opening chapter");game.BeginRun();game.TestFire=false;game.TestProtection(10000);
            Check(game.EscapeRunActive&&game.EscapeFraction==0&&game.MissionBeat==0&&game.Ammo==300,"Escape starts at0% with stage supplies and first pursuit beat");
            Check(game.TestEscapeForbiddenObjectives==0,"The old generators and mandatory priority targets are absent");
            yield return Capture("01-escape-start.png");yield return new WaitForSecondsRealtime(8.5f);
            Check(game.EscapeFraction>.1f&&game.EscapeFraction<.2f&&game.Kills==0&&game.AmmoSpent==0,"Real escape distance advances without shooting or killing enemies");
            Check(game.EscapeRockWaves>=4&&game.EscapeCrossingWaves>0&&game.TestEscapeMovingRocks>0,"Dense asteroid waves and a crossing formation appear naturally during the opening");
            Check(game.EscapePursuersAlive==2&&game.EscapeWarningEvents>=4&&game.EscapeSalvos>=4,"Two rear pursuers enter early and deliver at least four warned salvos within the opening 8.5s");
            Check(game.TestEscapeTailShotsUpward>0,"Pursuit projectiles approach from behind toward the player's flight area");yield return Capture("02-asteroids-and-pursuer.png");
            float distance=game.EscapeDistance;int salvos=game.EscapeSalvos;game.TogglePause();yield return new WaitForSecondsRealtime(.35f);
            Check(game.EscapeDistance==distance&&game.EscapeSalvos==salvos,"Pause freezes both route progress and pursuit firing");game.TogglePause();
            game.TestMissionClock(999);yield return null;Check(!game.MissionBossStarted&&game.EscapeFraction<.25f,"Changing the legacy mission clock cannot skip escape progress");
            game.TestMissionClock(9);float before=game.EscapeDistance;game.TestEscapeOneWave();Check(game.TestEscapeLastWaveHasGap,"Every new asteroid formation leaves an actual navigable gap");
            game.TestShootObjective();yield return new WaitForSecondsRealtime(.15f);Check(game.EscapeDistance>before,"Escape continues rather than waiting for destructible objectives");
            dodge=false;game.TestEscapeHitAsteroid();Check(game.Health<game.MaxHealth,"A real asteroid collision subtracts player HP");game.HealPlayer(999);game.TestProtection(10000);dodge=true;
            int kills=game.EscapePursuerKills;game.TestEscapeKillPursuer();Check(game.EscapePursuerKills==kills+1&&game.TestEscapeKilledWarningCleared,"Destroying a pursuer removes its pending firing warning");
            yield return new WaitForSecondsRealtime(1);Check(game.EscapePursuersAlive==1,"Killed pursuers do not respawn immediately while the survivor remains");
            game.TestEscapeAdvanceTo(1250);Check(game.MissionBeat==1&&game.EscapeFraction>1f/3&&game.EscapeFraction<.4f,"First route checkpoint enters the denser asteroid beat without resetting progress");
            yield return new WaitForSecondsRealtime(1.8f);yield return Capture("03-escape-middle.png");
            game.TestEscapeAdvanceTo(2500);Check(game.MissionBeat==2&&game.EscapeFraction>2f/3,"Second checkpoint enters the final breakout beat");
            yield return new WaitForSecondsRealtime(1.6f);Check(game.CounterActive&&game.CounterEnginesAlive==2,"The final stretch enters the optional elite counterattack with two live engines");yield return Capture("04-final-counterattack.png");
            game.TestEscapeAdvanceTo(3590);Check(!game.MissionBossStarted&&game.EscapeFraction<1,"Boss cannot begin before the full escape route is reached");
            float end=Time.realtimeSinceStartup+2;while(game.EscapeFraction<1&&Time.realtimeSinceStartup<end)yield return null;
            Check(game.EscapeFraction==1&&!game.MissionBossStarted&&game.ClearedStages==3,"100% arrival is shown before the blocking boss and does not grant an early win");yield return Capture("05-arrived-100-percent.png");
            end=Time.realtimeSinceStartup+2;while(!game.BossActive&&Time.realtimeSinceStartup<end)yield return null;
            Check(game.BossActive&&game.MissionBossStarted&&!game.EscapeRunActive&&game.EscapeWarningsAlive==0,"Arrival transitions once into the boss encounter and clears rear warnings");
            Check(game.TestEscapeMovingRocks==0&&game.TestEscapeForbiddenObjectives==0,"Old escape hazards and removed objectives do not leak into boss entry");
            yield return new WaitForSecondsRealtime(2);yield return Capture("06-blocking-boss.png");game.TestDamageBoss(10000);
            Check(game.State==FlightState.BossDying&&game.ClearedStages==3,"The blocking boss still must collapse before progression is saved");yield return new WaitForSecondsRealtime(5.1f);
            Check(game.State==FlightState.StageClear&&game.ClearedStages==4&&game.UnlockedStages==5&&game.BossDestructionsCompleted==1,"Boss destruction completes stage4 and opens stage5");
            game.ReturnToMenu();game.SelectStage(4);game.BeginRun();Check(game.EscapeDistance==0&&game.EscapeSalvos==0&&game.EscapePursuersAlive==0,"Replay resets route progress, salvos and pursuers");
            game.TestProtection(10000);game.TestFire=false;dodge=true;yield return new WaitForSecondsRealtime(55);
            Check(game.MissionBeat==2&&game.EscapeFraction>.7f&&game.EscapeFraction<.9f&&game.CounterActive&&game.CounterEnginesAlive==2,"A full natural run reaches the elite counterattack without accelerated route hooks");
            end=Time.realtimeSinceStartup+6;while(game.CounterWarningsAlive==0&&Time.realtimeSinceStartup<end)yield return null;
            Check(game.CounterWarningsAlive>0,"The natural elite counterattack shows a locked firing warning");yield return Capture("07-natural-counterattack-warning.png");
            Check(game.EscapeRockWaves>=20&&game.EscapeSalvos>=20&&game.AmmoSpent==0,"Natural asteroid and gunfire pressure continues throughout an unarmed escape");
            end=Time.realtimeSinceStartup+23;while(!game.BossActive&&Time.realtimeSinceStartup<end)yield return null;
            Check(game.BossActive&&game.EscapeFraction==1&&game.CounterExpired&&!game.CounterSucceeded&&game.DraftOpenCount==2,"The unarmed natural route skips the optional elite and still reaches the blocking boss");
            game.ReturnToMenu();game.SelectStage(4);game.BeginRun();game.TestProtection(10000);game.TestEscapeForcePursuers();game.TestEscapeArmWarning();Check(game.EscapeWarningsAlive>0,"A pursuing ship can enter an active warning state");game.ReturnToMenu();
            Check(!game.EscapeRunActive&&game.TestEscapeVisibleWarnings==0&&game.EscapePursuersAlive==0,"Returning to the hangar removes escape visuals and enemies");
            game.SelectStage(5);game.BeginRun();Check(!game.EscapeRunActive&&game.MissionConvoyActive,"Stage5 retains its original convoy mission");
            game.ReturnToMenu();game.SelectStage(4);game.BeginRun();game.TestRemoveInvulnerability();game.HurtPlayer(10000);float lost=game.EscapeDistance;game.TestEscapeAdvanceTo(1000);
            Check(game.State==FlightState.Defeat&&game.EscapeDistance==lost,"A defeated ship cannot continue gaining route progress");
            Check(maxBolts<512,"Rear pursuit combat stays below the projectile pool limit");
            var report=new Report{passed=errors.Count==0,checks=checks.ToArray(),errors=errors.ToArray(),averageFps=elapsed>0?frames/elapsed:0,maxProjectiles=maxBolts};File.WriteAllText(Path.Combine(destination,"escape-report.json"),JsonUtility.ToJson(report,true));Debug.Log("ION_ESCAPE_TEST_"+(report.passed?"PASS":"FAIL"));yield return new WaitForSecondsRealtime(.2f);Application.Quit(report.passed?0:1);
        }
        IEnumerator Capture(string name){yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(destination,name));yield return new WaitForSecondsRealtime(.1f);}
        void Check(bool pass,string text){checks.Add((pass?"PASS: ":"FAIL: ")+text);if(!pass){errors.Add(text);Debug.LogWarning("ESCAPE_FAIL: "+text);}}
    }
    public sealed partial class IonGame
    {
        public int TestEscapeForbiddenObjectives=>WorldAlive(WorldKind.Generator)+MissionMarkedAlive;
        public int TestEscapeMovingRocks{get{int n=0;foreach(var o in world)if(o.active&&o.kind==WorldKind.Rock&&o.velocity.y<0)n++;return n;}}
        public int TestEscapeTailShotsUpward{get{int n=0;foreach(var b in bullets)if(b.active&&!b.friendly&&b.velocity.y>0)n++;return n;}}
        public int TestEscapeVisibleWarnings{get{int n=0;foreach(var sr in escapeWarnings)if(sr!=null&&sr.gameObject.activeSelf)n++;return n;}}
        public bool TestEscapeLastWaveHasGap {get;private set;}
        public void TestEscapeOneWave()
        {
            foreach(var o in world)if(o.active&&o.kind==WorldKind.Rock)ReleaseWorld(o);SpawnEscapeRocks();int consecutive=0,best=0;for(float x=-HalfWidth+.8f;x<=HalfWidth-.8f;x+=.5f){bool clear=true;foreach(var o in world)if(o.active&&o.kind==WorldKind.Rock&&Mathf.Abs(x-o.position.x)<o.radius+.4f){clear=false;break;}consecutive=clear?consecutive+1:0;best=Mathf.Max(best,consecutive);}TestEscapeLastWaveHasGap=best>=5;
        }
        public void TestEscapeHitAsteroid(){TestRemoveInvulnerability();SpawnWorld(WorldKind.Rock,playerPosition,4,.6f);UpdateMission(.01f);}
        public bool TestEscapeKilledWarningCleared {get;private set;}
        public void TestEscapeKillPursuer(){foreach(var e in enemies)if(e.active&&e.escapePursuer){DamageEnemy(e,10000);TestEscapeKilledWarningCleared=!e.escapeWarning&&!escapeWarnings[e.escapeLane].gameObject.activeSelf;return;}}
        public void TestEscapeAdvanceTo(float distance){distance=Mathf.Min(distance,EscapeRouteLength);for(int i=0;i<400&&State==FlightState.Playing&&!missionBossStarted&&escapeTravel<distance;i++){float step=Mathf.Min(.25f,(distance-escapeTravel)/50);UpdateMission(step);TickEscapeMission(step);}}
        public void TestEscapeForcePursuers(){while(EscapePursuersAlive<2&&EscapeRunActive)SpawnEscapePursuer();}
        public void TestEscapeArmWarning(){foreach(var e in enemies)if(e.active&&e.escapePursuer){e.age=2;e.fire=.8f;UpdateEscapePursuer(e,.01f);return;}}
    }
}
