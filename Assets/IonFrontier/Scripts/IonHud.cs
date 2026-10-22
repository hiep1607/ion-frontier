using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace IonFrontier
{
    public sealed partial class IonHud : MonoBehaviour
    {
        IonGame game;Font regular;
        GameObject flight,menu,pause,result,banner,bossBar,tutorial,help,nextButton,replayButton;
        Text boosts,score,best,sector,weapon,dash,ammo,ultimate,mode,statusTitle,statusBody,bannerNumber,bannerName,toast,bossTitle,tutorialTitle,tutorialBody,heroName,heroStatus,menuBrief,missionTitle,missionBody,missionTip;
        Image dashFill,bossFill,damage,blast,heroImage,unlockImage,healthFill,playerPortrait,targetPortrait,targetFill;
        Text healthText,playerName,targetName,targetHealth,targetState;GameObject targetPanel;
        readonly Image[] hullCards=new Image[8];
        readonly Button[] hullButtons=new Button[8],stageButtons=new Button[13];
        readonly Text[] hullStates=new Text[8],stageTexts=new Text[13],pickupLabels=new Text[20],worldLabels=new Text[36];
        GameObject missionPanel,routePanel,escapePanel;Image escapeFill,escapeShip;Text escapeTitle;
        GameObject ratingGuide,ratingBoard;Text ratingSummary;
        readonly IonRatingStar[] ratingStars=new IonRatingStar[5];
        readonly Text[] ratingRows=new Text[5],ratingValues=new Text[5],ratingStates=new Text[5],ratingGuideRows=new Text[5];
        RectTransform hero;float toastTime,damageTime,blastTime,blastStrength;
        Color text=new Color(.88f,.94f,1),muted=new Color(.5f,.65f,.76f);

        public void Initialize(IonGame value)
        {
            game=value;regular=Resources.Load<Font>("Interface")??Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            if(EventSystem.current==null){var es=new GameObject("Flight UI input");es.AddComponent<EventSystem>();es.AddComponent<StandaloneInputModule>();}
            MakeFlight();MakeMissionInterface();MakeTargetInterface();MakeMenu();MakePause();MakeResult();MakeHelp();MakeRatingGuide();MakeDraftInterface();
            banner=Panel("Stage banner",transform,new Color(.02f,.045f,.075f,.85f));
            var r=(RectTransform)banner.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.sizeDelta=new Vector2(690,100);r.anchoredPosition=new Vector2(0,70);
            bannerNumber=Label("",banner.transform,17,IonArt.Cyan,Vector2.one*.5f,new Vector2(0,25),new Vector2(650,28),TextAnchor.MiddleCenter);
            bannerName=Label("",banner.transform,29,text,Vector2.one*.5f,new Vector2(0,-17),new Vector2(670,50),TextAnchor.MiddleCenter);banner.SetActive(false);
            toast=Label("",transform,19,IonArt.Gold,new Vector2(.5f,0),new Vector2(0,155),new Vector2(1450,44),TextAnchor.MiddleCenter);
            damage=Panel("Damage vignette",transform,new Color(1,.1f,.02f,0)).GetComponent<Image>();damage.raycastTarget=false;
            blast=Panel("Reactor flash",transform,new Color(1,.95f,.85f,0)).GetComponent<Image>();blast.raycastTarget=false;
        }
        void MakeFlight()
        {
            flight=Panel("Combat interface",transform,Color.clear);
            var top=Bar(flight.transform,new Vector2(.5f,1),new Vector2(0,-44),new Vector2(1800,88),new Color(.015f,.03f,.06f,.94f));
            Label("ION / FRONTIER",flight.transform,23,text,new Vector2(0,1),new Vector2(150,-32),new Vector2(240,40));
            Label("PHI ĐỘI TIÊN PHONG",flight.transform,13,muted,new Vector2(0,1),new Vector2(150,-61),new Vector2(240,26));
            sector=Label("",flight.transform,18,IonArt.Cyan,new Vector2(.5f,1),new Vector2(0,-44),new Vector2(800,70),TextAnchor.MiddleCenter);
            score=Label("",flight.transform,28,text,new Vector2(1,1),new Vector2(-135,-43),new Vector2(210,42),TextAnchor.MiddleRight);
            var bottom=Bar(flight.transform,new Vector2(.5f,0),new Vector2(0,53),new Vector2(1800,106),new Color(.015f,.03f,.06f,.94f));
            playerPortrait=Bar(flight.transform,Vector2.zero,new Vector2(52,64),new Vector2(48,58),Color.white);playerPortrait.preserveAspect=true;
            playerName=Label("",flight.transform,12,muted,Vector2.zero,new Vector2(161,87),new Vector2(150,22));
            Bar(flight.transform,Vector2.zero,new Vector2(160,61),new Vector2(150,12),new Color(.12f,.2f,.25f));
            healthFill=Bar(flight.transform,Vector2.zero,new Vector2(85,61),new Vector2(150,12),IonArt.Cyan);healthFill.rectTransform.pivot=new Vector2(0,.5f);
            healthText=Label("",flight.transform,14,text,Vector2.zero,new Vector2(160,35),new Vector2(150,25),TextAnchor.MiddleCenter);
            ammo=Label("",flight.transform,25,text,Vector2.zero,new Vector2(345,69),new Vector2(230,40),TextAnchor.MiddleCenter);
            Label("1 ĐẠN / LOẠT BẮN",flight.transform,12,muted,Vector2.zero,new Vector2(345,34),new Vector2(230,25),TextAnchor.MiddleCenter);
            weapon=Label("",flight.transform,17,text,new Vector2(.5f,0),new Vector2(-55,83),new Vector2(400,30),TextAnchor.MiddleCenter);
            boosts=Label("",flight.transform,13,IonArt.Gold,new Vector2(.5f,0),new Vector2(-55,53),new Vector2(400,26),TextAnchor.MiddleCenter);
            mode=Label("",flight.transform,13,muted,new Vector2(.5f,0),new Vector2(-55,25),new Vector2(400,25),TextAnchor.MiddleCenter);
            var ult=Button("",flight.transform,new Vector2(1,0),new Vector2(-365,56),new Vector2(220,68),()=>game.UseUltimate(),false);
            ultimate=Label("",ult.transform,16,IonArt.Gold,Vector2.one*.5f,Vector2.zero,new Vector2(215,60),TextAnchor.MiddleCenter);
            dash=Label("",flight.transform,14,IonArt.Cyan,new Vector2(1,0),new Vector2(-130,75),new Vector2(190,28),TextAnchor.MiddleCenter);
            Bar(flight.transform,new Vector2(1,0),new Vector2(-130,42),new Vector2(165,5),new Color(.12f,.21f,.28f));
            dashFill=Bar(flight.transform,new Vector2(1,0),new Vector2(-212.5f,42),new Vector2(165,5),IonArt.Cyan);dashFill.rectTransform.pivot=new Vector2(0,.5f);
            bossBar=Panel("Boss integrity",flight.transform,Color.clear);
            var br=(RectTransform)bossBar.transform;br.anchorMin=br.anchorMax=new Vector2(.5f,1);br.anchoredPosition=new Vector2(0,-126);br.sizeDelta=new Vector2(520,60);
            bossTitle=Label("",bossBar.transform,16,IonArt.Orange,Vector2.one*.5f,new Vector2(0,18),new Vector2(540,30),TextAnchor.MiddleCenter);
            Bar(bossBar.transform,Vector2.one*.5f,new Vector2(0,-13),new Vector2(520,8),new Color(.25f,.1f,.07f));
            bossFill=Bar(bossBar.transform,Vector2.one*.5f,new Vector2(-260,-13),new Vector2(520,8),IonArt.Orange);bossFill.rectTransform.pivot=new Vector2(0,.5f);
            tutorial=Panel("First flight tutorial",flight.transform,new Color(.015f,.04f,.07f,.94f));
            br=(RectTransform)tutorial.transform;br.anchorMin=br.anchorMax=new Vector2(.5f,1);br.anchoredPosition=new Vector2(0,-235);br.sizeDelta=new Vector2(1050,142);
            tutorialTitle=Label("",tutorial.transform,25,IonArt.Cyan,Vector2.one*.5f,new Vector2(-65,32),new Vector2(850,40),TextAnchor.MiddleCenter);
            tutorialBody=Label("",tutorial.transform,18,text,Vector2.one*.5f,new Vector2(-65,-22),new Vector2(850,64),TextAnchor.MiddleCenter);
            Button("BỎ QUA",tutorial.transform,new Vector2(1,.5f),new Vector2(-75,-8),new Vector2(130,45),game.SkipTutorial,false);
            for(int i=0;i<pickupLabels.Length;i++)
            {
                pickupLabels[i]=Label("",flight.transform,14,text,Vector2.zero,Vector2.zero,new Vector2(220,28),TextAnchor.MiddleCenter);
                var outline=pickupLabels[i].gameObject.AddComponent<Outline>();outline.effectColor=new Color(0,0,0,.9f);outline.effectDistance=new Vector2(1.5f,-1.5f);pickupLabels[i].gameObject.SetActive(false);
            }
        }
        void MakeMissionInterface()
        {
            missionPanel=Panel("Mission orders",flight.transform,new Color(.01f,.03f,.06f,.22f));
            var r=(RectTransform)missionPanel.transform;r.anchorMin=r.anchorMax=new Vector2(0,1);r.anchoredPosition=new Vector2(290,-190);r.sizeDelta=new Vector2(540,150);
            missionTitle=Label("",missionPanel.transform,18,IonArt.Cyan,Vector2.one*.5f,new Vector2(0,48),new Vector2(510,32));
            missionBody=Label("",missionPanel.transform,17,text,Vector2.one*.5f,new Vector2(0,4),new Vector2(510,60));missionBody.horizontalOverflow=HorizontalWrapMode.Wrap;
            missionTip=Label("",missionPanel.transform,14,muted,Vector2.one*.5f,new Vector2(0,-49),new Vector2(510,38));missionTip.horizontalOverflow=HorizontalWrapMode.Wrap;
            escapePanel=Panel("Escape route progress",flight.transform,new Color(.015f,.035f,.06f,.18f));
            r=(RectTransform)escapePanel.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.anchoredPosition=new Vector2(30,-134);r.sizeDelta=new Vector2(540,86);
            escapeTitle=Label("",escapePanel.transform,16,IonArt.Cyan,Vector2.one*.5f,new Vector2(0,22),new Vector2(520,32),TextAnchor.MiddleCenter);
            Bar(escapePanel.transform,Vector2.one*.5f,new Vector2(0,-12),new Vector2(500,12),new Color(.12f,.22f,.28f));
            escapeFill=Bar(escapePanel.transform,Vector2.one*.5f,new Vector2(-250,-12),new Vector2(0,12),IonArt.Cyan);escapeFill.rectTransform.pivot=new Vector2(0,.5f);
            for(int i=1;i<3;i++)Bar(escapePanel.transform,Vector2.one*.5f,new Vector2(-250+500*i/3f,-12),new Vector2(2,20),new Color(.65f,.8f,.9f,.65f));
            escapeShip=Bar(escapePanel.transform,Vector2.one*.5f,new Vector2(-250,-12),new Vector2(23,28),Color.white);escapeShip.preserveAspect=true;
            MakeOverlayTextReadable(missionPanel);MakeOverlayTextReadable(escapePanel);
            for(int i=0;i<worldLabels.Length;i++)
            {
                worldLabels[i]=Label("",flight.transform,14,text,Vector2.zero,Vector2.zero,new Vector2(250,40),TextAnchor.MiddleCenter);
                var outline=worldLabels[i].gameObject.AddComponent<Outline>();outline.effectColor=Color.black;outline.effectDistance=Vector2.one;
            }
            routePanel=Panel("Choose rescue route",flight.transform,new Color(.015f,.035f,.07f,.98f));
            r=(RectTransform)routePanel.transform;r.anchorMin=r.anchorMax=Vector2.one*.5f;r.sizeDelta=new Vector2(1050,380);
            Label("HAI LỜI CẦU CỨU",routePanel.transform,35,text,Vector2.one*.5f,new Vector2(0,120),new Vector2(1000,70),TextAnchor.MiddleCenter);
            Label("Chọn một tuyến. Cả hai đều dẫn tới trận Song Sát.",routePanel.transform,20,muted,Vector2.one*.5f,new Vector2(0,55),new Vector2(1000,40),TextAnchor.MiddleCenter);
            Button("E / CỨU TÀU TIẾP TẾ",routePanel.transform,Vector2.one*.5f,new Vector2(-250,-20),new Vector2(450,65),()=>game.ChooseRoute(1),true);
            Button("R / CỨU PHI ĐỘI",routePanel.transform,Vector2.one*.5f,new Vector2(250,-20),new Vector2(450,65),()=>game.ChooseRoute(2),false);
            Label("Hộ tống → thêm đạn và chỉ số\nCứu phi đội → đồng minh bắn hỗ trợ",routePanel.transform,19,muted,Vector2.one*.5f,new Vector2(0,-100),new Vector2(1000,80),TextAnchor.MiddleCenter);
        }
        void TickMission()
        {
            bool show=game.Sector>=4&&(game.State==FlightState.Playing||game.State==FlightState.Paused)&&!game.BossDeathActive;
            missionPanel.SetActive(show);routePanel.SetActive(show&&game.RoutePending&&game.State==FlightState.Playing);
            escapePanel.SetActive(show&&game.EscapeRunActive);escapeTitle.text="ĐƯỜNG THOÁT / "+game.EscapeProgressText;
            escapeFill.rectTransform.sizeDelta=new Vector2(500*game.EscapeFraction,12);escapeShip.sprite=IonArt.PlayerShips[game.SelectedHull];escapeShip.rectTransform.anchoredPosition=new Vector2(-250+500*game.EscapeFraction,-12);
            missionTitle.text=game.MissionTitle;missionBody.text=game.CounterMissionText!=""?game.CounterMissionText+(game.CounterActive?"\n"+game.CounterEngineText:""):game.MissionObjectiveText;missionTip.text=game.EncounterHudText!=""?game.EncounterHudText:game.MissionTip;
            ((RectTransform)bossBar.transform).anchoredPosition=new Vector2(30,-126);
            for(int i=0;i<worldLabels.Length;i++)
            {
                string value=game.MissionObjectLabel(i,out Vector2 position);Vector3 vp=Camera.main.WorldToViewportPoint(position);
                bool visible=show&&value!=""&&vp.y>.15f&&vp.y<.85f;
                worldLabels[i].gameObject.SetActive(visible);if(!visible)continue;
                worldLabels[i].text=value;var size=((RectTransform)transform).rect.size;worldLabels[i].rectTransform.anchoredPosition=new Vector2(Mathf.Clamp(vp.x*size.x,100,size.x-100),vp.y*size.y);
            }
        }
        void MakeTargetInterface()
        {
            targetPanel=Panel("Attack target details",flight.transform,new Color(.015f,.035f,.06f,.22f));
            var r=(RectTransform)targetPanel.transform;r.anchorMin=r.anchorMax=new Vector2(1,1);r.anchoredPosition=new Vector2(-245,-205);r.sizeDelta=new Vector2(450,180);
            Label("MỤC TIÊU ĐANG BỊ TẤN CÔNG",targetPanel.transform,14,muted,Vector2.one*.5f,new Vector2(0,69),new Vector2(420,24));
            targetPortrait=Bar(targetPanel.transform,Vector2.one*.5f,new Vector2(-165,-2),new Vector2(88,105),Color.white);targetPortrait.preserveAspect=true;
            targetName=Label("",targetPanel.transform,17,IonArt.Cyan,Vector2.one*.5f,new Vector2(58,38),new Vector2(300,32));
            targetHealth=Label("",targetPanel.transform,17,text,Vector2.one*.5f,new Vector2(58,6),new Vector2(300,28));
            Bar(targetPanel.transform,Vector2.one*.5f,new Vector2(52,-20),new Vector2(280,6),new Color(.12f,.2f,.25f));
            targetFill=Bar(targetPanel.transform,Vector2.one*.5f,new Vector2(-88,-20),new Vector2(280,6),IonArt.Orange);targetFill.rectTransform.pivot=new Vector2(0,.5f);
            targetState=Label("",targetPanel.transform,14,muted,Vector2.one*.5f,new Vector2(58,-49),new Vector2(300,38));targetState.horizontalOverflow=HorizontalWrapMode.Wrap;
            MakeOverlayTextReadable(targetPanel);
        }
        void MakeOverlayTextReadable(GameObject panel)
        {
            // Fade only the panel image; keep text, portraits and HP/progress fills fully visible.
            foreach(var label in panel.GetComponentsInChildren<Text>(true))
            {
                if(label.color==muted)label.color=new Color(.74f,.85f,.93f,1);
                var outline=label.gameObject.AddComponent<Outline>();
                outline.effectColor=new Color(0,0,0,.95f);outline.effectDistance=new Vector2(1.1f,-1.1f);
            }
        }
        void TickTarget()
        {
            targetPanel.SetActive(!game.TutorialActive&&!game.BossDeathActive);
            bool has=game.HasAttackTarget;targetPortrait.enabled=has;if(has)targetPortrait.sprite=game.AttackTargetSprite;
            targetName.text=game.AttackTargetName;targetHealth.text=has?"HP "+game.AttackTargetHealth.ToString("0.##")+" / "+game.AttackTargetMaxHealth.ToString("0.##"):"—";
            targetFill.rectTransform.sizeDelta=new Vector2(280*game.AttackTargetFraction,6);targetFill.color=game.AttackTargetIsBoss?IonArt.Orange:IonArt.Cyan;targetState.text=game.AttackTargetState;
        }
        void MakeMenu()
        {
            menu=Panel("Hangar",transform,new Color(.01f,.02f,.04f,.18f));
            Label("I F / CĂN CỨ QUỸ ĐẠO",menu.transform,17,IonArt.Cyan,new Vector2(0,1),new Vector2(330,-62),new Vector2(450,35));
            Label("ION FRONTIER",menu.transform,60,text,new Vector2(0,.5f),new Vector2(450,310),new Vector2(690,90));
            Label("CHỌN CHIẾN HẠM / PHÍM 1–8",menu.transform,18,muted,new Vector2(0,.5f),new Vector2(450,238),new Vector2(690,35));
            for(int i=0;i<hullCards.Length;i++)
            {
                int choice=i;var card=Bar(menu.transform,new Vector2(0,.5f),new Vector2(190+i%4*175,135-i/4*160),new Vector2(162,145),new Color(.035f,.075f,.11f));
                hullCards[i]=card;card.raycastTarget=true;var button=card.gameObject.AddComponent<Button>();hullButtons[i]=button;button.onClick.AddListener(()=>game.SelectHull(choice));button.navigation=new Navigation{mode=Navigation.Mode.None};
                var image=Bar(card.transform,Vector2.one*.5f,new Vector2(0,18),new Vector2(95,95),Color.white);image.sprite=IonArt.PlayerShips[i];image.preserveAspect=true;
                Label(IonCampaign.HullNames[i],card.transform,16,IonCampaign.HullColors[i],Vector2.one*.5f,new Vector2(0,-38),new Vector2(155,25),TextAnchor.MiddleCenter);
                hullStates[i]=Label("",card.transform,11,muted,Vector2.one*.5f,new Vector2(0,-59),new Vector2(155,20),TextAnchor.MiddleCenter);
            }
            Label("CHỌN MÀN / SAO THÀNH TÍCH",menu.transform,15,muted,new Vector2(0,.5f),new Vector2(450,-139),new Vector2(690,30));
            for(int i=0;i<stageButtons.Length;i++)
            {
                int stage=i+1;var go=Button("",menu.transform,new Vector2(0,.5f),new Vector2(145+i%7*100,-190-i/7*55),new Vector2(94,48),()=>game.SelectStage(stage),false);
                stageButtons[i]=go.GetComponent<Button>();stageTexts[i]=Label("",go.transform,13,text,Vector2.one*.5f,Vector2.zero,new Vector2(92,44),TextAnchor.MiddleCenter);
            }
            Button("XUẤT KÍCH >",menu.transform,new Vector2(0,.5f),new Vector2(280,-330),new Vector2(330,65),game.BeginRun,true);
            Button("TIÊU CHÍ 5 SAO",menu.transform,new Vector2(0,.5f),new Vector2(800,-330),new Vector2(180,65),ShowRatingGuide,false);
            Button("VẬT PHẨM",menu.transform,new Vector2(0,.5f),new Vector2(570,-330),new Vector2(215,65),ShowHelp,false);
            best=Label("",menu.transform,15,muted,new Vector2(0,.5f),new Vector2(285,-385),new Vector2(340,30));
            Button("HƯỚNG DẪN LẠI",menu.transform,new Vector2(0,.5f),new Vector2(590,-385),new Vector2(210,35),game.ReplayTutorial,false);
            var halo=Bar(menu.transform,new Vector2(.79f,.6f),Vector2.zero,new Vector2(500,500),new Color(.15f,.65f,1,.08f));halo.sprite=IonArt.Glow;
            heroImage=Bar(menu.transform,new Vector2(.79f,.6f),Vector2.zero,new Vector2(430,430),Color.white);heroImage.preserveAspect=true;hero=heroImage.rectTransform;
            heroName=Label("",menu.transform,26,IonArt.Cyan,new Vector2(.79f,.6f),new Vector2(0,-245),new Vector2(520,40),TextAnchor.MiddleCenter);
            heroStatus=Label("THÔNG SỐ CHIẾN HẠM",menu.transform,15,muted,new Vector2(.79f,.6f),new Vector2(0,-306),new Vector2(530,90),TextAnchor.MiddleCenter);
            heroStatus.horizontalOverflow=HorizontalWrapMode.Wrap;
            menuBrief=Label("",menu.transform,16,text,new Vector2(.79f,.15f),Vector2.zero,new Vector2(530,110),TextAnchor.MiddleCenter);menuBrief.horizontalOverflow=HorizontalWrapMode.Wrap;
            Label("CHUỘT / WASD DI CHUYỂN  •  SPACE / CHUỘT TRÁI BẮN  •  SHIFT LƯỚT  •  Q ĐẠI CHIÊU",menu.transform,14,muted,new Vector2(.5f,0),new Vector2(0,20),new Vector2(1480,30),TextAnchor.MiddleCenter);
            Button("THOÁT",menu.transform,new Vector2(1,1),new Vector2(-100,-62),new Vector2(130,40),game.Quit,false);
        }
        void MakePause()
        {
            pause=Panel("Pause",transform,new Color(.01f,.02f,.045f,.96f));
            Label("TẠM DỪNG",pause.transform,52,text,Vector2.one*.5f,new Vector2(0,190),new Vector2(1000,85),TextAnchor.MiddleCenter);
            Label("P / ESC TIẾP TỤC • F TỰ BẮN • Q ĐẠI CHIÊU • M ÂM THANH",pause.transform,18,muted,Vector2.one*.5f,new Vector2(0,100),new Vector2(1300,50),TextAnchor.MiddleCenter);
            Button("TIẾP TỤC",pause.transform,Vector2.one*.5f,Vector2.zero,new Vector2(360,65),game.TogglePause,true);
            Button("VẬT PHẨM & VŨ KHÍ",pause.transform,Vector2.one*.5f,new Vector2(0,-90),new Vector2(360,65),ShowHelp,false);
            Button("TIÊU CHÍ 5 SAO",pause.transform,Vector2.one*.5f,new Vector2(0,-180),new Vector2(360,65),ShowRatingGuide,false);
            Button("VỀ CĂN CỨ",pause.transform,Vector2.one*.5f,new Vector2(0,-270),new Vector2(360,65),game.ReturnToMenu,false);
        }
        void MakeResult()
        {
            result=Panel("Debrief",transform,new Color(.01f,.025f,.05f,.97f));
            statusTitle=Label("",result.transform,43,text,Vector2.one*.5f,new Vector2(0,350),new Vector2(1450,70),TextAnchor.MiddleCenter);
            statusBody=Label("",result.transform,19,muted,Vector2.one*.5f,new Vector2(0,275),new Vector2(1400,70),TextAnchor.MiddleCenter);
            ratingBoard=Panel("Five-star debrief",result.transform,Color.clear);
            for(int i=0;i<5;i++)
            {
                ratingStars[i]=Star(ratingBoard.transform,new Vector2((i-2)*86,197),54,IonArt.Gold);
                var row=Bar(ratingBoard.transform,Vector2.one*.5f,new Vector2(0,78-i*58),new Vector2(960,53),new Color(.035f,.075f,.1f));
                ratingRows[i]=Label("",row.transform,17,text,Vector2.one*.5f,new Vector2(-45,10),new Vector2(800,26));
                ratingValues[i]=Label("",row.transform,14,muted,Vector2.one*.5f,new Vector2(-45,-12),new Vector2(800,24));
                ratingStates[i]=Label("",row.transform,16,IonArt.Gold,Vector2.one*.5f,new Vector2(408,0),new Vector2(130,35),TextAnchor.MiddleCenter);
            }
            ratingSummary=Label("",ratingBoard.transform,17,IonArt.Gold,Vector2.one*.5f,new Vector2(0,140),new Vector2(1000,35),TextAnchor.MiddleCenter);
            unlockImage=Bar(result.transform,Vector2.one*.5f,new Vector2(-605,10),new Vector2(175,175),Color.white);unlockImage.preserveAspect=true;
            nextButton=Button("CHỌN TÀU / MÀN TIẾP",result.transform,Vector2.one*.5f,new Vector2(-230,-255),new Vector2(420,60),game.NextStageHangar,true);
            replayButton=Button("CHƠI LẠI / SĂN 5 SAO",result.transform,Vector2.one*.5f,new Vector2(230,-255),new Vector2(420,60),game.BeginRun,false);
            Button("VỀ CĂN CỨ",result.transform,Vector2.one*.5f,new Vector2(0,-333),new Vector2(420,54),game.ReturnToMenu,false);
        }
        IonRatingStar Star(Transform parent,Vector2 position,float size,Color color)
        {
            var go=new GameObject("Rating star",typeof(RectTransform),typeof(IonRatingStar));go.transform.SetParent(parent,false);
            var rt=(RectTransform)go.transform;rt.anchorMin=rt.anchorMax=Vector2.one*.5f;rt.anchoredPosition=position;rt.sizeDelta=Vector2.one*size;
            var star=go.GetComponent<IonRatingStar>();star.color=color;star.raycastTarget=false;return star;
        }
        void MakeRatingGuide()
        {
            ratingGuide=Panel("Rating criteria",transform,new Color(.01f,.025f,.05f,.99f));
            Label("MỤC TIÊU 5 SAO",ratingGuide.transform,37,IonArt.Gold,Vector2.one*.5f,new Vector2(0,310),new Vector2(1200,65),TextAnchor.MiddleCenter);
            Label("Mỗi tiêu chí đạt khi thắng nhận 1 sao. Lưu thành tích tốt nhất của từng màn.",ratingGuide.transform,19,muted,Vector2.one*.5f,new Vector2(0,245),new Vector2(1350,45),TextAnchor.MiddleCenter);
            for(int i=0;i<5;i++)
            {
                Star(ratingGuide.transform,new Vector2(-520,147-i*74),30,IonArt.Gold);
                ratingGuideRows[i]=Label("",ratingGuide.transform,19,text,Vector2.one*.5f,new Vector2(40,147-i*74),new Vector2(1040,64));
            }
            Button("ĐÃ HIỂU",ratingGuide.transform,Vector2.one*.5f,new Vector2(0,-280),new Vector2(350,60),()=>ratingGuide.SetActive(false),true);ratingGuide.SetActive(false);
        }
        public void ShowRatingGuide()
        {
            for(int i=0;i<5;i++)ratingGuideRows[i].text=game.RatingCriterionName(i)+"\n"+game.RatingRequirement(i);
            ratingGuide.transform.SetAsLastSibling();ratingGuide.SetActive(true);
        }
        public void HideRatingGuide(){ratingGuide.SetActive(false);}
        void MakeHelp()
        {
            help=Panel("Supply guide",transform,new Color(.012f,.028f,.05f,1));
            Label("GHÉP KIỂU BẮN & CỘNG DỒN CHỈ SỐ",help.transform,34,text,new Vector2(.5f,1),new Vector2(0,-70),new Vector2(1450,65),TextAnchor.MiddleCenter);
            Label("Tối đa 2 kiểu bắn. Nhặt kiểu thứ ba thay kiểu cũ nhất.\nSát thương, nhịp bắn và số viên cộng dồn riêng, không chiếm ô kiểu bắn.",help.transform,18,muted,new Vector2(.5f,1),new Vector2(0,-137),new Vector2(1450,65),TextAnchor.MiddleCenter);
            string[] descriptions={
                "+60 loạt bắn. Tiếp tế khẩn cấp khi gần cạn.",
                "Sửa 30 HP, không vượt HP tối đa của tàu.",
                "Tăng cấp cả hai kiểu đang ghép, tối đa cấp 3.",
                "Bắn tỏa; ghép cùng xuyên hoặc truy đuổi.",
                "Mỗi viên xuyên tối đa 3 mục tiêu.",
                "Mỗi viên tự chỉnh hướng tìm mục tiêu.",
                "Nạp đại chiêu riêng của tàu. Q dùng, trữ 1.",
                "Mỗi lần +20% sát thương gốc, tối đa 10 lần.",
                "Mỗi lần +10% nhịp bắn gốc, tối đa 10 lần.",
                "Mỗi lần +1 viên trong loạt, tối đa 8 lần."};
            for(int i=0;i<IonCampaign.GiftNames.Length;i++)
            {
                float x=i<5?70:855,y=-230-(i%5)*98;
                var icon=Bar(help.transform,new Vector2(0,1),new Vector2(x+25,y),new Vector2(48,48),IonCampaign.GiftColors[i]);icon.sprite=IonArt.GiftIcons[i];
                Label(IonCampaign.GiftNames[i],help.transform,20,IonCampaign.GiftColors[i],new Vector2(0,1),new Vector2(x+365,y+19),new Vector2(590,36));
                Label(descriptions[i],help.transform,16,muted,new Vector2(0,1),new Vector2(x+365,y-20),new Vector2(590,43));
            }
            Label("Một loạt vẫn chỉ tốn 1 đạn. Các chỉ số và kiểu bắn đặt lại khi bắt đầu màn mới.",help.transform,16,muted,new Vector2(.5f,0),new Vector2(0,158),new Vector2(1450,40),TextAnchor.MiddleCenter);
            Button("ĐÃ HIỂU",help.transform,new Vector2(.5f,0),new Vector2(0,82),new Vector2(330,60),()=>help.SetActive(false),true);help.SetActive(false);
        }
        public void ShowHelp(){help.transform.SetAsLastSibling();help.SetActive(true);}
        public void Refresh()
        {
            bool playing=game.State==FlightState.Playing||game.State==FlightState.Paused||game.State==FlightState.BossDying||game.State==FlightState.UpgradeChoice;
            flight.SetActive(playing);menu.SetActive(game.State==FlightState.Menu);pause.SetActive(game.State==FlightState.Paused);
            bool clear=game.State==FlightState.StageClear,won=game.State==FlightState.Victory;
            result.SetActive(clear||won||game.State==FlightState.Defeat);nextButton.SetActive(clear);replayButton.SetActive(true);
            ((RectTransform)replayButton.transform).anchoredPosition=new Vector2(clear?230:0,-255);
            ratingBoard.SetActive(clear||won);
            for(int i=0;i<5;i++)
            {
                ratingStars[i].color=i<game.LastStars?IonArt.Gold:new Color(.16f,.24f,.29f);
                ratingRows[i].text=game.RatingCriterionName(i);ratingValues[i].text=game.LastRatingDetails[i];
                ratingStates[i].text=game.LastRatingEarned(i)?"+1 SAO":"CHƯA ĐẠT";ratingStates[i].color=game.LastRatingEarned(i)?IonArt.Gold:muted;
            }
            ratingSummary.text=game.LastStars+" / 5 SAO  •  TỐT NHẤT "+game.StarsAt(game.Sector)+" / 5"+(game.LastWasNewRating?"  •  THÀNH TÍCH MỚI!":"");
            if(!playing){banner.SetActive(false);tutorial.SetActive(false);toast.text="";toastTime=0;}
            if(game.State==FlightState.Playing||game.State==FlightState.BossDying){help.SetActive(false);ratingGuide.SetActive(false);}
            for(int i=0;i<hullCards.Length;i++)
            {
                hullButtons[i].interactable=i<game.UnlockedHulls;hullStates[i].text=i<game.UnlockedHulls?(i==game.SelectedHull?"ĐANG CHỌN":"ĐÃ MỞ KHÓA"):"THẮNG MÀN "+IonCampaign.HullUnlocks[i];
                hullCards[i].color=i==game.SelectedHull?new Color(.1f,.28f,.34f):new Color(.035f,.075f,.11f);
            }
            for(int i=0;i<stageButtons.Length;i++)
            {
                stageButtons[i].interactable=i+1<=game.UnlockedStages;stageTexts[i].text="M"+(i+1)+"\n"+(i+1>game.UnlockedStages?"KHÓA":game.StarsAt(i+1)>0?game.StarsAt(i+1)+"/5 SAO":i+1==game.Sector?"CHỌN":"SẴN SÀNG");
                stageButtons[i].image.color=i+1==game.Sector?new Color(.1f,.28f,.34f):new Color(.07f,.13f,.18f);
            }
            heroImage.sprite=IonArt.PlayerShips[game.SelectedHull];heroName.text=IonCampaign.HullNames[game.SelectedHull];heroName.color=IonCampaign.HullColors[game.SelectedHull];
            heroStatus.text=IonHullStats.Roles[game.SelectedHull]+" / "+IonHullStats.Summary(game.SelectedHull)+"\nQ / "+game.UltimateName;
            menuBrief.text="MÀN "+game.Sector+" / "+game.SectorName+"\n"+IonCampaign.Briefings[game.Sector-1];
            best.text=game.SessionUnlocked?"CHƠI THỬ / MỞ TẤT CẢ":"KỶ LỤC / "+game.Best.ToString("D6");
            statusTitle.text=clear?"HOÀN THÀNH MÀN "+game.Sector:won?"PHÁ VỠ ĐẾ HẠM":"MẤT TÍN HIỆU";
            int reward=IonCampaign.RewardHull(game.Sector);
            statusBody.text=(clear||won?(reward>=0?"NHẬN PHI CƠ "+IonCampaign.HullNames[reward]+" / ":"")+(won?"HOÀN TẤT 13 MÀN!":"ĐÃ MỞ MÀN "+(game.Sector+1)):game.DefeatReason+" Tiến trình mở khóa vẫn được giữ.")+"\nĐIỂM "+game.Score.ToString("D6")+" / KỶ LỤC "+game.Best.ToString("D6");
            statusBody.horizontalOverflow=HorizontalWrapMode.Wrap;
            unlockImage.gameObject.SetActive((clear||won)&&reward>=0);if(reward>=0)unlockImage.sprite=IonArt.PlayerShips[reward];
            Tick(0);RefreshTutorial();TickDraftInterface();
        }
        public void Tick(float dt)
        {
            TickDraftInterface();score.text=game.Score.ToString("D6");
            sector.text="MÀN "+game.Sector+"/13 — "+game.SectorName+"\n"+(game.BossDeathActive?"LÕI PHẢN ỨNG ĐANG SỤP ĐỔ":game.TutorialActive?"HUẤN LUYỆN":game.BossActive?"ĐỐI ĐẦU BOSS":game.Sector>=4?"CHIẾN DỊCH MỞ RỘNG":"ĐỢT "+Mathf.Min(3,game.Wave)+"/3");
            TickMission();
            ammo.text="ĐẠN "+game.Ammo;ammo.color=game.Ammo<30?IonArt.Orange:text;
            weapon.text=game.Loadout.Summary;
            boosts.text="SM "+game.GunDamage.ToString("0.00")+"  |  "+(1/game.GunDelay).ToString("0.0")+"/s  |  "+game.Loadout.ShotCount+" VIÊN";
            mode.text="SPACE / CHUỘT BẮN • F TỰ BẮN "+(game.AutoFire?"BẬT":"TẮT");
            ultimate.text="Q / "+game.UltimateName+"\n"+(game.UltimateActive?"ĐANG THI TRIỂN":game.UltimateCharges>0?"SẴN SÀNG ×1":"NHẶT LÕI ĐỎ");ultimate.color=game.UltimateCharges>0||game.UltimateActive?IonCampaign.HullColors[game.SelectedHull]:muted;
            playerPortrait.sprite=IonArt.PlayerShips[game.SelectedHull];playerName.text=IonCampaign.HullNames[game.SelectedHull];
            healthText.text="HP "+game.Health.ToString("0.#")+" / "+game.MaxHealth.ToString("0");
            healthFill.rectTransform.sizeDelta=new Vector2(150*game.HealthFraction,12);healthFill.color=game.HealthFraction<.3f?IonArt.Orange:IonCampaign.HullColors[game.SelectedHull];
            TickTarget();
            dash.text=game.DashReady>.99f?"SHIFT / LƯỚT":"ĐANG HỒI LƯỚT";dashFill.rectTransform.sizeDelta=new Vector2(165*game.DashReady,5);
            bossBar.SetActive(game.BossActive);bossTitle.text=game.BossBarName+" / "+Mathf.CeilToInt(game.BossBarHealth)+" / "+Mathf.CeilToInt(game.BossBarMaxHealth)+(game.BossBarShielded?" / KHIÊN ĐÓNG":"");bossFill.rectTransform.sizeDelta=new Vector2(520*game.BossBarHealth/game.BossBarMaxHealth,8);
        }
        public void RefreshTutorial()
        {
            tutorial.SetActive(game.TutorialActive&&(game.State==FlightState.Playing||game.State==FlightState.Paused));
            string[] titles={"01 / DI CHUYỂN","02 / KHAI HỎA","03 / LƯỚT NÉ ĐẠN","04 / NHẶT TIẾP TẾ","HUẤN LUYỆN HOÀN TẤT"};
            string[] bodies={"Di chuột hoặc dùng WASD / phím mũi tên để di chuyển.","Đưa tàu dưới mục tiêu, giữ SPACE hoặc chuột trái để bắn.\nMỗi loạt tốn 1 đạn. Nhấn F nếu muốn tự bắn.","Nhấn SHIFT hoặc chuột phải để lướt.\nLướt có thời gian hồi và bảo vệ ngắn.","Tiến lại gần hộp đạn xanh để hút vật phẩm.\nHình và tên cho biết loại phần thưởng.","Màn 1 sắp bắt đầu. Đạn được cấp lại đầy đủ.\nNhặt lõi đỏ để dùng đại chiêu Q một lần!"};
            int step=Mathf.Clamp(game.TutorialStep,0,4);tutorialTitle.text=titles[step];tutorialBody.text=bodies[step];
        }
        public void PickupLabel(int index,string value,Vector2 world,bool visible)
        {
            var label=pickupLabels[index];var vp=Camera.main.WorldToViewportPoint(world);
            bool show=visible&&game.State==FlightState.Playing&&vp.y>.14f&&vp.y<.85f&&vp.x>.04f&&vp.x<.96f;
            label.gameObject.SetActive(show);if(!show)return;
            label.text=value;var size=((RectTransform)transform).rect.size;label.rectTransform.anchoredPosition=new Vector2(Mathf.Clamp(vp.x*size.x,100,size.x-100),vp.y*size.y);
        }
        public void ShowBanner(bool visible,int number,string name)
        {
            banner.SetActive(visible&&!game.TutorialActive&&game.State==FlightState.Playing);bannerNumber.text="MÀN "+number+" / 13";bannerName.text=name;
        }
        void Update()
        {
            if(menu.activeSelf)hero.anchoredPosition=new Vector2(Mathf.Sin(Time.unscaledTime*.7f)*8,Mathf.Sin(Time.unscaledTime*1.1f)*10);
            if(toastTime>0){toastTime-=Time.unscaledDeltaTime;toast.color=new Color(1,.76f,.34f,Mathf.Clamp01(toastTime));if(toastTime<=0)toast.text="";}
            if(damageTime>0){damageTime-=Time.unscaledDeltaTime;damage.color=new Color(1,.12f,.02f,Mathf.Max(0,damageTime)*.55f);}
            if(blastTime>0&&game.State!=FlightState.Paused){blastTime=Mathf.Max(0,blastTime-Time.unscaledDeltaTime);blast.color=new Color(1,.95f,.85f,blastStrength*Mathf.Clamp01(blastTime/.35f));}
        }
        public bool SupportsVietnamese()
        {
            const string letters="ăâđêôơưàáảãạằắẳẵặầấẩẫậèéẻẽẹềếểễệìíỉĩịòóỏõọồốổỗộờớởỡợùúủũụừứửữựỳýỷỹỵ";
            foreach(char c in letters+letters.ToUpperInvariant())if(!regular.HasCharacter(c))return false;
            return true;
        }
        public void Toast(string message){toast.text=message;toastTime=2.5f;}
        public void FlashDamage(){damageTime=.28f;}
        public void FlashBlast(float strength){blastTime=.35f;blastStrength=strength;}
        public void ClearBlast(){blastTime=0;if(blast!=null)blast.color=Color.clear;}

        GameObject Panel(string name,Transform parent,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
            Stretch((RectTransform)go.transform);var im=go.GetComponent<Image>();im.color=color;im.raycastTarget=false;return go;
        }
        void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        Text Label(string value,Transform parent,int size,Color color,Vector2 anchor,Vector2 pos,Vector2 dimensions,TextAnchor align=TextAnchor.MiddleLeft)
        {
            var go=new GameObject(value,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);var t=go.GetComponent<Text>();
            t.font=regular;t.fontSize=size;t.color=color;t.text=value;t.alignment=align;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Overflow;t.verticalOverflow=VerticalWrapMode.Overflow;
            var r=t.rectTransform;r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=pos;r.sizeDelta=dimensions;return t;
        }
        Image Bar(Transform parent,Vector2 anchor,Vector2 pos,Vector2 size,Color color)
        {
            var go=new GameObject("Interface element",typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);var im=go.GetComponent<Image>();
            im.color=color;im.raycastTarget=false;var r=im.rectTransform;r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=pos;r.sizeDelta=size;return im;
        }
        GameObject Button(string title,Transform parent,Vector2 anchor,Vector2 pos,Vector2 size,UnityEngine.Events.UnityAction action,bool primary,bool invisible=false)
        {
            var im=Bar(parent,anchor,pos,size,invisible?Color.clear:primary?IonArt.Cyan:new Color(.07f,.13f,.18f));im.raycastTarget=true;
            var b=im.gameObject.AddComponent<UnityEngine.UI.Button>();var colors=b.colors;
            colors.normalColor=Color.white;colors.highlightedColor=new Color(.8f,.93f,1);colors.pressedColor=new Color(.55f,.78f,.9f);colors.selectedColor=Color.white;colors.fadeDuration=.1f;b.colors=colors;b.onClick.AddListener(action);
            b.navigation=new Navigation {mode=Navigation.Mode.None};
            Label(title,im.transform,19,primary?IonArt.Ink:text,Vector2.one*.5f,Vector2.zero,size,TextAnchor.MiddleCenter);return im.gameObject;
        }
    }
}

