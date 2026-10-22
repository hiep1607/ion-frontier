using UnityEngine;

namespace IonFrontier
{
    public sealed partial class IonGame
    {
        static readonly string[] UltimateNames={"PHÁO THIÊN QUANG","LÔI LIÊN HOÀN","PHÁO ĐÀI XUNG","MƯA NHIỆT HẠCH","ẢNH SONG HÀNH","XOÁY TỪ PHẢN KÍCH","PHI ĐỘI TINH VÂN","BẺ CONG KHÔNG GIAN"};
        static readonly string[] UltimateDescriptions={
            "Tự ngắm mục tiêu, bắn tia xuyên trong 1,2 giây.",
            "Ba đợt sét nối tối đa 8 mục tiêu mỗi đợt.",
            "Bảo vệ 4 giây, hồi 1 khiên và phát 3 sóng xung kích.",
            "Rải 6 bom nhiệt, để lại vùng cháy trong 3 giây.",
            "Hai phân thân bắn yểm trợ trong 4 giây.",
            "Hút đạn gần tàu rồi phóng loạt đạn phản kích.",
            "Gọi 3 tàu yểm trợ oanh kích trong 4 giây.",
            "Làm chậm địch và đạn trong vùng, rồi nổ không gian."};
        public string UltimateName=>UltimateNames[SelectedHull];
        public string UltimateDescription=>UltimateDescriptions[SelectedHull];
        public bool UltimateActive=>ultimateTime>=0;
        public float UltimateElapsed=>ultimateTime;
        public bool BossDeathActive=>State==FlightState.BossDying||State==FlightState.Paused&&pausedFrom==FlightState.BossDying;
        public float BossDeathTime=>deathTime;
        public int BossDeathEvents {get;private set;}
        public int UltimatePulseCount {get;private set;}
        public int LastUltimateHull {get;private set;}=-1;
        float ultimateTime=-1,ultimateVisualClock,deathTime;
        int ultimatePulses;
        Vector2 ultimateDirection;
        Enemy ultimateTarget;
        SpriteRenderer ultimateCore,ultimateBloom,fortressField;
        readonly SpriteRenderer[] lightning=new SpriteRenderer[32];
        void EnsureSpecialEffects()
        {
            if(ultimateCore!=null)return;
            ultimateCore=IonArt.Sprite("Celestial cannon core",IonArt.Pixel,Vector2.zero,Vector2.one,Color.white,29,transform);
            ultimateBloom=IonArt.Sprite("Celestial cannon bloom",IonArt.Glow,Vector2.zero,Vector2.one,IonArt.Cyan,28,transform);
            fortressField=IonArt.Sprite("Fortress shield",IonArt.Ring,Vector2.zero,Vector2.one,Color.green,24,transform);
            for(int i=0;i<lightning.Length;i++)lightning[i]=IonArt.Sprite("Chain lightning segment",IonArt.Pixel,Vector2.zero,Vector2.one,IonArt.Gold,29,transform);
            HideUltimate();
        }

        void StartUltimate()
        {
            EnsureSpecialEffects();ultimateTime=0;ultimateVisualClock=0;ultimatePulses=0;UltimatePulseCount=0;LastUltimateHull=SelectedHull;
            Enemy target=BossActive?boss:null;float distance=float.MaxValue;
            if(target==null)foreach(var e in enemies)if(e.active){float d=(e.position-playerPosition).sqrMagnitude;if(d<distance){target=e;distance=d;}}
            ultimateDirection=target==null?Vector2.up:(target.position-playerPosition).normalized;
            ultimateTarget=target;
            if(SelectedHull==2)HealPlayer(20);
            if(SelectedHull>=3)StartExpansionUltimate();
            bannerTimer=0;hud.ShowBanner(false,Sector,SectorName);
            Ring(playerPosition,IonCampaign.HullColors[SelectedHull],2,.5f);Play(IonArt.Dash,SelectedHull==1?1.5f:.6f);
        }

        void DrawLine(SpriteRenderer renderer,Vector2 from,Vector2 to,float width,Color color)
        {
            renderer.gameObject.SetActive(true);renderer.transform.position=(from+to)*.5f;
            var direction=to-from;renderer.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg-90);
            renderer.transform.localScale=new Vector3(width,direction.magnitude,1);renderer.color=color;
        }

        void UpdateUltimate(float dt)
        {
            if(!UltimateActive)return;
            float previous=ultimateTime;ultimateTime+=dt;
            if(SelectedHull>=3){UpdateExpansionUltimate(dt,previous);return;}
            if(SelectedHull==0)
            {
                Vector2 start=playerPosition+Vector2.up*.65f,end=start+ultimateDirection*29;
                if(ultimateTarget!=null&&ultimateTarget.active){ultimateDirection=(ultimateTarget.position-start).normalized;end=start+ultimateDirection*29;}
                bool firing=ultimateTime>=.4f;
                DrawLine(ultimateCore,start,end,firing?.38f:.035f,firing?Color.white:new Color(.3f,.9f,1,.65f));
                DrawLine(ultimateBloom,start,end,firing?1.7f:.22f,new Color(.1f,.75f,1,firing?.75f:.25f));
                Ring(playerPosition,IonArt.Cyan,.7f,.12f);
                // Integrate only the firing portion of this frame, exactly 90 damage per target.
                float slice=Mathf.Max(0,Mathf.Min(ultimateTime,1.6f)-Mathf.Max(previous,.4f));
                if(slice>0)
                {
                    foreach(var e in enemies)if(e.active&&SegmentDistance(start,end,e.position)<(e.kind>=8?2.1f:.9f))
                    {
                        DamageEnemy(e,75*slice);if(State!=FlightState.Playing)return;
                    }
                    foreach(var b in bullets)if(b.active&&!b.friendly&&SegmentDistance(start,end,b.position)<1)Release(b);
                    if(previous<.4f)Play(IonArt.Blast,.8f);
                }
                if(ultimateTime>=1.6f)EndUltimate();
            }
            else if(SelectedHull==1)
            {
                if(ultimatePulses<3&&ultimateTime>=.15f+ultimatePulses*.4f)
                {
                    HideLightning();Vector2 from=playerPosition;ulong visited=0;int segment=0;
                    for(int hit=0;hit<8;hit++)
                    {
                        Enemy next=null;float distance=float.MaxValue;
                        foreach(var e in enemies)if(e.active&&(visited&(1UL<<e.index))==0)
                        {float d=(e.position-from).sqrMagnitude;if(d<distance){next=e;distance=d;}}
                        if(next==null)break;visited|=1UL<<next.index;
                        Vector2 to=next.position,mid=(from+to)*.5f+Vector2.right*RandomRange(-.55f,.55f);
                        DrawLine(lightning[segment++],from,mid,.055f,IonArt.Gold);DrawLine(lightning[segment++],mid,to,.08f,Color.white);
                        Ring(to,IonArt.Gold,.8f,.3f);DamageEnemy(next,30);if(State!=FlightState.Playing)return;from=to;
                    }
                    ultimatePulses++;UltimatePulseCount=ultimatePulses;ultimateVisualClock=.23f;shake=Mathf.Max(shake,.09f);Play(IonArt.Impact,1.6f);
                }
                ultimateVisualClock-=dt;if(ultimateVisualClock<=0)HideLightning();
                if(ultimateTime>=1.3f)EndUltimate();
            }
            else
            {
                fortressField.gameObject.SetActive(true);fortressField.transform.position=playerPosition;
                fortressField.transform.localScale=Vector3.one*(2.8f+Mathf.Sin(ultimateTime*12)*.12f);
                fortressField.transform.rotation=Quaternion.Euler(0,0,ultimateTime*50);
                fortressField.color=new Color(.3f,1,.55f,Mathf.Clamp01(4-ultimateTime)*.75f);
                if(ultimatePulses<3&&ultimateTime>=.3f+ultimatePulses*.6f)
                {
                    ultimatePulses++;UltimatePulseCount=ultimatePulses;Ring(playerPosition,new Color(.3f,1,.5f),13,.65f);
                    foreach(var e in enemies)if(e.active&&(e.position-playerPosition).sqrMagnitude<169)
                    {DamageEnemy(e,30);if(State!=FlightState.Playing)return;}
                    ClearHostileBullets();ClearBeams();Play(IonArt.Blast,.65f);shake=Mathf.Max(shake,.12f);
                }
                if(ultimateTime>=4)EndUltimate();
            }
        }
        void HideLightning(){foreach(var line in lightning)if(line!=null)line.gameObject.SetActive(false);}
        void HideUltimate()
        {
            if(ultimateCore!=null){ultimateCore.gameObject.SetActive(false);ultimateBloom.gameObject.SetActive(false);fortressField.gameObject.SetActive(false);}HideLightning();
            HideExpansionUltimate();
        }
        void EndUltimate(){ultimateTime=-1;HideUltimate();}
        void ResetSpecialEffects()
        {
            EndUltimate();ResetWrecks();deathTime=0;BossDeathEvents=0;
            if(hud!=null)hud.ClearBlast();
        }
    }
}
