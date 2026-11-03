using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

namespace IonFrontier
{
    public sealed class IonResultTest : MonoBehaviour
    {
        [Serializable] sealed class Report {public bool passed;public string[] checks,errors;public string assemblySha256,profileBefore,profileAfter,captureMethod;}
        readonly List<string> checks=new List<string>(),errors=new List<string>();IonGame game;IonHud hud;string folder,realProfile,before;
        void OnEnable(){Application.logMessageReceived+=Log;}void OnDisable(){Application.logMessageReceived-=Log;}
        void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
        static string Hash(string file){if(!File.Exists(file))return "absent";using(var sha=SHA256.Create())using(var stream=File.OpenRead(file))return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();}
        void Setup(int cleared,int stage,int hull=0)
        {
            game.ReturnToMenu();game.TestSetProgress(cleared);game.SelectStage(stage);game.SelectHull(hull);game.BeginRun();game.TestInput=true;game.TestFire=false;game.TestFreezeEnemies=true;game.TestProtection(10000);game.TestAutoDraft=false;
        }
        IEnumerator Start()
        {
            game=GetComponent<IonGame>();hud=FindAnyObjectByType<IonHud>();var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-ion-captures");folder=at>=0?args[at+1]:Application.temporaryCachePath;Directory.CreateDirectory(folder);
            realProfile=Path.Combine(Application.persistentDataPath,"pilot-profile-v2.json");before=Hash(realProfile);yield return new WaitForSecondsRealtime(.3f);
            Check(game.MenuTestUsesTemporaryProfile,"Result QA uses a disposable profile and best-score path");
            Setup(0,1);game.TestRatingBeginBoss();yield return new WaitForSecondsRealtime(.15f);game.TestDamageBoss(10000);yield return new WaitForSecondsRealtime(.2f);
            Check(game.State==FlightState.BossDying&&game.LastNewHull==-1,"A new hull is not awarded before the actual boss wreck finishes");
            float deadline=Time.realtimeSinceStartup+8;while(game.State!=FlightState.StageClear&&Time.realtimeSinceStartup<deadline)yield return null;
            Check(game.State==FlightState.StageClear&&game.LastCompletedHull==0&&game.LastNewHull==1,"First mission records the flown hull separately from the newly awarded Loi Dieu");
            Check(hud.ResultShipMatchesFlight,"The result portrait uses the exact sprite that was flown");
            Check(game.TestResultSaved.clearedStages==1&&game.TestResultSaved.stars[0]==game.LastStars,"Progress and stars are saved before the reward animation starts");
            Check(hud.ResultVisibleStars==0,"The star sequence starts with zero earned stars lit");yield return Capture("01-debrief-start.png");
            yield return new WaitForSecondsRealtime(.55f);int first=hud.ResultVisibleStars;Check(first>0&&first<5,"Stars appear over time rather than all at once");yield return Capture("02-stars-building.png");
            float elapsed=hud.ResultAnimationTime;game.ToggleAudio();game.ToggleAudio();Check(hud.ResultAnimationTime>=elapsed,"Refreshing the HUD for audio does not restart the star sequence");
            deadline=Time.realtimeSinceStartup+5;while(!hud.NewHullRevealOpen&&Time.realtimeSinceStartup<deadline)yield return null;
            Check(hud.NewHullRevealOpen&&hud.RewardHullSpriteMatches,"A dedicated new-hull reveal follows the star result using the reward's exact sprite");
            Check(hud.ResultVisibleStars==game.LastStars&&hud.ResultRowsVisible==5,"The completed star count equals the real five-criterion rating");
            int actual=game.SelectedHull;Find("Result.Reward.Use").onClick.Invoke();Check(hud.NewHullRevealOpen&&game.SelectedHull==actual,"The reveal's entry guard prevents a skip click from immediately equipping or closing it");
            Find("Result.Next").onClick.Invoke();Find("Result.Replay").onClick.Invoke();Find("Result.Home").onClick.Invoke();Check(game.State==FlightState.StageClear,"Underlying result callbacks cannot leave through the reward modal");
            yield return new WaitForSecondsRealtime(.95f);yield return Capture("03-new-loi-dieu.png");
            Click("Result.Reward.Keep");Check(!hud.NewHullRevealOpen&&hud.ResultShipMatchesFlight,"Closing the reward returns to the result with the original flown ship");
            hud.TestCompleteResultAnimation();Check(!hud.NewHullRevealOpen,"Finishing or skipping the same result again does not replay the unlock reveal");
            Click("Result.Next");Check(game.State==FlightState.Menu&&game.Sector==2&&game.SelectedHull==0,"Keeping the current ship also keeps it selected for the next mission");
            Setup(0,1);game.TestResultFinish();hud.TestCompleteResultAnimation();yield return new WaitForSecondsRealtime(.95f);Click("Result.Reward.Use");
            Check(game.SelectedHull==0&&hud.ResultShipMatchesFlight,"Choosing the new ship does not replace the flown ship on the result screen");Click("Result.Next");Check(game.SelectedHull==1&&game.Sector==2,"The explicit new-ship choice applies when entering the next mission's hangar");game.BeginRun();Check(game.RunHull==1,"The next actual run uses the explicitly chosen new ship");
            Setup(1,1);game.TestResultFinish();hud.TestCompleteResultAnimation();Check(game.LastNewHull==-1&&!hud.NewHullRevealOpen,"Replaying an already completed unlock mission awards no duplicate new ship");
            for(int hull=0;hull<8;hull++)
            {
                Setup(13,2,hull);game.TestResultFinish();hud.TestCompleteResultAnimation();
                Check(game.LastCompletedHull==hull&&hud.ResultShipMatchesFlight,"Hull "+(hull+1)+" matches its combat sprite on victory");
            }
            foreach(int stage in new[]{1,2,5,7,9,11,13})
            {
                Setup(stage-1,stage);game.TestResultFinish();hud.TestCompleteResultAnimation();
                Check(game.LastNewHull==IonCampaign.RewardHull(stage)&&hud.NewHullRevealOpen&&hud.RewardHullSpriteMatches,"First clear of milestone "+stage+" reveals its correct newly unlocked hull");
                Check(game.TestResultSaved.clearedStages==stage,"Milestone "+stage+" is saved before its reveal is acknowledged");
            }
            foreach(int stage in new[]{3,4,6,8,10,12})
            {
                Setup(stage-1,stage);game.TestResultFinish();hud.TestCompleteResultAnimation();Check(game.LastNewHull==-1&&!hud.NewHullRevealOpen,"Non-unlock stage "+stage+" has no false reward reveal");
            }
            foreach(int stars in new[]{1,4,5})
            {
                Setup(13,2);game.TestResultPerformance(stars);game.TestResultFinish();hud.TestCompleteResultAnimation();
                Check(game.LastStars==stars&&hud.ResultVisibleStars==stars&&hud.ResultAllStarsSettled,"A "+stars+"-star result shows exactly that many settled gold stars after Skip");
                yield return Capture("rating-"+stars+"-stars.png");
            }
            Setup(0,1);game.TestRemoveInvulnerability();game.HurtPlayer(10000);yield return null;
            Check(game.State==FlightState.Defeat&&game.LastNewHull==-1&&!hud.NewHullRevealOpen&&hud.ResultShipMatchesFlight,"Defeat shows the flown ship with no reward or victory-star celebration");yield return Capture("04-defeat.png");
            Setup(1,2);game.TestResultFinish();hud.TestCompleteResultAnimation();yield return new WaitForSecondsRealtime(.95f);hud.SetMenuReducedMotion(true,false);yield return null;
            Check(hud.ResultAllStarsSettled&&hud.RewardRevealScaleIsOne,"Reduced motion removes star bounce and ship zoom without hiding the earned reward");hud.SetMenuReducedMotion(false,false);
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1024,768),new Vector2Int(1680,720)})
            {
                Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.45f);
                Setup(13,3,4);game.TestResultFinish();hud.TestCompleteResultAnimation();yield return null;
                Check(hud.ResultControlsFit,"Result controls fit "+Screen.width+"x"+Screen.height);yield return Capture("result-"+size.x+"x"+size.y+".png");
                Setup(1,2);game.TestResultFinish();hud.TestCompleteResultAnimation();yield return new WaitForSecondsRealtime(.95f);
                Check(hud.ResultControlsFit,"New-ship controls fit "+Screen.width+"x"+Screen.height);yield return Capture("unlock-"+size.x+"x"+size.y+".png");
            }
            Check(Hash(realProfile)==before,"The real player campaign file remains byte-for-byte unchanged");
            var report=new Report{passed=errors.Count==0,checks=checks.ToArray(),errors=errors.ToArray(),assemblySha256=Hash(typeof(IonGame).Assembly.Location),profileBefore=before,profileAfter=Hash(realProfile),captureMethod="Native Unity RenderTexture capture; gameplay boss-death path plus explicit isolated reward/rating fixtures. UI callbacks are exercised, not OS input."};
            File.WriteAllText(Path.Combine(folder,"result-report.json"),JsonUtility.ToJson(report,true));Debug.Log("ION_RESULT_TEST_"+(report.passed?"PASS":"FAIL"));Application.Quit(report.passed?0:1);
        }
        Button Find(string name){foreach(var b in hud.GetComponentsInChildren<Button>(true))if(b.name==name)return b;throw new Exception("Missing result button: "+name);}
        void Click(string name){var b=Find(name);if(!b.gameObject.activeInHierarchy||!b.IsInteractable())throw new Exception("Unavailable result button: "+name);b.onClick.Invoke();}
        IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();var camera=Camera.main;var canvas=hud.GetComponent<Canvas>();var mode=canvas.renderMode;var previousCamera=canvas.worldCamera;var distance=canvas.planeDistance;var sorting=canvas.sortingOrder;var overriding=canvas.overrideSorting;var target=camera.targetTexture;var active=RenderTexture.active;
            var rt=new RenderTexture(Screen.width,Screen.height,24);var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
            try{camera.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;canvas.overrideSorting=true;canvas.sortingOrder=32760;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name),image.EncodeToPNG());}
            finally{camera.targetTexture=target;canvas.renderMode=mode;canvas.worldCamera=previousCamera;canvas.planeDistance=distance;canvas.sortingOrder=sorting;canvas.overrideSorting=overriding;RenderTexture.active=active;Canvas.ForceUpdateCanvases();rt.Release();Destroy(rt);Destroy(image);}yield return null;
        }
        void Check(bool success,string message){checks.Add((success?"PASS: ":"FAIL: ")+message);if(!success){errors.Add(message);Debug.LogWarning("RESULT_FAIL: "+message);}}
    }
    public sealed partial class IonGame
    {
        internal PilotProfile TestResultSaved=>IonCampaign.Load(profilePath);
        internal void TestResultFinish(){Finish(true);}
        internal void TestResultPerformance(int stars){ratingBossSeen=true;BossCombatSeconds=stars==1?BossParSeconds+1:10;playerHits=stars==1?3:0;Health=stars==1?MaxHealth*.2f:MaxHealth;DamageTaken=stars==1?MaxHealth*.8f:stars==4?MaxHealth*.6f:0;}
        internal Sprite TestActualFlownSprite=>player.sprite;
    }
    public sealed partial class IonHud
    {
        internal bool ResultShipMatchesFlight=>unlockImage.sprite==game.TestActualFlownSprite&&unlockImage.sprite==IonArt.PlayerShips[game.LastCompletedHull];
        internal bool RewardHullSpriteMatches=>game.LastNewHull>=0&&rewardShip.sprite==IonArt.PlayerShips[game.LastNewHull];
        internal int ResultVisibleStars=>resultGoldStars;
        internal int ResultRowsVisible=>resultRowsShown;
        internal float ResultAnimationTime=>resultClock;
        internal bool ResultAllStarsSettled{get{foreach(var star in ratingStars)if((star.rectTransform.localScale-Vector3.one).sqrMagnitude>.0001f)return false;return true;}}
        internal bool RewardRevealScaleIsOne=>(rewardShipRect.localScale-Vector3.one).sqrMagnitude<.0001f;
        internal void TestCompleteResultAnimation(){CompleteResultSequence();}
        internal bool ResultControlsFit
        {
            get{var corners=new Vector3[4];foreach(var button in result.GetComponentsInChildren<Button>()){((RectTransform)button.transform).GetWorldCorners(corners);foreach(var c in corners){var p=RectTransformUtility.WorldToScreenPoint(null,c);if(p.x<-.5f||p.y<-.5f||p.x>Screen.width+.5f||p.y>Screen.height+.5f)return false;}}return true;}
        }
    }
}
