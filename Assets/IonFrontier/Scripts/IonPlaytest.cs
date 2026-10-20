using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace IonFrontier
{
    // Runs only with the explicit -ion-playtest launch flag. Normal pilot input is never overridden.
    public sealed class IonPlaytest : MonoBehaviour
    {
        [Serializable] public sealed class Report
        {
            public bool passed; public string unityVersion; public string[] checks;
            public int scoreFromNaturalCombat, killsFromNaturalCombat, maxProjectiles;
            public float averageFps, p95FrameMilliseconds; public string[] errors;
        }
        IonGame game; string destination; readonly List<string> checks=new List<string>(),errors=new List<string>();
        readonly List<float> frames=new List<float>(); Report report=new Report(); bool sampling;
        void OnEnable(){Application.logMessageReceived+=Log;}
        void OnDisable(){Application.logMessageReceived-=Log;}
        void Log(string message,string stack,LogType type){if(type==LogType.Exception||type==LogType.Error||type==LogType.Assert)errors.Add(message);}
        void Update()
        {
            if(game==null)return;
            if(game.TestInput&&game.State==FlightState.Playing)game.TestTarget=game.TestAimTarget();
            if(sampling&&Time.unscaledDeltaTime>0&&Time.unscaledDeltaTime<.1f)frames.Add(Time.unscaledDeltaTime);
            report.maxProjectiles=Mathf.Max(report.maxProjectiles,game.ActiveBullets);
        }
        IEnumerator Start()
        {
            game=GetComponent<IonGame>();var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-ion-captures");
            destination=index>=0&&index+1<args.Length?args[index+1]:Application.persistentDataPath;
            Directory.CreateDirectory(destination);report.unityVersion=Application.unityVersion;
            yield return new WaitForSecondsRealtime(1);yield return Capture("01-terminal.png");Check(game.State==FlightState.Menu,"Starts at the new spacecraft terminal");
            Check(UnityEngine.Object.FindFirstObjectByType<IonHud>().SupportsVietnamese(),"Interface font supports all Vietnamese letters and diacritics");
            game.TestInput=true;game.BeginRun();sampling=true;
            yield return new WaitForSecondsRealtime(7);yield return Capture("02-combat.png");
            Check(game.ActiveEnemies>0,"First squadron spawns");Check(game.ActiveBullets>0,"Pooled projectiles are active");
            game.TogglePause();Vector2 position=game.PlayerPosition;float elapsed=game.Elapsed;int projectiles=game.ActiveBullets;
            yield return new WaitForSecondsRealtime(.6f);
            Check(game.State==FlightState.Paused&&game.PlayerPosition==position&&game.Elapsed==elapsed&&game.ActiveBullets==projectiles,"Pause freezes combat and the mission clock");
            yield return Capture("03-pause.png");game.TogglePause();Check(game.State==FlightState.Playing,"Resume returns to combat");
            yield return new WaitForSecondsRealtime(12);
            report.scoreFromNaturalCombat=game.Score;report.killsFromNaturalCombat=game.Kills;
            Check(game.Score>0&&game.Kills>0,"Normal bolts hit ships, destroy targets, and award score");
            Check(game.Shield>0&&game.State==FlightState.Playing,"Pilot survives a natural first-wave combat sample");
            yield return Capture("04-combat-later.png");
            for(int sector=game.Sector;sector<=4;sector++)
            {
                Check(game.Sector==sector,"Sector "+sector+" loaded");game.TestClearWave();
                yield return new WaitForSecondsRealtime(4);
                Check(game.Sector==sector+1&&game.State==FlightState.Playing,"Clear advances to sector "+(sector+1));
                if(sector==3)yield return Capture("05-orbital-blockade.png");
            }
            Check(game.Sector==5&&game.BossFraction>0,"Dreadnought and its hull bar spawn");
            yield return new WaitForSecondsRealtime(3);yield return Capture("06-dreadnought.png");
            Check(game.BossFraction<1&&game.BossFraction>0,"Player bolts damage dreadnought hull");
            game.TestClearWave();yield return new WaitForSecondsRealtime(.6f);
            Check(game.State==FlightState.Victory,"Destroying the command ship completes the mission");yield return Capture("07-victory.png");
            Check(game.ScoreFileExists,"Best score persists to a local save file");
            game.BeginRun();Check(game.Score==0&&game.Shield==5&&game.WeaponTier==1&&game.Sector==1,"Restart resets score, shield, weapons and sector");
            Vector2 dashStart=game.PlayerPosition;game.TestDashRequest=true;
            yield return new WaitForSecondsRealtime(.12f);
            Check(game.DashReady<.15f&&(game.PlayerPosition-dashStart).magnitude>1,"Dash moves the ship and consumes cooldown");
            yield return new WaitForSecondsRealtime(2.6f);
            Check(game.DashReady>.99f,"Dash recharges after its cooldown");
            for(int i=0;i<5;i++){game.TestRemoveInvulnerability();game.HurtPlayer();}
            Check(game.State==FlightState.Defeat&&game.Shield==0,"Five unshielded impacts produce mission defeat");yield return Capture("08-defeat.png");
            game.BeginRun();game.HurtPlayer();Check(game.Shield==5,"Respawn protection rejects immediate repeat damage");
            bool auto=game.AutoFire;game.ToggleAuto();Check(game.AutoFire!=auto,"Auto-fire toggle changes control mode");game.ToggleAuto();
            bool muted=game.Muted;game.ToggleAudio();Check(game.Muted!=muted,"Audio toggle changes the game's audio state");game.ToggleAudio();
            game.ReturnToMenu();Check(game.State==FlightState.Menu&&game.ActiveEnemies==0&&game.ActiveBullets==0,"Return to terminal releases combat pools");
            sampling=false;game.TestInput=false;
            float sum=0;foreach(float f in frames)sum+=f;frames.Sort();
            report.averageFps=frames.Count>0?frames.Count/sum:0;
            report.p95FrameMilliseconds=frames.Count>0?frames[Mathf.Min(frames.Count-1,Mathf.FloorToInt(frames.Count*.95f))]*1000:0;
            Check(report.maxProjectiles<320,"Projectile pool remains below capacity");
            report.errors=errors.ToArray();report.checks=checks.ToArray();report.passed=errors.Count==0;
            File.WriteAllText(Path.Combine(destination,"playtest-report.json"),JsonUtility.ToJson(report,true));
            Debug.Log("ION_PLAYTEST_"+(report.passed?"PASS":"FAIL"));
            yield return new WaitForSecondsRealtime(.5f);Application.Quit(report.passed?0:1);
        }
        IEnumerator Capture(string name){yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(destination,name));yield return new WaitForSecondsRealtime(.12f);}
        void Check(bool condition,string description)
        {checks.Add((condition?"PASS: ":"FAIL: ")+description);if(!condition)errors.Add(description);}
    }
}

