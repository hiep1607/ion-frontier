using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace IonFrontier
{
    // Opt-in player integration tests. Runs against disposable profile and best-score paths.
    public sealed class IonMenuTest : MonoBehaviour
    {
        [Serializable] sealed class Report {public bool passed;public string[] checks,errors;public string assemblySha256,realProfileBefore,realProfileAfter;public string unityVersion,captureMethod;}
        readonly List<string> checks=new List<string>(),errors=new List<string>();IonGame game;IonHud hud;string folder,realProfile,realBefore;
        void OnEnable(){Application.logMessageReceived+=Log;}
        void OnDisable(){Application.logMessageReceived-=Log;}
        void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
        static string Hash(string path){if(!File.Exists(path))return "absent";using(var hash=SHA256.Create())using(var stream=File.OpenRead(path))return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","").ToLowerInvariant();}
        IEnumerator Start()
        {
            game=GetComponent<IonGame>();hud=FindAnyObjectByType<IonHud>();game.TestInput=true;game.TestFire=false;
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-ion-captures");folder=at>=0&&at+1<args.Length?args[at+1]:Application.temporaryCachePath;Directory.CreateDirectory(folder);
            realProfile=Path.Combine(Application.persistentDataPath,"pilot-profile-v2.json");realBefore=Hash(realProfile);
            yield return Wait();
            Check(game.MenuTestUsesTemporaryProfile,"Menu QA uses a temporary profile, with no real campaign save access");
            Check(hud.SupportsVietnamese(),"Interface font contains Vietnamese characters and uppercase accents");
            Check(hud.CurrentMenuPage==IonMenuPage.Home&&game.State==FlightState.Menu,"Startup opens the cinematic home page");
            Check(Find("Menu.Home.Launch").gameObject.activeInHierarchy&&!Find("Menu.Ships.Use").gameObject.activeInHierarchy,"Home contains the launch action without the eight-ship selection grid");
            yield return Capture("startup-home-locked.png");
            Check(PointerClick("Menu.Home.Ships"),"A pointer raycast reaches the visible Ships button");yield return Wait();
            Check(hud.CurrentMenuPage==IonMenuPage.Ships,"A real Unity pointer-click event opens Ships");
            hud.OpenMenuPage(IonMenuPage.Home);yield return Wait();
            EventSystem.current.SetSelectedGameObject(Find("Menu.Home.Guide").gameObject);ExecuteEvents.Execute(Find("Menu.Home.Guide").gameObject,new BaseEventData(EventSystem.current),ExecuteEvents.submitHandler);yield return Wait();
            Check(hud.CurrentMenuPage==IonMenuPage.Guide,"Unity's keyboard submit event opens the focused Guide action exactly once");
            hud.OpenMenuPage(IonMenuPage.Home);yield return Wait();
            Click("Menu.Home.Ships");yield return Wait();
            Click("Menu.Ships.Preview.7");yield return Wait();
            Check(!Find("Menu.Ships.Use").IsInteractable(),"Locked ship can be inspected but cannot be equipped");
            Find("Menu.Ships.Use").onClick.Invoke();Check(game.SelectedHull==0&&hud.CurrentMenuPage==IonMenuPage.Ships,"Even a direct callback cannot equip an unearned ship");
            Click("Menu.Nav.Campaign");yield return Wait();Click("Menu.Campaign.Preview.13");yield return Wait();
            Check(!Find("Menu.Campaign.Use").IsInteractable(),"Locked mission preview explains progression without allowing launch");
            Find("Menu.Campaign.Use").onClick.Invoke();Check(game.Sector==1,"A locked mission callback does not change the selected stage");
            game.TestSetProgress(13);yield return Wait();
            hud.OpenMenuPage(IonMenuPage.Ships);yield return Wait();Click("Menu.Ships.Preview.2");
            Check(EventSystem.current.currentSelectedGameObject==Find("Menu.Ships.Use").gameObject,"Previewing an unlocked ship focuses Use so Enter confirms the choice");
            ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject,new BaseEventData(EventSystem.current),ExecuteEvents.submitHandler);yield return Wait();
            Check(game.SelectedHull==2&&hud.CurrentMenuPage==IonMenuPage.Home,"Keyboard submit equips the previewed ship and returns home once");
            for(int i=0;i<8;i++)
            {
                hud.OpenMenuPage(IonMenuPage.Ships);yield return Wait();Click("Menu.Ships.Preview."+i);Click("Menu.Ships.Use");yield return Wait();
                Check(game.SelectedHull==i&&hud.CurrentMenuPage==IonMenuPage.Home,"Ship "+(i+1)+" equips through real UI callbacks and returns home");
                Check(game.MenuUltimateName(i)!=""&&game.MenuUltimateDescription(i)!="","Ship "+(i+1)+" has its actual ultimate name and description");
            }
            for(int stage=1;stage<=13;stage++)
            {
                hud.OpenMenuPage(IonMenuPage.Campaign);yield return Wait();Click("Menu.Campaign.Preview."+stage);Click("Menu.Campaign.Use");yield return Wait();
                Check(game.Sector==stage&&hud.CurrentMenuPage==IonMenuPage.Home,"Mission "+stage+" selects through UI and returns home");
                Check(hud.MenuPreviewMatches(stage),"Mission "+stage+" preview binds its exact background and live briefing");
            }
            game.SelectHull(0);game.SelectStage(4);hud.OpenMenuPage(IonMenuPage.Home);yield return Wait();
            Click("Menu.Options");yield return Wait();
            Check(hud.MenuModalOpen&&game.State==FlightState.Menu,"Options opens as a real modal while the simulation stays in the menu");
            Check(!PointerClick("Menu.Home.Launch"),"The modal raycast shield blocks pointer events from reaching Launch");
            var focused=EventSystem.current.currentSelectedGameObject;Check(focused!=null&&focused.transform.IsChildOf(Find("Menu.Modal.Close").transform.parent),"Opening a modal moves keyboard focus into its action group");
            bool modalFocusSafe=true;for(int i=0;i<8;i++){hud.CycleMenuFocus(1);var current=EventSystem.current.currentSelectedGameObject;modalFocusSafe&=current!=null&&current.transform.IsChildOf(Find("Menu.Modal.Close").transform.parent);}
            Check(modalFocusSafe,"Tab focus cycles inside the Options modal without reaching underlying menu buttons");
            Find("Menu.Home.Launch").onClick.Invoke();Check(game.State==FlightState.Menu,"An underlying launch callback cannot start combat through Options");
            bool muted=game.Muted;Click("Menu.Options.Audio");Check(game.Muted!=muted,"Options uses the game's actual audio toggle");Click("Menu.Options.Audio");
            Click("Menu.Options.Exit");yield return Wait();Check(hud.MenuModalOpen&&Find("Menu.Exit.Confirm").IsActive(),"Exit requires a separate confirmation");Click("Menu.Modal.Close");yield return Wait();Check(!hud.MenuModalOpen,"Canceling exit keeps the game open");
            hud.SetMenuReducedMotion(true,false);var backdrop=FindAnyObjectByType<IonBackdrop>();float clock=backdrop.MotionClock;yield return new WaitForSecondsRealtime(.3f);
            Check(backdrop.MotionClock==clock,"Reduced motion stops the actual menu background clock");hud.SetMenuReducedMotion(false,false);yield return new WaitForSecondsRealtime(.25f);Check(backdrop.MotionClock>clock,"Restoring motion resumes the menu background");
            hud.OpenMenuPage(IonMenuPage.Guide);yield return Wait();
            for(int i=0;i<5;i++){Click("Menu.Guide.Topic."+i);yield return null;Check(hud.MenuGuideContentUsable(i),"Guide topic "+(i+1)+" has content with a fitted scroll region");}
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(1024,768),new Vector2Int(1680,720)})
            {
                Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.6f);
                foreach(IonMenuPage page in Enum.GetValues(typeof(IonMenuPage)))
                {
                    hud.OpenMenuPage(page);yield return Wait();Canvas.ForceUpdateCanvases();
                    Check(hud.MenuVisibleControlsFit(),page+" controls stay inside the window at "+Screen.width+"x"+Screen.height);
                    yield return Capture(page.ToString().ToLowerInvariant()+"-"+size.x+"x"+size.y+".png");
                }
            }
            Screen.SetResolution(1280,720,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);
            hud.OpenMenuPage(IonMenuPage.Guide);yield return Wait();Click("Menu.Guide.Topic.4");Click("Menu.Guide.Tutorial");yield return Wait();
            Check(game.State==FlightState.Playing&&game.Sector==1&&game.TutorialActive,"Replay tutorial uses the existing four-step flight tutorial");
            game.ReturnToMenu();game.SelectStage(11);hud.OpenMenuPage(IonMenuPage.Home);yield return Wait();Click("Menu.Home.Launch");yield return Wait();
            Check(game.State==FlightState.Playing&&game.RoutePending,"Launching mission 11 retains the route-choice flow");Check(game.ChooseRoute(2),"The rescue route is still selectable after launching from the new menu");
            game.TogglePause();Check(game.State==FlightState.Paused,"Combat pause still works after the menu input split");game.ReturnToMenu();yield return Wait();
            Check(hud.CurrentMenuPage==IonMenuPage.Home,"Returning from combat restores the cinematic home page");
            hud.OpenMenuPage(IonMenuPage.Home);yield return Wait();Click("Menu.Home.Launch");yield return Wait();
            Check(game.State==FlightState.Playing&&game.Sector==11,"Repeated launch uses the chosen mission without a stale menu state");
            game.ReturnToMenu();game.SelectStage(1);game.SelectHull(0);hud.OpenMenuPage(IonMenuPage.Home);yield return Wait();yield return Capture("final-home.png");
            Check(Hash(realProfile)==realBefore,"Real campaign profile is byte-for-byte unchanged by all menu QA");
            var report=new Report{passed=errors.Count==0,checks=checks.ToArray(),errors=errors.ToArray(),assemblySha256=Hash(typeof(IonGame).Assembly.Location),realProfileBefore=realBefore,realProfileAfter=Hash(realProfile),unityVersion=Application.unityVersion,captureMethod="Unity Camera.Render + ScreenSpaceCamera canvas to RenderTexture; temporary capture-only canvas mode, restored afterward. No OS mouse/keyboard input is claimed."};
            File.WriteAllText(Path.Combine(folder,"menu-report.json"),JsonUtility.ToJson(report,true));Debug.Log("ION_MENU_TEST_"+(report.passed?"PASS":"FAIL"));Application.Quit(report.passed?0:1);
        }
        Button Find(string name){var button=hud.FindMenuButton(name);if(button==null)throw new Exception("Missing UI button: "+name);return button;}
        void Click(string name){var button=Find(name);if(!button.gameObject.activeInHierarchy||!button.IsInteractable())throw new Exception("Unavailable UI button: "+name);button.onClick.Invoke();}
        IEnumerator Wait(){yield return new WaitForSecondsRealtime(.24f);}
        bool PointerClick(string name)
        {
            var button=Find(name);var r=(RectTransform)button.transform;var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center)),button=PointerEventData.InputButton.Left};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);if(hits.Count==0)return false;
            var target=ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);if(target!=button.gameObject)return false;
            ExecuteEvents.Execute(target,pointer,ExecuteEvents.pointerClickHandler);return true;
        }
        IEnumerator Capture(string name)
        {
            yield return null;yield return new WaitForEndOfFrame();
            var camera=Camera.main;var canvas=hud.GetComponent<Canvas>();var previousMode=canvas.renderMode;var previousCamera=canvas.worldCamera;var previousDistance=canvas.planeDistance;var previousTarget=camera.targetTexture;var previousActive=RenderTexture.active;
            var image=new RenderTexture(Screen.width,Screen.height,24,RenderTextureFormat.ARGB32);var pixels=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=image;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
                Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=image;pixels.ReadPixels(new Rect(0,0,image.width,image.height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,name),pixels.EncodeToPNG());
                bool hasImage=false;foreach(var c in pixels.GetPixels32()){if(c.r>15||c.g>15||c.b>15){hasImage=true;break;}}
                Check(hasImage,"Rendered UI capture "+name+" contains visible content");
            }
            finally
            {
                camera.targetTexture=previousTarget;canvas.renderMode=previousMode;canvas.worldCamera=previousCamera;canvas.planeDistance=previousDistance;RenderTexture.active=previousActive;Canvas.ForceUpdateCanvases();image.Release();Destroy(image);Destroy(pixels);
            }
            yield return new WaitForSecondsRealtime(.12f);
        }
        void Check(bool success,string message){checks.Add((success?"PASS: ":"FAIL: ")+message);if(!success){errors.Add(message);Debug.LogWarning("MENU_FAIL: "+message);}}
    }
    public sealed partial class IonGame
    {
        internal bool MenuTestUsesTemporaryProfile=>profilePath.StartsWith(Application.temporaryCachePath,StringComparison.OrdinalIgnoreCase)&&bestPath!=Path.Combine(Application.persistentDataPath,"best-score.txt");
    }
    public sealed partial class IonHud
    {
        internal bool MenuPreviewMatches(int stage)=>previewStage==stage&&menuMissionPicture.sprite==IonArt.StageBackdrops[stage-1]&&menuMissionBrief.text==IonCampaign.Briefings[stage-1];
        internal bool MenuGuideContentUsable(int topic)=>menuGuideSections[topic].activeSelf&&menuGuideBodies[topic].text.Length>80&&menuGuideBodies[topic].rectTransform.sizeDelta.y>=menuGuideBodies[topic].preferredHeight;
        internal bool MenuVisibleControlsFit()
        {
            var canvas=GetComponent<Canvas>();Vector3[] corners=new Vector3[4];
            foreach(var button in menu.GetComponentsInChildren<Button>())
            {
                if(!button.gameObject.activeInHierarchy)continue;((RectTransform)button.transform).GetWorldCorners(corners);
                foreach(var corner in corners){Vector2 point=RectTransformUtility.WorldToScreenPoint(canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,corner);if(point.x<-.5f||point.y<-.5f||point.x>Screen.width+.5f||point.y>Screen.height+.5f)return false;}
            }
            return true;
        }
    }
}
