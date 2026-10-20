using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace IonFrontier
{
    public sealed class IonHud : MonoBehaviour
    {
        IonGame game; Canvas canvas; Font regular;
        GameObject flight, menu, pause, result, banner, bossBar;
        Text score, best, sector, weapon, dash, sound, mode, statusTitle, statusBody, bannerNumber, bannerName, toast;
        Image dashFill, bossFill, damage;
        Image[] shields = new Image[5];
        RectTransform hero; float timer, toastTime, damageTime; int shownBanner=-1;
        Color text=new Color(.84f,.91f,.97f), muted=new Color(.4f,.57f,.69f);

        public void Initialize(IonGame flightGame)
        {
            game=flightGame; regular=Resources.Load<Font>("Interface") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            if(EventSystem.current==null){var es=new GameObject("Flight UI input");es.AddComponent<EventSystem>();es.AddComponent<StandaloneInputModule>();}
            MakeFlight();MakeMenu();MakePause();MakeResult();
            banner=Panel("Sector announcement",transform,new Color(.02f,.045f,.075f,.82f));
            var br=(RectTransform)banner.transform;br.anchorMin=br.anchorMax=new Vector2(.5f,.5f);br.sizeDelta=new Vector2(650,140);br.anchoredPosition=new Vector2(0,65);
            bannerNumber=Label("KHU VỰC 01",banner.transform,17,IonArt.Cyan,new Vector2(.5f,.5f),new Vector2(0,35),new Vector2(620,35),TextAnchor.MiddleCenter);
            bannerName=Label("VÀNH ĐAI NGOÀI",banner.transform,32,text,new Vector2(.5f,.5f),new Vector2(0,-8),new Vector2(620,58),TextAnchor.MiddleCenter);
            Bar(banner.transform,new Vector2(.5f,0),new Vector2(0,1),new Vector2(650,2),IonArt.Cyan);
            banner.SetActive(false);
            toast=Label("",transform,19,IonArt.Gold,new Vector2(.5f,0),new Vector2(0,230),new Vector2(1000,40),TextAnchor.MiddleCenter);
            damage=Panel("Damage vignette",transform,new Color(1,.1f,.02f,0)).GetComponent<Image>();damage.raycastTarget=false;
        }

        void MakeFlight()
        {
            flight=new GameObject("Combat HUD",typeof(RectTransform));flight.transform.SetParent(transform,false);Stretch((RectTransform)flight.transform);
            var top=Panel("Command strip",flight.transform,new Color(.015f,.03f,.06f,.9f));
            var r=(RectTransform)top.transform;r.anchorMin=new Vector2(0,1);r.anchorMax=new Vector2(1,1);r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(0,94);
            Label("ION / FRONTIER",top.transform,24,text,new Vector2(0,.5f),new Vector2(156,10),new Vector2(240,34));
            Label("PHI ĐỘI TIÊN PHONG",top.transform,13,muted,new Vector2(0,.5f),new Vector2(156,-18),new Vector2(240,24));
            sector=Label("KHU VỰC 01 / 05",top.transform,18,IonArt.Cyan,new Vector2(.5f,.5f),new Vector2(0,0),new Vector2(620,44),TextAnchor.MiddleCenter);
            score=Label("000000",top.transform,31,text,new Vector2(1,.5f),new Vector2(-138,8),new Vector2(210,42),TextAnchor.MiddleRight);
            Label("ĐIỂM NHIỆM VỤ",top.transform,13,muted,new Vector2(1,.5f),new Vector2(-138,-21),new Vector2(210,24),TextAnchor.MiddleRight);
            Bar(top.transform,new Vector2(.5f,0),Vector2.zero,new Vector2(1600,1),new Color(.14f,.31f,.41f,.7f));
            var bottom=Panel("Systems strip",flight.transform,new Color(.015f,.03f,.06f,.9f));
            r=(RectTransform)bottom.transform;r.anchorMin=Vector2.zero;r.anchorMax=new Vector2(1,0);r.pivot=new Vector2(.5f,0);r.sizeDelta=new Vector2(0,83);
            Label("KHIÊN",bottom.transform,14,muted,new Vector2(0,.5f),new Vector2(81,14),new Vector2(92,24));
            for(int i=0;i<5;i++)shields[i]=Bar(bottom.transform,new Vector2(0,.5f),new Vector2(48+i*31,-12),new Vector2(24,8),IonArt.Cyan);
            mode=Label("TỰ BẮN  /  BẬT",bottom.transform,16,text,new Vector2(.5f,.5f),new Vector2(-180,0),new Vector2(270,32),TextAnchor.MiddleCenter);
            weapon=Label("VŨ KHÍ  /  CẤP 01",bottom.transform,16,text,new Vector2(.5f,.5f),new Vector2(120,0),new Vector2(260,32),TextAnchor.MiddleCenter);
            dash=Label("LƯỚT SẴN SÀNG",bottom.transform,14,IonArt.Cyan,new Vector2(1,.5f),new Vector2(-142,13),new Vector2(210,26),TextAnchor.MiddleRight);
            Bar(bottom.transform,new Vector2(1,.5f),new Vector2(-141,-13),new Vector2(190,5),new Color(.12f,.21f,.28f));
            dashFill=Bar(bottom.transform,new Vector2(1,.5f),new Vector2(-236,-13),new Vector2(190,5),IonArt.Cyan);
            dashFill.rectTransform.pivot=new Vector2(0,.5f);
            Bar(bottom.transform,new Vector2(.5f,1),Vector2.zero,new Vector2(1600,1),new Color(.14f,.31f,.41f,.7f));
            bossBar=new GameObject("Dreadnought integrity",typeof(RectTransform));bossBar.transform.SetParent(flight.transform,false);
            r=(RectTransform)bossBar.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-126);r.sizeDelta=new Vector2(550,65);
            Label("THIẾT GIÁP HẠM  /  GIÁP TÀU",bossBar.transform,15,IonArt.Orange,new Vector2(.5f,.5f),new Vector2(0,17),new Vector2(550,28),TextAnchor.MiddleCenter);
            Bar(bossBar.transform,new Vector2(.5f,.5f),new Vector2(0,-10),new Vector2(550,5),new Color(.25f,.1f,.07f));
            bossFill=Bar(bossBar.transform,new Vector2(.5f,.5f),new Vector2(-275,-10),new Vector2(550,5),IonArt.Orange);bossFill.rectTransform.pivot=new Vector2(0,.5f);
        }

        void MakeMenu()
        {
            menu=Panel("Flight terminal",transform,new Color(.01f,.02f,.04f,.18f));
            Label("I F   /   BỘ CHỈ HUY QUỸ ĐẠO",menu.transform,17,IonArt.Cyan,new Vector2(0,1),new Vector2(320,-72),new Vector2(440,40));
            Label("ION",menu.transform,100,text,new Vector2(0,.5f),new Vector2(385,147),new Vector2(570,125));
            Label("FRONTIER",menu.transform,100,text,new Vector2(0,.5f),new Vector2(385,47),new Vector2(570,125));
            Bar(menu.transform,new Vector2(0,.5f),new Vector2(138,-37),new Vector2(76,4),IonArt.Cyan);
            Label("MỘT PHI THUYỀN. NĂM VÙNG CHIẾN SỰ.",menu.transform,20,IonArt.Cyan,new Vector2(0,.5f),new Vector2(400,-82),new Vector2(600,40));
            Label("Phá vòng phong tỏa. Nâng cấp vũ khí ion.\nHạ gục tàu chỉ huy của hạm đội địch.",menu.transform,20,muted,new Vector2(0,.5f),new Vector2(410,-139),new Vector2(620,70));
            Button("XUẤT KÍCH    >",menu.transform,new Vector2(0,.5f),new Vector2(270,-235),new Vector2(340,66),game.BeginRun,true);
            Button("THOÁT",menu.transform,new Vector2(0,.5f),new Vector2(500,-235),new Vector2(100,66),game.Quit,false);
            best=Label("KỶ LỤC  /  000000",menu.transform,16,muted,new Vector2(0,.5f),new Vector2(270,-298),new Vector2(340,32));
            var halo=Bar(menu.transform,new Vector2(.76f,.57f),Vector2.zero,new Vector2(550,550),new Color(.15f,.65f,1,.08f));halo.sprite=IonArt.Glow;
            var ring=Bar(menu.transform,new Vector2(.76f,.57f),Vector2.zero,new Vector2(450,450),new Color(.2f,.65f,.8f,.11f));ring.sprite=IonArt.Ring;
            var ship=Bar(menu.transform,new Vector2(.76f,.57f),Vector2.zero,new Vector2(610,610),Color.white);ship.sprite=IonArt.Ships[0];ship.preserveAspect=true;hero=ship.rectTransform;
            Label("TIÊN PHONG   /   MK-01",menu.transform,17,IonArt.Cyan,new Vector2(.76f,.57f),new Vector2(0,-297),new Vector2(500,35),TextAnchor.MiddleCenter);
            Label("DI CHUYỂN  CHUỘT / WASD    |    BẮN  SPACE / CHUỘT TRÁI    |    LƯỚT  SHIFT / CHUỘT PHẢI",menu.transform,17,muted,new Vector2(.5f,0),new Vector2(0,90),new Vector2(1400,38),TextAnchor.MiddleCenter);
            Label("F  TỰ BẮN      P / ESC  TẠM DỪNG      M  ÂM THANH",menu.transform,16,muted,new Vector2(.5f,0),new Vector2(0,55),new Vector2(1100,32),TextAnchor.MiddleCenter);
            sound=Label("ÂM THANH BẬT",menu.transform,15,IonArt.Cyan,new Vector2(1,1),new Vector2(-110,-72),new Vector2(150,40),TextAnchor.MiddleRight);
            Button("",menu.transform,new Vector2(1,1),new Vector2(-110,-72),new Vector2(160,45),game.ToggleAudio,false,true);
        }

        void MakePause()
        {
            pause=Panel("Pause overlay",transform,new Color(.01f,.02f,.045f,.93f));
            Label("TẠM DỪNG",pause.transform,56,text,new Vector2(.5f,.5f),new Vector2(0,150),new Vector2(1000,85),TextAnchor.MiddleCenter);
            Label("Nghỉ một chút. Hệ thống đang chờ lệnh.",pause.transform,21,muted,new Vector2(.5f,.5f),new Vector2(0,75),new Vector2(1000,50),TextAnchor.MiddleCenter);
            Button("TIẾP TỤC",pause.transform,new Vector2(.5f,.5f),new Vector2(0,-25),new Vector2(360,64),game.TogglePause,true);
            Button("VỀ MÀN HÌNH CHÍNH",pause.transform,new Vector2(.5f,.5f),new Vector2(0,-107),new Vector2(360,64),game.ReturnToMenu,false);
            Label("CHUỘT / WASD  DI CHUYỂN    |    SHIFT / CHUỘT PHẢI  LƯỚT\nF  TỰ BẮN    |    SPACE / CHUỘT TRÁI  BẮN    |    M  ÂM THANH",pause.transform,18,muted,new Vector2(.5f,.5f),new Vector2(0,-219),new Vector2(1100,78),TextAnchor.MiddleCenter);
        }
        void MakeResult()
        {
            result=Panel("Mission debrief",transform,new Color(.01f,.025f,.05f,.94f));
            Label("BÁO CÁO NHIỆM VỤ",result.transform,18,IonArt.Cyan,new Vector2(.5f,.5f),new Vector2(0,225),new Vector2(900,35),TextAnchor.MiddleCenter);
            statusTitle=Label("HOÀN THÀNH NHIỆM VỤ",result.transform,60,text,new Vector2(.5f,.5f),new Vector2(0,136),new Vector2(1400,100),TextAnchor.MiddleCenter);
            statusBody=Label("",result.transform,25,muted,new Vector2(.5f,.5f),new Vector2(0,28),new Vector2(1200,100),TextAnchor.MiddleCenter);
            Button("XUẤT KÍCH LẠI",result.transform,new Vector2(.5f,.5f),new Vector2(0,-110),new Vector2(360,64),game.BeginRun,true);
            Button("VỀ MÀN HÌNH CHÍNH",result.transform,new Vector2(.5f,.5f),new Vector2(0,-191),new Vector2(360,64),game.ReturnToMenu,false);
            Label("NHẤN R ĐỂ CHƠI LẠI",result.transform,16,muted,new Vector2(.5f,.5f),new Vector2(0,-261),new Vector2(600,32),TextAnchor.MiddleCenter);
        }

        public void Refresh()
        {
            bool playing=game.State==FlightState.Playing||game.State==FlightState.Paused;
            flight.SetActive(playing);menu.SetActive(game.State==FlightState.Menu);pause.SetActive(game.State==FlightState.Paused);
            result.SetActive(game.State==FlightState.Defeat||game.State==FlightState.Victory);
            score.text=game.Score.ToString("D6");best.text="KỶ LỤC  /  "+game.Best.ToString("D6");
            sector.text="KHU VỰC "+game.Sector.ToString("D2")+" / 05   -   "+game.SectorName;
            weapon.text="VŨ KHÍ  /  CẤP "+game.WeaponTier.ToString("D2");mode.text="TỰ BẮN  /  "+(game.AutoFire?"BẬT":"TẮT");
            sound.text=game.Muted?"ÂM THANH TẮT":"ÂM THANH BẬT";
            for(int i=0;i<shields.Length;i++)shields[i].color=i<game.Shield?IonArt.Cyan:new Color(.1f,.2f,.27f);
            bool won=game.State==FlightState.Victory;
            statusTitle.text=won?"PHÁ VỠ PHONG TỎA":"MẤT TÍN HIỆU";
            statusTitle.color=won?IonArt.Cyan:text;
            statusBody.text=(won?"Đã kiểm soát cả năm khu vực. Chào mừng trở về!":"Phi thuyền của bạn đã bị hạ tại khu vực "+game.Sector.ToString("D2")+".")+"\n\nĐIỂM   "+game.Score.ToString("D6")+"          KỶ LỤC   "+game.Best.ToString("D6");
            bossBar.SetActive(playing&&game.Sector==5);
            if(!playing)banner.SetActive(false);
        }
        public void Tick(float dt)
        {
            timer+=dt;if(timer>.06f)
            {
                timer=0;dashFill.rectTransform.sizeDelta=new Vector2(190*game.DashReady,5);
                dash.text=game.DashReady>.99f?"LƯỚT SẴN SÀNG":"ĐANG HỒI LƯỚT";
                bossFill.rectTransform.sizeDelta=new Vector2(550*game.BossFraction,5);
            }
        }
        void Update()
        {
            if(hero!=null&&menu.activeSelf)hero.anchoredPosition=new Vector2(Mathf.Sin(Time.unscaledTime*.7f)*9,Mathf.Sin(Time.unscaledTime*1.1f)*12);
            if(toastTime>0){toastTime-=Time.unscaledDeltaTime;toast.color=new Color(1,.76f,.34f,Mathf.Clamp01(toastTime));if(toastTime<=0)toast.text="";}
            if(damageTime>0){damageTime-=Time.unscaledDeltaTime;damage.color=new Color(1,.12f,.02f,Mathf.Max(0,damageTime)*.55f);}
        }
        public void ShowBanner(bool visible,int number,string name)
        {
            if(banner.activeSelf!=visible)banner.SetActive(visible);
            if(shownBanner==number)return;shownBanner=number;bannerNumber.text="KHU VỰC "+number.ToString("D2")+" / 05";bannerName.text=name;
        }
        public bool SupportsVietnamese()
        {
            const string letters="ăâđêôơưàáảãạằắẳẵặầấẩẫậèéẻẽẹềếểễệìíỉĩịòóỏõọồốổỗộờớởỡợùúủũụừứửữựỳýỷỹỵ";
            foreach(char c in letters+letters.ToUpperInvariant())if(!regular.HasCharacter(c))return false;
            return true;
        }
        public void Toast(string message){toast.text=message;toastTime=2.5f;}
        public void FlashDamage(){damageTime=.28f;}

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
        void Button(string title,Transform parent,Vector2 anchor,Vector2 pos,Vector2 size,UnityEngine.Events.UnityAction action,bool primary,bool invisible=false)
        {
            var im=Bar(parent,anchor,pos,size,invisible?Color.clear:primary?IonArt.Cyan:new Color(.07f,.13f,.18f));im.raycastTarget=true;
            var b=im.gameObject.AddComponent<UnityEngine.UI.Button>();var colors=b.colors;
            colors.normalColor=Color.white;colors.highlightedColor=new Color(.8f,.93f,1);colors.pressedColor=new Color(.55f,.78f,.9f);colors.selectedColor=Color.white;colors.fadeDuration=.1f;b.colors=colors;b.onClick.AddListener(action);
            b.navigation=new Navigation {mode=Navigation.Mode.None};
            Label(title,im.transform,19,primary?IonArt.Ink:text,Vector2.one*.5f,Vector2.zero,size,TextAnchor.MiddleCenter);
        }
    }
}

