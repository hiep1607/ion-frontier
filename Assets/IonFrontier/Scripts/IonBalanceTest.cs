using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace IonFrontier
{
    public sealed class IonBalanceTest : MonoBehaviour
    {
        [Serializable] sealed class Trial {public int hull;public bool suppliedQ,eliteKilled,reachedBoss;public string outcome,lastDamageSource;public float seconds,distance,health,damageTaken;public int ammo,ammoSpent,choices;public IonGame.DamageEntry[] damage;}
        [Serializable] sealed class Report {public string scope="Six unprotected stage4 bot trials, three hulls with/without a supplied Q; not a manual balance verdict";public Trial[] trials;public string[] errors;}
        readonly List<Trial> trials=new List<Trial>();readonly List<string> errors=new List<string>();IonGame game;bool piloting,useQ;string folder;
        void OnEnable(){Application.logMessageReceived+=Log;}void OnDisable(){Application.logMessageReceived-=Log;}
        void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)errors.Add(message);}
        void Update(){if(!piloting||game==null||game.State!=FlightState.Playing)return;game.TestTarget=game.TestBalanceAim();if(useQ&&game.CounterActive&&game.EscapeDistance>=2520&&game.UltimateCharges>0)game.UseUltimate();}
        IEnumerator Start()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-wait-focus")>=0)while(!Application.isFocused)yield return null;
            game=GetComponent<IonGame>();var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-ion-captures");folder=at>=0?args[at+1]:Application.persistentDataPath;Directory.CreateDirectory(folder);yield return new WaitForSecondsRealtime(.3f);
            for(int hull=0;hull<3;hull++)for(int q=0;q<2;q++)
            {
                game.ReturnToMenu();game.TestSetProgress(13);game.SelectStage(4);game.SelectHull(hull);game.TestDraftSeed=11102;game.BeginRun();game.TestAutoDraft=true;game.TestInput=true;game.TestFire=true;useQ=q==1;if(useQ)game.TestGivePickup(PickupKind.Ultimate);piloting=true;
                float start=Time.realtimeSinceStartup;while(game.State==FlightState.Playing&&!game.BossActive&&Time.realtimeSinceStartup-start<100)yield return null;piloting=false;game.TestFire=false;
                var trial=new Trial{hull=hull,suppliedQ=useQ,eliteKilled=game.CounterSucceeded,reachedBoss=game.BossActive,outcome=game.State.ToString(),lastDamageSource=game.LastDamageSource,seconds=Time.realtimeSinceStartup-start,distance=game.EscapeDistance,health=game.Health,damageTaken=game.DamageTaken,ammo=game.Ammo,ammoSpent=game.AmmoSpent,choices=game.DraftChoicesMade,damage=game.DamageBreakdown()};trials.Add(trial);File.WriteAllText(Path.Combine(folder,"balance-report.json"),JsonUtility.ToJson(new Report{trials=trials.ToArray(),errors=errors.ToArray()},true));Debug.Log("ION_BALANCE_TRIAL "+JsonUtility.ToJson(trial));
                yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,"hull-"+hull+"-q-"+q+".png"));yield return new WaitForSecondsRealtime(.2f);
            }
            game.ReturnToMenu();GetComponent<IonFrameMetrics>().Write();Application.Quit(errors.Count==0?0:1);
        }
    }
}
