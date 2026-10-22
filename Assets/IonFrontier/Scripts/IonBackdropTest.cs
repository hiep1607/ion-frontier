using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace IonFrontier
{
    // Opt-in rendered gallery and integration checks; isolated from campaign saves.
    public sealed class IonBackdropTest : MonoBehaviour
    {
        [Serializable] public sealed class Report {public bool passed;public string[] checks,errors;public float averageFps;public int maxLayers,maxProjectiles;}
        readonly List<string> checks=new List<string>(),errors=new List<string>();
        IonGame game;IonBackdrop backdrop;IonHud hud;string destination;float elapsed;int frames,maxLayers,maxProjectiles;
        void OnEnable(){Application.logMessageReceived+=Log;}
        void OnDisable(){Application.logMessageReceived-=Log;}
        void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
        void Update(){if(Time.unscaledDeltaTime>0){frames++;elapsed+=Time.unscaledDeltaTime;}if(backdrop!=null)maxLayers=Mathf.Max(maxLayers,backdrop.ActiveLayers);if(game!=null)maxProjectiles=Mathf.Max(maxProjectiles,game.ActiveBullets);}
        IEnumerator Start()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-wait-focus")>=0)while(!Application.isFocused)yield return null;
            game=GetComponent<IonGame>();game.TestAutoDraft=true;backdrop=FindFirstObjectByType<IonBackdrop>();hud=FindFirstObjectByType<IonHud>();game.TestInput=true;game.TestFire=false;
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-ion-captures");destination=at>=0?args[at+1]:Application.persistentDataPath;Directory.CreateDirectory(destination);
            yield return new WaitForSecondsRealtime(.5f);game.TestSetProgress(13);var unique=new HashSet<Sprite>();
            int pool=backdrop.GetComponentsInChildren<SpriteRenderer>(true).Length;
            for(int stage=1;stage<=13;stage++)
            {
                game.ReturnToMenu();Check(game.SelectStage(stage),"Stage "+stage+" remains selectable from the normal hangar");yield return null;
                game.BeginRun();game.TestProtection(10000);game.TestFreezeEnemies=true;game.TestBackdropQuiet();yield return new WaitForSecondsRealtime(.3f);
                Check(backdrop.BackgroundStage==stage&&backdrop.BackgroundSprite==IonArt.StageBackdrops[stage-1],"Stage "+stage+" selects its own illustrated environment");
                Check(backdrop.CoversCamera,"Stage "+stage+" covers the full visible camera area");unique.Add(backdrop.BackgroundSprite);
                yield return EnvironmentCapture("environment-"+stage.ToString("D2")+".png");
                if(stage==1||stage==4||stage==7||stage==8||stage==9||stage==12||stage==13)yield return Capture("gameplay-"+stage.ToString("D2")+".png");
                if(stage==8)
                {
                    game.TogglePause();float frozen=backdrop.MotionClock;yield return new WaitForSecondsRealtime(.3f);Check(backdrop.MotionClock==frozen,"Pause freezes stars, fog and scenery motion");game.TogglePause();
                    yield return new WaitForSecondsRealtime(.15f);Check(backdrop.MotionClock>frozen,"Environmental movement resumes after pause");
                }
                if(stage==11)
                {
                    Check(game.ChooseRoute(1),"Supply route remains usable with the forked environment");yield return Capture("route-11-supply.png");game.ReturnToMenu();game.BeginRun();game.TestProtection(10000);game.TestFreezeEnemies=true;game.TestBackdropQuiet();
                    Check(game.ChooseRoute(2),"Rescue route remains usable with the forked environment");yield return Capture("route-11-rescue.png");
                }
                Check(backdrop.GetComponentsInChildren<SpriteRenderer>(true).Length==pool,"Stage "+stage+" reuses the fixed background renderer pool");
            }
            Check(unique.Count==13,"All thirteen stages use distinct background sprites");
            foreach(int stage in new[]{5,8,13})
            {
                game.ReturnToMenu();game.SelectStage(stage);game.BeginRun();game.TestProtection(10000);game.TestStartBossEncounter();game.TestFreezeEnemies=false;yield return new WaitForSecondsRealtime(2.4f);
                Check(game.BossActive&&backdrop.BackgroundStage==stage&&backdrop.CoversCamera,"Stage "+stage+" keeps its environment during real boss combat");yield return Capture("boss-"+stage.ToString("D2")+".png");
            }
            game.TogglePause();float bossFrozen=backdrop.MotionClock;yield return new WaitForSecondsRealtime(.25f);Check(bossFrozen==backdrop.MotionClock,"Boss combat pause also freezes the background");game.TogglePause();
            game.ReturnToMenu();game.SelectStage(7);game.BeginRun();game.TestProtection(10000);game.TestFreezeEnemies=true;game.TestBackdropQuiet();
            foreach(var size in new[]{new Vector2Int(1280,960),new Vector2Int(1920,810),new Vector2Int(1280,720)})
            {
                Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.6f);Check(backdrop.CoversCamera,"Camera coverage survives window resize to "+size.x+"x"+size.y);yield return EnvironmentCapture("aspect-"+size.x+"x"+size.y+".png");
            }
            game.TestSpecialTargets();game.TestTarget=game.PlayerPosition;game.TestSetAmmo(600);for(int i=0;i<10;i++){game.TestGivePickup(PickupKind.FireRate);game.TestGivePickup(PickupKind.Multishot);}game.TestGivePickup(PickupKind.Spread);game.TestGivePickup(PickupKind.Homing);game.TestFire=true;
            yield return new WaitForSecondsRealtime(4);game.TestFire=false;Check(game.AmmoSpent>10&&game.ActiveBullets>0,"High-speed environment remains active under actual upgraded projectile combat");
            Check(maxProjectiles<512&&backdrop.CoversCamera,"Dense combat stays inside projectile capacity while the background covers the camera");yield return Capture("high-speed-combat.png");
            game.ReturnToMenu();game.SelectStage(1);yield return Capture("hangar-environment-preview.png");yield return Gallery();
            Check(maxLayers<=174,"Layer counts stay within the fixed background budget");
            var report=new Report{passed=errors.Count==0,checks=checks.ToArray(),errors=errors.ToArray(),averageFps=elapsed>0?frames/elapsed:0,maxLayers=maxLayers,maxProjectiles=maxProjectiles};
            File.WriteAllText(Path.Combine(destination,"backdrop-report.json"),JsonUtility.ToJson(report,true));Debug.Log("ION_BACKDROP_TEST_"+(report.passed?"PASS":"FAIL"));yield return new WaitForSecondsRealtime(.2f);Application.Quit(report.passed?0:1);
        }
        IEnumerator EnvironmentCapture(string name)
        {
            game.enabled=false;hud.gameObject.SetActive(false);var renderers=FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);var enabled=new bool[renderers.Length];
            for(int i=0;i<renderers.Length;i++){enabled[i]=renderers[i].enabled;if(!renderers[i].transform.IsChildOf(backdrop.transform))renderers[i].enabled=false;}
            yield return Capture(name);
            for(int i=0;i<renderers.Length;i++)if(renderers[i]!=null)renderers[i].enabled=enabled[i];hud.gameObject.SetActive(true);game.enabled=true;
        }
        IEnumerator Capture(string name){yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(destination,name));yield return new WaitForSecondsRealtime(.12f);}
        IEnumerator Gallery()
        {
            var overlay=new GameObject("Sector art gallery");var canvas=overlay.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=200;
            var scaler=overlay.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);
            var bg=new GameObject("Gallery",typeof(RectTransform),typeof(Image));bg.transform.SetParent(overlay.transform,false);var r=(RectTransform)bg.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;bg.GetComponent<Image>().color=new Color(.015f,.025f,.045f);
            var font=Resources.Load<Font>("Interface");
            for(int i=0;i<14;i++)
            {
                if(i==13)
                {
                    var title=new GameObject("Title",typeof(RectTransform),typeof(Text));title.transform.SetParent(overlay.transform,false);r=(RectTransform)title.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-40);r.sizeDelta=new Vector2(1500,50);var t=title.GetComponent<Text>();t.font=font;t.fontSize=29;t.alignment=TextAnchor.MiddleCenter;t.color=IonArt.Cyan;t.text="ION FRONTIER / 13 KHU VỰC RIÊNG BIỆT";continue;
                }
                float x=i<10?160+i%5*320:480+(i-10)*320,y=700-i/5*270;
                var card=new GameObject("Sector",typeof(RectTransform),typeof(Image),typeof(RectMask2D));card.transform.SetParent(overlay.transform,false);r=(RectTransform)card.transform;r.anchorMin=r.anchorMax=Vector2.zero;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(296,166);card.GetComponent<Image>().color=Color.black;
                var illustration=new GameObject("Painting",typeof(RectTransform),typeof(Image));illustration.transform.SetParent(card.transform,false);r=(RectTransform)illustration.transform;r.anchorMin=r.anchorMax=Vector2.one*.5f;var sprite=IonArt.StageBackdrops[i];float scale=Mathf.Max(296/sprite.rect.width,166/sprite.rect.height);r.sizeDelta=sprite.rect.size*scale;var im=illustration.GetComponent<Image>();im.sprite=sprite;im.color=new Color(.76f,.76f,.79f);
                var label=new GameObject("Name",typeof(RectTransform),typeof(Text));label.transform.SetParent(overlay.transform,false);r=(RectTransform)label.transform;r.anchorMin=r.anchorMax=Vector2.zero;r.anchoredPosition=new Vector2(x,y-112);r.sizeDelta=new Vector2(300,48);var text=label.GetComponent<Text>();text.font=font;text.fontSize=16;text.alignment=TextAnchor.MiddleCenter;text.color=Color.white;text.text="M"+(i+1)+" / "+IonCampaign.StageNames[i];
            }
            yield return Capture("all-13-sectors.png");Destroy(overlay);
        }
        void Check(bool pass,string message){checks.Add((pass?"PASS: ":"FAIL: ")+message);if(!pass){errors.Add(message);Debug.LogWarning("BACKDROP_FAIL: "+message);}}
    }
    public sealed partial class IonGame
    {
        public void TestBackdropQuiet(){bannerTimer=0;waveLive=true;transition=1000;hud.ShowBanner(false,Sector,SectorName);}
    }
}
