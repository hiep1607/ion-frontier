using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace IonFrontier
{
    public enum IonMenuPage { Home, Campaign, Ships, Guide }
    public sealed partial class IonHud
    {
        static readonly Color MenuInk=new Color(.025f,.055f,.085f),MenuPanel=new Color(.055f,.105f,.15f,.98f),MenuMuted=new Color(.65f,.74f,.8f),MenuLine=new Color(.17f,.25f,.3f);
        const string ReducedMotionKey="IonFrontier.Next.MenuReducedMotion";
        RectTransform menuFrame;CanvasGroup menuInteraction;
        readonly GameObject[] menuPages=new GameObject[4];
        readonly CanvasGroup[] menuPageGroups=new CanvasGroup[4];
        readonly Button[] menuNav=new Button[4];
        readonly Image[] menuNavLines=new Image[4],menuHullImages=new Image[8];
        readonly Text[] menuHullNames=new Text[8];
        readonly IonRatingStar[] menuStars=new IonRatingStar[5];
        GameObject menuNavigation,menuModal;CanvasGroup menuModalGroup;Text menuModalTitle,menuModalBody,menuAudioText,menuMotionText;
        GameObject menuOptions,menuExit;Button menuLaunch,menuUseHull,menuUseStage,menuModalBack;
        Text menuSelectedMission,menuHullName,menuHullRole,menuHullStats,menuHullQ,menuHullLock;
        Text menuMissionName,menuMissionBrief,menuMissionBoss,menuMissionReward,menuMissionLock,menuMissionStars;
        Text menuGuideTitle;Text[] menuGuideBodies=new Text[5];GameObject[] menuGuideSections=new GameObject[5];
        readonly Button[] menuGuideTabs=new Button[5];
        Image menuMissionPicture;RectTransform menuMissionCrop;Sprite lastMenuPreview;
        int previewHull,previewStage=1,guideTopic;float menuTransition=1,menuMotionClock;bool menuWasVisible,menuReducedMotion;
        Vector2 menuHeroOrigin;GameObject legacyOverlayFocus;
        IonMenuPage menuPage;GameObject menuLastFocus;
        public bool ReducedMenuMotion=>menuReducedMotion;
        public IonMenuPage CurrentMenuPage=>menuPage;
        public bool MenuModalOpen=>menuModal!=null&&menuModal.activeSelf;
        public bool InformationOpen=>(help!=null&&help.activeSelf)||(ratingGuide!=null&&ratingGuide.activeSelf);

        void MakeModernMenu()
        {
            menuReducedMotion=PlayerPrefs.GetInt(ReducedMotionKey,0)!=0;
            menu=Panel("Hangar",transform,new Color(.015f,.03f,.05f,.18f));
            var shade=new GameObject("Menu readability gradient",typeof(RectTransform),typeof(IonMenuShade));shade.transform.SetParent(menu.transform,false);Stretch((RectTransform)shade.transform);shade.GetComponent<IonMenuShade>().raycastTarget=false;
            menuFrame=new GameObject("Menu design frame",typeof(RectTransform),typeof(CanvasGroup)).GetComponent<RectTransform>();menuFrame.SetParent(menu.transform,false);menuFrame.anchorMin=menuFrame.anchorMax=Vector2.one*.5f;menuFrame.sizeDelta=new Vector2(1600,900);
            menuInteraction=menuFrame.GetComponent<CanvasGroup>();
            MLabel("IF /",menuFrame,72,35,70,45,28,IonArt.Cyan);
            MLabel("ION FRONTIER",menuFrame,150,39,300,38,22,text);
            MButton("Menu.Options","TÙY CHỌN",menuFrame,1350,33,170,48,()=>OpenMenuModal(false));
            menuNavigation=MGroup("Menu navigation",menuFrame);
            string[] navTitles={"CĂN CỨ","CHIẾN DỊCH","KHO TÀU","CẨM NANG"};
            for(int i=0;i<4;i++)
            {
                int page=i;menuNav[i]=MButton("Menu.Nav."+(IonMenuPage)i,navTitles[i],menuNavigation.transform,650+i*160,32,150,55,()=>OpenMenuPage((IonMenuPage)page));
                menuNavLines[i]=MBox(menuNav[i].transform,9,53,132,2,IonArt.Cyan);menuNavLines[i].gameObject.SetActive(false);
            }
            MBox(menuFrame,72,824,1450,1,MenuLine);
            MLabel("CĂN CỨ QUỸ ĐẠO",menuFrame,74,845,600,30,15,MenuMuted);
            best=MLabel("",menuFrame,1050,845,470,30,15,MenuMuted,TextAnchor.MiddleRight);
            for(int i=0;i<4;i++)
            {
                menuPages[i]=MGroup("Menu page "+(IonMenuPage)i,menuFrame);menuPageGroups[i]=menuPages[i].AddComponent<CanvasGroup>();
            }
            MakeCinemaHome();MakeMenuShips();MakeMenuCampaign();MakeMenuGuide();MakeMenuModal();
            previewHull=game.SelectedHull;previewStage=game.Sector;
            OpenMenuPage(IonMenuPage.Home,false);LayoutMenuFrame();
        }
        void MakeCinemaHome()
        {
            var p=menuPages[0].transform;
            MLabel("CĂN CỨ QUỸ ĐẠO",p,90,145,500,30,17,IonArt.Cyan);
            var title=MLabel("ION\nFRONTIER",p,85,185,650,215,86,text);title.lineSpacing=.92f;
            menuLaunch=MButton("Menu.Home.Launch","XUẤT KÍCH                         →",p,90,425,430,72,LaunchFromMenu,true);
            MButton("Menu.Home.Campaign","CHIẾN DỊCH                         ↗",p,90,510,430,58,()=>OpenMenuPage(IonMenuPage.Campaign));
            MButton("Menu.Home.Ships","KHO TÀU                               ↗",p,90,578,430,58,()=>OpenMenuPage(IonMenuPage.Ships));
            MButton("Menu.Home.Guide","CẨM NANG                            ↗",p,90,646,430,58,()=>OpenMenuPage(IonMenuPage.Guide));
            menuSelectedMission=MLabel("",p,92,736,600,60,19,MenuMuted);
            heroImage=MPicture(p,850,140,590,550,null);hero=heroImage.rectTransform;hero.pivot=Vector2.one*.5f;hero.anchoredPosition=new Vector2(1145,-415);menuHeroOrigin=hero.anchoredPosition;hero.localRotation=Quaternion.Euler(0,0,-14);
            MLabel("CHIẾN HẠM ĐANG CHỌN",p,900,686,550,28,16,IonArt.Cyan);
            heroName=MLabel("",p,900,720,550,43,32,text);
            heroStatus=MLabel("",p,900,770,550,30,17,MenuMuted);
            MButton("Menu.Home.ChangeHull","ĐỔI TÀU ↗",p,1330,725,170,52,()=>OpenMenuPage(IonMenuPage.Ships));
        }
        void MakeMenuShips()
        {
            var p=menuPages[2].transform;
            MLabel("HẠM ĐỘI / 08",p,90,135,600,28,17,IonArt.Cyan);
            MLabel("KHO TÀU",p,86,174,800,65,45,text);
            for(int i=0;i<8;i++)
            {
                int hull=i;float x=90+i%4*170,y=280+i/4*226;
                var b=MButton("Menu.Ships.Preview."+i,"",p,x,y,155,208,()=>PreviewMenuHull(hull));
                hullButtons[i]=b;hullCards[i]=b.image;
                menuHullImages[i]=MPicture(b.transform,16,10,123,130,IonArt.PlayerShips[i]);
                menuHullNames[i]=MLabel(IonCampaign.HullNames[i],b.transform,5,144,145,28,17,text,TextAnchor.MiddleCenter);
                hullStates[i]=MLabel("",b.transform,5,175,145,28,13,MenuMuted,TextAnchor.MiddleCenter);
            }
            var inspector=MBox(p,830,265,670,465,MenuPanel).transform;
            menuHullRole=MLabel("",inspector,30,24,610,40,16,IonArt.Cyan);
            menuHullName=MLabel("",inspector,28,72,610,54,36,text);
            menuHullStats=MLabel("",inspector,30,145,610,100,23,text);
            MBox(inspector,30,255,610,1,MenuLine);
            menuHullQ=MLabel("",inspector,30,280,610,100,20,MenuMuted);
            menuHullLock=MLabel("",p,835,749,660,42,17,MenuMuted);
            menuUseHull=MButton("Menu.Ships.Use","DÙNG CHIẾN HẠM →",inspector,30,388,610,55,UsePreviewHull,true);
            MLabel("1–8 / XEM TÀU    ·    ENTER / DÙNG TÀU    ·    ESC / CĂN CỨ",p,92,776,1150,30,15,MenuMuted);
        }
        void MakeMenuCampaign()
        {
            var p=menuPages[1].transform;
            MLabel("13 KHU VỰC",p,90,135,600,28,17,IonArt.Cyan);MLabel("CHIẾN DỊCH",p,86,174,900,65,45,text);
            string[] groups={"CHƯƠNG MỞ ĐẦU","TUYẾN NGOẠI VI","TIẾN VÀO NỘI VỰC"};int[] starts={0,3,8},counts={3,5,5};
            for(int group=0;group<3;group++)
            {
                float y=285+group*148;MLabel(groups[group],p,92,y,650,27,15,MenuMuted);
                for(int column=0;column<counts[group];column++)
                {
                    int index=starts[group]+column;var button=MButton("Menu.Campaign.Preview."+(index+1),"",p,90+column*135,y+38,123,84,()=>PreviewMenuStage(index+1));
                    stageButtons[index]=button;stageTexts[index]=MLabel("",button.transform,4,8,115,68,19,text,TextAnchor.MiddleCenter);
                }
            }
            var card=MBox(p,830,265,670,475,MenuPanel).transform;
            var crop=MBox(card,0,0,670,150,Color.white);crop.gameObject.AddComponent<RectMask2D>();menuMissionCrop=crop.rectTransform;
            menuMissionPicture=MPicture(crop.transform,0,0,670,150,IonArt.StageBackdrops[0]);
            menuMissionName=MLabel("",card,28,170,610,78,30,text);
            menuMissionBrief=MLabel("",card,30,253,610,87,18,MenuMuted);
            menuMissionBoss=MLabel("",card,30,346,610,32,15,MenuMuted);
            menuMissionReward=MLabel("",card,30,380,610,32,15,MenuMuted);
            menuMissionStars=MLabel("",p,835,748,400,30,16,MenuMuted);
            for(int i=0;i<5;i++)
            {
                menuStars[i]=Star(p,new Vector2(488+i*36,-317),25,IonArt.Gold);
                var r=menuStars[i].rectTransform;r.anchorMin=r.anchorMax=new Vector2(0,1);r.anchoredPosition=new Vector2(1330+i*34,-765);
            }
            menuMissionLock=MLabel("",p,92,759,680,46,17,MenuMuted);
            menuUseStage=MButton("Menu.Campaign.Use","CHỌN NHIỆM VỤ →",card,30,421,610,42,UsePreviewStage,true);
        }
        void MakeMenuGuide()
        {
            var p=menuPages[3].transform;
            MLabel("HUẤN LUYỆN & TIẾP TẾ",p,90,135,800,28,17,IonArt.Cyan);MLabel("CẨM NANG PHI CÔNG",p,86,174,1300,65,45,text);
            string[] names={"ĐIỀU KHIỂN","VẬT PHẨM & KIỂU BẮN","CHÍN MÔ-ĐUN","TIÊU CHÍ 5 SAO","CHƠI LẠI HƯỚNG DẪN"};
            for(int i=0;i<5;i++){int topic=i;menuGuideTabs[i]=MButton("Menu.Guide.Topic."+i,names[i],p,90,285+i*78,425,62,()=>OpenGuideTopic(topic));}
            menuGuideTitle=MLabel("",p,605,277,880,60,30,text);
            var viewport=MBox(p,600,350,900,380,MenuPanel);viewport.gameObject.AddComponent<RectMask2D>();
            for(int i=0;i<5;i++)
            {
                menuGuideSections[i]=MGroup("Guide topic "+i,viewport.transform);
                var hitArea=menuGuideSections[i].AddComponent<Image>();hitArea.color=Color.clear;hitArea.raycastTarget=true;
                var scroll=menuGuideSections[i].AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=38;
                var content=new GameObject("Guide content",typeof(RectTransform)).GetComponent<RectTransform>();content.SetParent(menuGuideSections[i].transform,false);content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;content.sizeDelta=new Vector2(0,380);
                scroll.viewport=viewport.rectTransform;scroll.content=content;
                menuGuideBodies[i]=MLabel("",content,25,22,840,330,21,MenuMuted,TextAnchor.UpperLeft);
            }
            MLabel("CUỘN ĐỂ ĐỌC THÊM",p,605,742,800,25,14,MenuMuted);
            MButton("Menu.Guide.Tutorial","BẮT ĐẦU HƯỚNG DẪN →",p,605,774,540,48,()=>{if(menuPage==IonMenuPage.Guide&&guideTopic==4&&!MenuModalOpen)game.ReplayTutorial();},true).gameObject.SetActive(false);
            RefreshGuideContent();OpenGuideTopic(0);
        }
        void MakeMenuModal()
        {
            menuModal=Panel("Menu modal shield",menu.transform,new Color(.003f,.012f,.025f,.88f));menuModal.GetComponent<Image>().raycastTarget=true;menuModalGroup=menuModal.AddComponent<CanvasGroup>();
            var frame=new GameObject("Menu modal frame",typeof(RectTransform)).GetComponent<RectTransform>();frame.SetParent(menuModal.transform,false);frame.anchorMin=frame.anchorMax=Vector2.one*.5f;frame.sizeDelta=new Vector2(1600,900);
            var box=MBox(frame,450,200,700,500,MenuPanel).transform;
            menuModalTitle=MLabel("",box,38,32,624,70,35,text);
            menuModalBody=MLabel("",box,40,112,620,82,21,MenuMuted);
            menuOptions=MGroup("Menu options actions",box);
            var audio=MButton("Menu.Options.Audio","",menuOptions.transform,40,210,620,55,game.ToggleAudio);menuAudioText=audio.GetComponentInChildren<Text>();
            var motion=MButton("Menu.Options.Motion","",menuOptions.transform,40,277,620,55,ToggleMenuMotion);menuMotionText=motion.GetComponentInChildren<Text>();
            MButton("Menu.Options.Exit","THOÁT GAME →",menuOptions.transform,40,344,620,55,()=>OpenMenuModal(true));
            menuExit=MGroup("Menu exit actions",box);
            MButton("Menu.Exit.Confirm","THOÁT GAME",menuExit.transform,40,255,620,66,()=>{if(MenuModalOpen&&menuExit.activeSelf)game.Quit();},true);
            menuModalBack=MButton("Menu.Modal.Close","QUAY LẠI",box,40,420,620,54,CloseMenuModal);
            menuModal.SetActive(false);
        }
        void RefreshGuideContent()
        {
            menuGuideBodies[0].text="DI CHUYỂN\nChuột, WASD hoặc phím mũi tên.\n\nKHAI HỎA\nGiữ Space / chuột trái. F bật hoặc tắt tự bắn. Một loạt tốn một đạn.\n\nCƠ ĐỘNG\nShift / chuột phải: lướt. Q: đại chiêu riêng của tàu.\n\nTRONG TRẬN\nP / Esc: tạm dừng. M: bật hoặc tắt âm thanh.";
            string[] gifts={"Thêm 60 loạt bắn.","Sửa 30 HP, không vượt HP tối đa.","Tăng cấp hai kiểu bắn đang có, tối đa cấp 3.","Đạn phủ rộng; ghép được với Xuyên hoặc Truy Đuổi.","Đạn xuyên qua nhiều mục tiêu.","Đạn tự tìm mục tiêu.","Nạp một lần đại chiêu Q.","Mỗi lần cộng 20%, tối đa 10 lần.","Mỗi lần cộng 10%, tối đa 10 lần.","Mỗi lần thêm một viên, tối đa 8 lần."};
            string supply="Tối đa hai kiểu bắn. Kiểu thứ ba thay kiểu cũ nhất. Chỉ số cộng dồn dùng ô riêng.\n\n";
            for(int i=0;i<gifts.Length;i++)supply+=IonCampaign.GiftNames[i]+"\n"+gifts[i]+"\n\n";menuGuideBodies[1].text=supply;
            string modules="Tối đa hai mô-đun trong một lượt. Mô-đun cần kiểu bắn sẽ tạm ngừng khi mất kiểu tương ứng.\n\n";
            for(int i=0;i<9;i++)modules+=IonGame.ModuleName((CombatModule)i)+"\n"+game.MenuModuleDescription(i)+"\n\n";menuGuideBodies[2].text=modules;
            string stars="MÀN "+game.Sector+" / "+game.SectorName+"\nMỗi tiêu chí đạt khi thắng nhận một sao. Giữ thành tích tốt nhất.\n\n";
            for(int i=0;i<5;i++)stars+=(i+1)+". "+game.RatingCriterionName(i)+"\n"+game.RatingRequirement(i)+"\n\n";menuGuideBodies[3].text=stars;
            menuGuideBodies[4].text="LÀM QUEN CHUYẾN BAY\n\nBốn bước: di chuyển, bắn mục tiêu tập, lướt và nhặt tiếp tế.\n\nBắt đầu hướng dẫn sẽ đưa bạn tới màn 1. Tiến trình mở khóa và sao tốt nhất vẫn được giữ.\n\nBạn có thể bỏ qua hướng dẫn trong trận.";
            foreach(var body in menuGuideBodies)
            {
                float height=Mathf.Max(330,body.preferredHeight+18);body.rectTransform.sizeDelta=new Vector2(840,height);
                var content=(RectTransform)body.transform.parent;content.sizeDelta=new Vector2(0,height+44);
            }
        }
        public void OpenMenuPage(IonMenuPage page){OpenMenuPage(page,true);}
        void OpenMenuPage(IonMenuPage page,bool animate)
        {
            if(MenuModalOpen||InformationOpen)return;
            menuPage=page;menuTransition=animate&&!menuReducedMotion?0:1;
            for(int i=0;i<4;i++){bool active=i==(int)page;menuPages[i].SetActive(active);menuPageGroups[i].alpha=active?menuTransition:0;menuPageGroups[i].interactable=active&&menuTransition>=1;menuPageGroups[i].blocksRaycasts=active&&menuTransition>=1;menuNavLines[i].gameObject.SetActive(active);}
            menuNavigation.SetActive(page!=IonMenuPage.Home);
            if(page==IonMenuPage.Ships)previewHull=game.SelectedHull;if(page==IonMenuPage.Campaign)previewStage=game.Sector;
            RefreshMenuData();FocusMenuDefault();
        }
        void PreviewMenuHull(int hull){if(MenuModalOpen)return;previewHull=Mathf.Clamp(hull,0,7);RefreshMenuData();SetMenuFocus(menuUseHull.interactable?menuUseHull.gameObject:hullButtons[previewHull].gameObject);}
        void PreviewMenuStage(int stage){if(MenuModalOpen)return;previewStage=Mathf.Clamp(stage,1,13);RefreshMenuData();SetMenuFocus(menuUseStage.interactable?menuUseStage.gameObject:stageButtons[previewStage-1].gameObject);}
        void UsePreviewHull(){if(MenuModalOpen||InformationOpen||menuPage!=IonMenuPage.Ships||previewHull>=game.UnlockedHulls)return;if(game.SelectHull(previewHull))OpenMenuPage(IonMenuPage.Home);}
        void UsePreviewStage(){if(MenuModalOpen||InformationOpen||menuPage!=IonMenuPage.Campaign||previewStage>game.UnlockedStages)return;if(game.SelectStage(previewStage))OpenMenuPage(IonMenuPage.Home);}
        void LaunchFromMenu(){if(game.State==FlightState.Menu&&menuPage==IonMenuPage.Home&&!MenuModalOpen&&!InformationOpen&&menuTransition>=1)game.BeginRun();}
        void OpenGuideTopic(int topic)
        {
            guideTopic=Mathf.Clamp(topic,0,4);string[] names={"ĐIỀU KHIỂN CHUYẾN BAY","VẬT PHẨM & KIỂU BẮN","CHÍN MÔ-ĐUN CHIẾN ĐẤU","TIÊU CHÍ 5 SAO","CHƠI LẠI HƯỚNG DẪN"};menuGuideTitle.text=names[guideTopic];
            for(int i=0;i<5;i++){menuGuideSections[i].SetActive(i==guideTopic);menuGuideTabs[i].image.color=i==guideTopic?new Color(.08f,.25f,.3f):Color.clear;}
            FindMenuButton("Menu.Guide.Tutorial").gameObject.SetActive(guideTopic==4);
        }
        void RefreshModernMenu()
        {
            if(menuFrame==null)return;
            if(game.State!=FlightState.Menu){menuWasVisible=false;menuModal.SetActive(false);return;}
            if(!menuWasVisible){menuWasVisible=true;OpenMenuPage(IonMenuPage.Home,false);}
            RefreshMenuData();RefreshGuideContent();RefreshMenuModal();
        }
        void RefreshMenuData()
        {
            if(heroImage==null)return;
            heroImage.sprite=IonArt.PlayerShips[game.SelectedHull];heroName.text=IonCampaign.HullNames[game.SelectedHull];heroStatus.text=IonHullStats.Roles[game.SelectedHull]+" / HP "+IonHullStats.Health[game.SelectedHull];
            menuSelectedMission.text="ĐANG CHỌN / MÀN "+game.Sector.ToString("D2")+"\n"+game.SectorName;
            best.text=game.SessionUnlocked?"CHƠI THỬ / MỞ TẤT CẢ":"KỶ LỤC / "+game.Best.ToString("D6");
            for(int i=0;i<8;i++)
            {
                bool unlocked=i<game.UnlockedHulls;hullCards[i].color=i==previewHull?new Color(.08f,.26f,.31f):new Color(.045f,.09f,.13f,.97f);
                menuHullImages[i].color=unlocked?Color.white:new Color(.14f,.19f,.23f,1);menuHullNames[i].color=unlocked?text:MenuMuted;
                hullStates[i].text=!unlocked?"THẮNG MÀN "+IonCampaign.HullUnlocks[i]:i==game.SelectedHull?"ĐANG DÙNG":"ĐÃ MỞ KHÓA";
            }
            int hull=previewHull;menuHullName.text=IonCampaign.HullNames[hull];menuHullRole.text=IonHullStats.Roles[hull];
            menuHullStats.text="ĐỘ BỀN  "+IonHullStats.Health[hull].ToString("0")+" HP          TỐC ĐỘ  "+(10.5f*IonHullStats.Speed[hull]).ToString("0.00",CultureInfo.InvariantCulture)+"\n\nSÁT THƯƠNG  ×"+IonHullStats.Damage[hull].ToString("0.00",CultureInfo.InvariantCulture)+"       NHỊP BẮN  ×"+IonHullStats.Rate[hull].ToString("0.00",CultureInfo.InvariantCulture);
            menuHullQ.text="Q / "+game.MenuUltimateName(hull)+"\n"+game.MenuUltimateDescription(hull);
            menuUseHull.interactable=hull<game.UnlockedHulls;menuUseHull.GetComponentInChildren<Text>().text=hull<game.UnlockedHulls?"DÙNG CHIẾN HẠM →":"CHƯA MỞ KHÓA";
            menuHullLock.text=hull<game.UnlockedHulls?"Mỗi chiến hạm có chỉ số và đại chiêu riêng.":"Hoàn thành màn "+IonCampaign.HullUnlocks[hull]+" để mở chiến hạm này.";
            for(int i=0;i<13;i++)
            {
                bool unlocked=i+1<=game.UnlockedStages;stageButtons[i].image.color=i+1==previewStage?new Color(.08f,.26f,.31f):new Color(.045f,.09f,.13f,.97f);
                stageTexts[i].text=(i+1).ToString("D2")+"\n"+(!unlocked?"KHÓA":game.StarsAt(i+1)+" / 5 SAO");stageTexts[i].color=unlocked?text:MenuMuted;
            }
            int stage=previewStage-1;menuMissionName.text=previewStage.ToString("D2")+" / "+IonCampaign.StageNames[stage];menuMissionBrief.text=IonCampaign.Briefings[stage];menuMissionBoss.text="ĐỐI ĐẦU / "+IonCampaign.BossNames[stage];
            int reward=IonCampaign.RewardHull(previewStage);menuMissionReward.text="HOÀN THÀNH / "+(reward>=0?IonCampaign.HullNames[reward]+" · ":"")+(previewStage<13?"MỞ MÀN "+(previewStage+1):"HOÀN TẤT CHIẾN DỊCH");
            menuMissionPicture.sprite=IonArt.StageBackdrops[stage];FitMenuPreview();
            menuUseStage.interactable=previewStage<=game.UnlockedStages;menuUseStage.GetComponentInChildren<Text>().text=previewStage<=game.UnlockedStages?"CHỌN NHIỆM VỤ →":"CHƯA MỞ KHÓA";
            menuMissionLock.text=previewStage<=game.UnlockedStages?"Chọn một khu vực để xem nhiệm vụ.":"Hoàn thành màn "+(previewStage-1)+" để mở nhiệm vụ này.";
            menuMissionStars.text="THÀNH TÍCH TỐT NHẤT / "+game.StarsAt(previewStage)+" / 5";
            for(int i=0;i<5;i++)menuStars[i].color=i<game.StarsAt(previewStage)?IonArt.Gold:MenuLine;
        }
        void FitMenuPreview()
        {
            var sprite=menuMissionPicture.sprite;if(sprite==null)return;
            if(lastMenuPreview==sprite&&menuMissionPicture.rectTransform.sizeDelta.x>670)return;
            lastMenuPreview=sprite;float scale=Mathf.Max(670/sprite.rect.width,150/sprite.rect.height);
            var r=menuMissionPicture.rectTransform;r.anchorMin=r.anchorMax=Vector2.one*.5f;r.pivot=Vector2.one*.5f;r.anchoredPosition=Vector2.zero;r.sizeDelta=sprite.rect.size*scale;
        }
        void LayoutMenuFrame()
        {
            var size=((RectTransform)transform).rect.size;float scale=Mathf.Min(size.x/1600f,size.y/900f);menuFrame.localScale=Vector3.one*scale;
            if(menuModal!=null)((RectTransform)menuModal.transform.GetChild(0)).localScale=Vector3.one*scale;
        }
        void TickModernMenu()
        {
            if(menuFrame==null||!menu.activeSelf)return;LayoutMenuFrame();
            float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);menuTransition=menuReducedMotion?1:Mathf.Min(1,menuTransition+dt/.18f);
            var group=menuPageGroups[(int)menuPage];group.alpha=Mathf.SmoothStep(0,1,menuTransition);group.interactable=menuTransition>=1;group.blocksRaycasts=menuTransition>=1;
            bool info=InformationOpen;menuInteraction.interactable=!MenuModalOpen&&!info;menuInteraction.blocksRaycasts=!MenuModalOpen&&!info;
            if(!menuReducedMotion&&!MenuModalOpen&&!info)menuMotionClock+=dt;
            hero.anchoredPosition=menuHeroOrigin+(menuReducedMotion?Vector2.zero:new Vector2(Mathf.Sin(menuMotionClock*.7f)*5,Mathf.Sin(menuMotionClock*.9f)*7));
            if(MenuModalOpen)EnsureModalFocus(menuModal,menuModalBack);else if(info)EnsureModalFocus(ratingGuide.activeSelf?ratingGuide:help,null);
            else if(menuTransition>=1&&EventSystem.current!=null&&EventSystem.current.currentSelectedGameObject==null)FocusMenuDefault();
        }
        void ToggleMenuMotion(){SetMenuReducedMotion(!menuReducedMotion,true);}
        internal void SetMenuReducedMotion(bool value,bool persist)
        {
            menuReducedMotion=value;if(persist){PlayerPrefs.SetInt(ReducedMotionKey,value?1:0);PlayerPrefs.Save();}RefreshMenuModal();
        }
        void OpenMenuModal(bool exit)
        {
            menuLastFocus=EventSystem.current==null?null:EventSystem.current.currentSelectedGameObject;menuModal.SetActive(true);menuOptions.SetActive(!exit);menuExit.SetActive(exit);
            menuInteraction.interactable=false;menuInteraction.blocksRaycasts=false;RefreshMenuModal();EnsureModalFocus(menuModal,menuModalBack);
        }
        void RefreshMenuModal()
        {
            if(menuModal==null)return;bool exit=menuExit.activeSelf;menuModalTitle.text=exit?"THOÁT GAME?":"TÙY CHỌN";menuModalBody.text=exit?"Bạn có muốn rời căn cứ và thoát Ion Frontier?":"Điều chỉnh âm thanh và chuyển động tại căn cứ.";
            menuAudioText.text="ÂM THANH / "+(game.Muted?"TẮT":"BẬT")+"                 M";menuMotionText.text="GIẢM CHUYỂN ĐỘNG / "+(menuReducedMotion?"BẬT":"TẮT");
        }
        void CloseMenuModal()
        {
            menuModal.SetActive(false);menuInteraction.interactable=true;menuInteraction.blocksRaycasts=true;
            if(menuLastFocus!=null&&menuLastFocus.activeInHierarchy)SetMenuFocus(menuLastFocus);else FocusMenuDefault();
        }
        void FocusMenuDefault()
        {
            GameObject target=menuPage==IonMenuPage.Home?menuLaunch.gameObject:menuPage==IonMenuPage.Ships?(menuUseHull.interactable?menuUseHull.gameObject:hullButtons[Mathf.Clamp(previewHull,0,7)].gameObject):menuPage==IonMenuPage.Campaign?(menuUseStage.interactable?menuUseStage.gameObject:stageButtons[Mathf.Clamp(previewStage-1,0,12)].gameObject):menuGuideTabs[guideTopic].gameObject;SetMenuFocus(target);
        }
        void SetMenuFocus(GameObject target){if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(target);}
        void EnsureModalFocus(GameObject modal,Button fallback)
        {
            if(EventSystem.current==null)return;var current=EventSystem.current.currentSelectedGameObject;
            if(current!=null&&current.activeInHierarchy&&current.transform.IsChildOf(modal.transform))return;
            var selected=fallback!=null?fallback:modal.GetComponentInChildren<Button>();if(selected!=null)SetMenuFocus(selected.gameObject);
        }
        void FocusLegacyOverlay(GameObject overlay){legacyOverlayFocus=EventSystem.current==null?null:EventSystem.current.currentSelectedGameObject;EnsureModalFocus(overlay,null);}
        public bool HandleInformationInput()
        {
            if(!InformationOpen)return false;
            if(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.P))
            {
                if(ratingGuide.activeSelf)ratingGuide.SetActive(false);else help.SetActive(false);
                if(legacyOverlayFocus!=null&&legacyOverlayFocus.activeInHierarchy)SetMenuFocus(legacyOverlayFocus);else if(game.State==FlightState.Menu)FocusMenuDefault();
            }
            return true;
        }
        public void HandleMenuInput()
        {
            if(HandleInformationInput())return;
            if(Input.GetKeyDown(KeyCode.Escape)){if(MenuModalOpen)CloseMenuModal();else if(menuPage!=IonMenuPage.Home)OpenMenuPage(IonMenuPage.Home);else OpenMenuModal(false);return;}
            if(MenuModalOpen){if(Input.GetKeyDown(KeyCode.M))game.ToggleAudio();if(Input.GetKeyDown(KeyCode.Tab))CycleMenuFocus(Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)?-1:1);return;}
            if(Input.GetKeyDown(KeyCode.M))game.ToggleAudio();
            if(menuTransition<1)return;
            if(menuPage==IonMenuPage.Ships)for(int i=0;i<8;i++)if(Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1+i))){PreviewMenuHull(i);SetMenuFocus(menuUseHull.interactable?menuUseHull.gameObject:hullButtons[i].gameObject);}
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                CycleMenuFocus(Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)?-1:1);
            }
        }
        internal void CycleMenuFocus(int direction)
        {
            var buttons=MenuModalOpen?menuModal.GetComponentsInChildren<Button>():menuFrame.GetComponentsInChildren<Button>();
            int current=Array.FindIndex(buttons,b=>EventSystem.current!=null&&b.gameObject==EventSystem.current.currentSelectedGameObject);
            for(int n=0;n<buttons.Length;n++){current=(current+direction+buttons.Length)%buttons.Length;if(buttons[current].IsInteractable()){SetMenuFocus(buttons[current].gameObject);break;}}
        }
        internal Button FindMenuButton(string name)
        {
            foreach(var button in menu.GetComponentsInChildren<Button>(true))if(button.name==name)return button;return null;
        }
        GameObject MGroup(string name,Transform parent){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);Stretch((RectTransform)go.transform);return go;}
        RectTransform MRect(Component component,float x,float y,float w,float h)
        {
            var r=(RectTransform)component.transform;r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
        }
        Image MBox(Transform parent,float x,float y,float w,float h,Color color){var image=Bar(parent,Vector2.zero,Vector2.zero,Vector2.zero,color);MRect(image,x,y,w,h);return image;}
        Image MPicture(Transform parent,float x,float y,float w,float h,Sprite sprite){var image=MBox(parent,x,y,w,h,Color.white);image.sprite=sprite;image.preserveAspect=true;return image;}
        Text MLabel(string value,Transform parent,float x,float y,float w,float h,int size,Color color,TextAnchor align=TextAnchor.UpperLeft)
        {
            var label=Label(value,parent,size,color,Vector2.zero,Vector2.zero,Vector2.zero,align);MRect(label,x,y,w,h);label.horizontalOverflow=HorizontalWrapMode.Wrap;label.verticalOverflow=VerticalWrapMode.Truncate;return label;
        }
        Button MButton(string name,string title,Transform parent,float x,float y,float w,float h,UnityEngine.Events.UnityAction action,bool primary=false)
        {
            var go=Button(title,parent,Vector2.zero,Vector2.zero,new Vector2(w,h),action,primary);go.name=name;var button=go.GetComponent<Button>();MRect(button,x,y,w,h);
            button.image.color=primary?IonArt.Cyan:new Color(.04f,.085f,.12f,.75f);var colors=button.colors;colors.highlightedColor=new Color(.7f,.91f,1);colors.selectedColor=new Color(.7f,.91f,1);colors.disabledColor=new Color(.4f,.45f,.5f,.75f);colors.fadeDuration=.13f;button.colors=colors;button.navigation=new Navigation{mode=Navigation.Mode.Automatic};
            var label=button.GetComponentInChildren<Text>();label.fontSize=20;label.horizontalOverflow=HorizontalWrapMode.Wrap;label.verticalOverflow=VerticalWrapMode.Truncate;
            var r=label.rectTransform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(8,3);r.offsetMax=new Vector2(-8,-3);
            return button;
        }
    }
    public sealed partial class IonGame
    {
        public bool ReducedMenuMotion=>hud!=null&&hud.ReducedMenuMotion;
        public string MenuUltimateName(int hull)=>UltimateNames[Mathf.Clamp(hull,0,7)];
        public string MenuUltimateDescription(int hull)=>UltimateDescriptions[Mathf.Clamp(hull,0,7)];
        public string MenuModuleDescription(int module)=>moduleDescriptions[Mathf.Clamp(module,0,8)];
    }
}
