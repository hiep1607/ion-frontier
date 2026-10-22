using UnityEngine;

namespace IonFrontier
{
    public sealed class IonBackdrop : MonoBehaviour
    {
        struct Star {public SpriteRenderer renderer;public Vector2 position;public float speed,phase,size;}
        struct Scenery {public SpriteRenderer renderer;public Vector2 position;public float speed,angle,size;}
        public static readonly string[] ThemeNames={"QUỸ ĐẠO HUẤN LUYỆN","KHE BỤI ĐỘT KÍCH","CỬA THÀNH TRÌ","NGHĨA ĐỊA CHIẾN HẠM","HÀNH LANG SƠ TÁN","VÀNH TRẠM QUỸ ĐẠO","ĐƯỜNG HẦM SIÊU TỐC","SƯƠNG TÍM CỨU HỘ","MẠNG LÕI LỤC BẢO","VÀNH ĐAI KHAI MỎ","NGÃ BA HAI TÍN HIỆU","RÌA KHÔNG GIAN SỤP ĐỔ","NỘI VỰC ĐẾ HẠM"};
        static readonly int[] Density={145,105,48,72,90,52,105,35,46,70,85,115,32};
        static readonly float[] Motion={.7f,1.25f,.35f,.68f,.8f,.4f,5.5f,.26f,.32f,.55f,.65f,2.7f,.23f};
        readonly Star[] stars=new Star[160];readonly Scenery[] scenery=new Scenery[10];
        readonly SpriteRenderer[] atmosphere=new SpriteRenderer[3];
        SpriteRenderer sky;Camera view;IonGame game;float width,clock,combatDim=1;int theme=-1;
        public int BackgroundStage=>theme+1;
        public string ThemeName=>theme>=0?ThemeNames[theme]:"";
        public Sprite BackgroundSprite=>sky==null?null:sky.sprite;
        public float MotionClock=>clock;
        public int ActiveLayers{get{int n=1;foreach(var s in stars)if(s.renderer!=null&&s.renderer.enabled)n++;foreach(var s in scenery)if(s.renderer!=null&&s.renderer.enabled)n++;foreach(var a in atmosphere)if(a!=null&&a.enabled)n++;return n;}}
        public bool CoversCamera
        {
            get{if(sky==null||view==null)return false;var b=sky.bounds;float h=view.orthographicSize,w=h*view.aspect;var p=view.transform.position;return b.min.x<=p.x-w&&b.max.x>=p.x+w&&b.min.y<=p.y-h&&b.max.y>=p.y+h;}
        }
        public void Initialize(float halfWidth,IonGame owner)
        {
            width=halfWidth;game=owner;view=Camera.main;
            sky=IonArt.Sprite("Sector environment",IonArt.StageBackdrops[0],Vector2.zero,Vector2.one,Color.white,-100,transform);
            for(int i=0;i<stars.Length;i++)stars[i].renderer=IonArt.Sprite("Distant light",IonArt.Disc,Vector2.zero,Vector2.one,Color.white,-93,transform);
            for(int i=0;i<scenery.Length;i++)scenery[i].renderer=IonArt.Sprite("Distant scenery",IonArt.MissionProps[0],Vector2.zero,Vector2.one,Color.white,-88,transform);
            for(int i=0;i<atmosphere.Length;i++)atmosphere[i]=IonArt.Sprite("Environmental atmosphere",IonArt.Glow,Vector2.zero,Vector2.one,Color.white,-86,transform);
            FitCamera();SetTheme(0);RenderAtmosphere();
        }
        void FitCamera()
        {
            if(view==null)return;float h=view.orthographicSize,w=h*view.aspect;
            width=w;float scale=Mathf.Max((w*2+2)/sky.sprite.bounds.size.x,(h*2+2)/sky.sprite.bounds.size.y);
            sky.transform.localScale=Vector3.one*scale;
        }
        void SetTheme(int index)
        {
            theme=index;clock=0;combatDim=game.BossActive?.72f:1;sky.sprite=IonArt.StageBackdrops[theme];FitCamera();
            var rng=new System.Random(7001+index*193);
            for(int i=0;i<stars.Length;i++)
            {
                ref var s=ref stars[i];float depth=(float)rng.NextDouble();s.position=new Vector2((float)rng.NextDouble()*width*2-width,(float)rng.NextDouble()*16-8);s.speed=.14f+depth*.65f;s.phase=(float)rng.NextDouble()*6.28f;s.size=Mathf.Lerp(.018f,.055f,depth);
                s.renderer.enabled=i<Density[theme];s.renderer.sprite=IonArt.Disc;s.renderer.transform.position=s.position;
            }
            int count=theme==0?4:theme==2||theme==5||theme==8?6:theme==3?10:theme==4?5:theme==9?8:theme==10?4:theme==11?9:0;
            for(int i=0;i<scenery.Length;i++)
            {
                ref var s=ref scenery[i];s.renderer.enabled=i<count;
                int prop=theme==0||theme==8?4:theme==1||theme==2||theme==5?2:theme==3||theme==11?0:theme==4||theme==10?5:6;
                s.renderer.sprite=IonArt.MissionProps[prop];s.position=new Vector2((i%2==0?-1:1)*(width*.72f+(float)rng.NextDouble()*width*.2f),(float)rng.NextDouble()*17-8.5f);
                s.speed=.12f+(float)rng.NextDouble()*.23f;s.size=theme==3||theme==11?.55f+(float)rng.NextDouble()*.65f:.55f+(float)rng.NextDouble()*.45f;
                s.angle=(float)rng.NextDouble()*360;if(theme==4||theme==10)s.angle=0;
                s.renderer.transform.localScale=Vector3.one*IonArt.ScaleFor(s.renderer.sprite,s.size);s.renderer.transform.position=s.position;s.renderer.transform.rotation=Quaternion.Euler(0,0,s.angle);
            }
            for(int i=0;i<atmosphere.Length;i++){atmosphere[i].enabled=theme==7||theme==8||theme==10||theme==11||theme==12;atmosphere[i].sprite=theme==8||theme==12?IonArt.Ring:IonArt.Glow;}
        }
        void Update()
        {
            if(sky==null)return;int next=Mathf.Clamp(game.Sector-1,0,12);if(next!=theme)SetTheme(next);
            FitCamera();if(game.State==FlightState.Paused||game.State==FlightState.UpgradeChoice)return;
            float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);float pace=game.State==FlightState.Menu?.35f:game.State==FlightState.StageClear||game.State==FlightState.Victory||game.State==FlightState.Defeat?.18f:1;
            clock+=dt*pace;float motion=(game.EscapeRunActive?2.4f+game.EscapeFraction*1.8f:Motion[theme])*(game.BossActive?.62f:1)*pace;
            combatDim=Mathf.MoveTowards(combatDim,game.BossActive?.72f:1,dt*.28f);
            sky.color=new Color(.76f*combatDim,.76f*combatDim,.79f*combatDim,1);
            sky.transform.position=new Vector2(Mathf.Sin(clock*.09f)*.18f,Mathf.Cos(clock*.07f)*.12f);
            Color color=IonCampaign.StageColors[theme];
            for(int i=0;i<Density[theme];i++)
            {
                ref var s=ref stars[i];s.position.y-=s.speed*dt*motion;if(theme==1)s.position.x-=s.speed*dt*motion*.25f;if(theme==11)s.position.x+=s.speed*dt*motion*.55f;
                if(s.position.y<-8)s.position.y=8;if(s.position.x>width)s.position.x=-width;if(s.position.x<-width)s.position.x=width;
                s.renderer.transform.position=s.position;
                float length=game.EscapeRunActive?s.size*(2+s.speed*4):theme==6?s.size*(4+s.speed*8):theme==11?s.size*3:s.size;s.renderer.transform.localScale=new Vector3(s.size,length,1);
                s.renderer.transform.rotation=Quaternion.Euler(0,0,theme==11?28:theme==1?-14:0);
                Color tint=theme==11?new Color(1,.35f,.12f):theme==6?new Color(.5f,.9f,1):Color.Lerp(new Color(.55f,.7f,.9f),color,.25f);
                tint.a=(.14f+Mathf.Sin(clock*.8f+s.phase)*.06f)*combatDim;s.renderer.color=tint;
            }
            for(int i=0;i<scenery.Length;i++)
            {
                ref var s=ref scenery[i];if(!s.renderer.enabled)continue;s.position.y-=s.speed*dt*motion;if(s.position.y<-8.5f)s.position.y=8.5f;
                s.renderer.transform.position=s.position;if(theme==3||theme==9||theme==11)s.angle+=dt*motion*(i%2==0?2:-3);s.renderer.transform.rotation=Quaternion.Euler(0,0,s.angle);
                s.renderer.color=new Color(.45f,.5f,.58f,(theme==3?.19f:.13f)*combatDim);
            }
            RenderAtmosphere();
        }
        void RenderAtmosphere()
        {
            for(int i=0;i<atmosphere.Length;i++)
            {
                var a=atmosphere[i];if(!a.enabled)continue;
                if(theme==7)
                {a.transform.position=new Vector2(Mathf.Sin(clock*.055f+i*2)*width*.55f,(i-1)*3+Mathf.Sin(clock*.08f+i));a.transform.localScale=new Vector3(24,7+i,1);a.color=new Color(.44f,.25f,.7f,(.08f+Mathf.Sin(clock*.11f+i)*.015f)*combatDim);}
                else if(theme==8)
                {a.transform.position=new Vector2(i==0?-width*.78f:i==1?width*.78f:0,i==2?-6:3.5f);a.transform.localScale=Vector3.one*(4.2f+Mathf.Sin(clock*.4f+i)*.1f);a.color=new Color(.15f,1,.6f,.04f*combatDim);}
                else if(theme==10)
                {int route=game.MissionRoute;bool selected=route==0||route==1&&i==0||route==2&&i==1;a.transform.position=new Vector2(i==0?-width*.75f:width*.75f,1.5f);a.transform.localScale=new Vector3(13,17,1);a.color=i==0?new Color(.1f,.65f,1,selected?.045f:.012f):new Color(.65f,.25f,1,selected?.045f:.012f);if(i==2)a.color=Color.clear;}
                else if(theme==11)
                {a.transform.position=new Vector2((i-1)*width*.75f,-7.5f+Mathf.Sin(clock*.2f+i)*.2f);a.transform.localScale=new Vector3(14,6,1);a.color=new Color(1,.16f,.05f,.065f*combatDim);}
                else
                {a.transform.position=new Vector2(i==0?0:i==1?-width*.8f:width*.8f,i==0?2.2f:-4);a.transform.localScale=Vector3.one*(i==0?11:5)*(1+Mathf.Sin(clock*.24f+i)*.015f);a.color=new Color(.5f,.2f,1,(game.BossActive?.045f:.025f)*combatDim);}
            }
        }
    }
}
