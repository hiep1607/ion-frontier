using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace IonFrontier
{
    public sealed class IonClarityTest : MonoBehaviour
    {
        [Serializable] sealed class Report {public bool passed;public string[] checks,errors;}
        readonly List<string> checks=new List<string>(),errors=new List<string>();IonGame game;string folder;bool paused;
        void OnEnable(){Application.logMessageReceived+=Log;}void OnDisable(){Application.logMessageReceived-=Log;}
        void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
        void Update(){if(game!=null&&game.DraftActive&&game.State==FlightState.Paused)paused=true;}
        void Begin(int stage=4,int cleared=13){game.ReturnToMenu();game.TestSetProgress(cleared);game.SelectStage(stage);game.SelectHull(0);game.TestDraftSeed=11101;game.BeginRun();game.TestFreezeEnemies=true;game.TestInput=true;game.TestFire=false;game.TestAutoDraft=false;game.TestHideBanner();}
        IEnumerator Start()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-wait-focus")>=0)while(!Application.isFocused)yield return null;
            game=GetComponent<IonGame>();var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-ion-captures");folder=at>=0?args[at+1]:Application.persistentDataPath;Directory.CreateDirectory(folder);yield return new WaitForSecondsRealtime(.3f);
            if(Array.IndexOf(args,"-ion-clarity-stress")>=0){Begin(9);game.TestClarityStress();yield return new WaitForSecondsRealtime(12);Check(game.State==FlightState.Playing&&game.ActiveBossCount==3&&game.ActiveEnemies>=20&&game.ActiveBullets>0&&game.ActiveBullets<=512,"Full Update stress sustains three bosses, at least twenty ships and maximum upgraded fire within the pools");yield return Capture("01-three-boss-stress.png");game.TogglePause();game.TestClarityModuleLegend();yield return Capture("02-nine-module-symbols.png");Finish("stress-report.json");yield break;}
            if(Array.IndexOf(args,"-ion-clarity-input")>=0){yield return PhysicalInput();Finish("input-report.json");yield break;}
            Begin(4,3);Check(game.TestClarityTerminalRace(),"Defeat is terminal even when a pending player bolt and late boss-death callback execute");
            Begin();Check(game.TestClarityWorldAfterLoss(),"Lethal asteroid contact prevents later world damage, route progress and checkpoint rewards");
            string[] scenarios={"Owner killed during charge cancels only its laser","Owner killed while firing cancels its laser","Reused enemy slot cannot keep the previous laser","Unexpected serial mismatch retires the stale laser","Source retreat cancels its dependent beam","Killing a source without a laser preserves unrelated beams"};
            for(int i=0;i<scenarios.Length;i++){Begin(6);Check(game.TestClarityBeam(i),scenarios[i]);}
            Begin(6);Check(game.TestClarityBeamDamageSlice(),"Laser damage integrates only the active firing slice after its telegraph");
            for(int i=0;i<3;i++){Begin();Check(game.TestClarityInputCallback(i),"Real UI button callback "+(i+1)+" selects one card without pickup credit or duplicate award");game.TestResolveDraft();}
            Begin();yield return null;yield return new WaitForEndOfFrame();game.TestOpenDraft(0);game.ChooseUpgrade(1);string snapshot=game.TestDraftSimulationSnapshot();game.TogglePause();yield return new WaitForSecondsRealtime(.3f);game.TogglePause();Check(game.DraftActive&&game.DraftSelected==1&&game.TestDraftSimulationSnapshot()==snapshot,"Pause after selection preserves the chosen reward and frozen resume countdown");yield return new WaitForSecondsRealtime(.7f);Check(game.State==FlightState.Playing&&game.AmmoSpent==0,"The real resume delay ends without an accidental player shot");
            Begin();Check(game.TestClarityPreview(),"Pattern replacement preview reports the suspended module without mutating the live loadout");
            Begin();Check(game.TestClarityModuleCounters(),"HUD reads real pulse progress and reactive-armor cooldown");
            Begin(1);game.TestClarityHudSample();yield return new WaitForSecondsRealtime(.45f);yield return Capture("01-module-status.png");game.TestFire=false;
            Begin();game.TestClarityModuleDraft();yield return Capture("02-module-cards-16x9.png");Screen.SetResolution(1024,768,false);yield return new WaitForSecondsRealtime(.3f);yield return Capture("03-module-cards-4x3.png");Screen.SetResolution(1280,720,false);game.TestResolveDraft();
            Begin(11);game.ChooseRoute(2);game.TestFreezeEnemies=false;game.TestStartEncounter(1);game.TestFreezeEnemies=false;game.TestProtection(0);game.TestTarget=new Vector2(0,-3);yield return new WaitForSecondsRealtime(3.3f);game.TestClarityDashCooldown();game.TestTarget=new Vector2(-7,-3);yield return new WaitForSecondsRealtime(2.1f);Check(game.EncounterLaserEvents>0&&game.State==FlightState.Playing&&game.DamageTaken==0&&game.DashReady<.99f,"Normal movement escapes live tractor/laser on the full Update path with dash unavailable and no protection");yield return Capture("04-live-tractor-laser.png");
            Finish("clarity-report.json");
        }
        IEnumerator PhysicalInput()
        {
            for(int i=0;i<6;i++)
            {
                Begin();game.TestPrepareClarityInput();game.TestDraftInstruction=(i<3?"BẤM CHUỘT THẺ ":"NHẤN PHÍM ")+(i%3+1)+(i==0?" • KIỂM TRA P / ESC TRƯỚC":"");game.TestOpenDraft(0);
                float deadline=Time.realtimeSinceStartup+300;while(game.DraftActive&&Time.realtimeSinceStartup<deadline)yield return null;
                Check(game.State==FlightState.Playing&&game.DraftSelected==i%3&&game.DraftChoicesMade==1&&game.AmmoSpent==0,"Physical "+(i<3?"mouse":"keyboard")+" selects card "+(i%3+1)+" once and does not fire through the modal");yield return new WaitForSecondsRealtime(.4f);
            }
            Check(paused,"Physical pause and resume preserve the pending draft");game.TestDraftInstruction="";
        }
        IEnumerator Capture(string name){yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,name));yield return new WaitForSecondsRealtime(.15f);}
        void Check(bool pass,string message){checks.Add((pass?"PASS: ":"FAIL: ")+message);if(!pass){errors.Add(message);Debug.LogWarning("CLARITY_FAIL: "+message);}}
        void Finish(string file){var report=new Report{passed=errors.Count==0,checks=checks.ToArray(),errors=errors.ToArray()};File.WriteAllText(Path.Combine(folder,file),JsonUtility.ToJson(report,true));GetComponent<IonFrameMetrics>().Write();Debug.Log("ION_CLARITY_"+(report.passed?"PASS":"FAIL"));Application.Quit(report.passed?0:1);}
    }
}
