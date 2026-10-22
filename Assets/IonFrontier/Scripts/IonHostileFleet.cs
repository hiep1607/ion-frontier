using UnityEngine;

namespace IonFrontier
{
    public static class IonHostileFleet
    {
        public static readonly string[] Names={"TRINH SÁT","ĐÁNH CHẶN","ĐỘT KÍCH","OANH TẠC","XẠ THỦ","HỘ VỆ","TINH NHUỆ","DAO CẮT","LÔI NGƯ","CUNG ĐIỆN","THỢ MÁY","DỆT KHIÊN","CHỚP ẢNH","RẢI MỐC","LAO KÉO","TỔ ONG","LĂNG KÍNH","PHÁO ĐÀI","TỬ THẦN"};
        public static readonly int[] Health={2,3,3,5,3,7,5,4,7,6,9,9,5,7,7,12,8,14,10};
        static readonly int[] Roles={1,2,3,4,5,6,7,3,4,7,4,6,3,4,5,6,5,6,7};
        static readonly int[][] Groups={new[]{0,1},new[]{1,2,7},new[]{5,8,9},new[]{7,8,10},new[]{11,12,14},new[]{9,10,15},new[]{7,12,16},new[]{12,16,18},new[]{9,11,17},new[]{8,13,14},new[]{14,15,16},new[]{7,13,18},new[]{10,11,15,17,18}};
        public static int Variant(int stage,int seed){var g=Groups[Mathf.Clamp(stage-1,0,12)];return g[Mathf.Abs(seed)%g.Length];}
        public static int Role(int variant)=>Roles[Mathf.Clamp(variant,0,18)];
    }
    public sealed partial class IonGame
    {
        public ulong EncounteredVariants {get;private set;}
        public int ActiveBossCount{get{int n=0;foreach(var e in enemies)if(e.active&&e.kind>=8)n++;return n;}}
        void SetEnemyVariant(Enemy e,int variant)
        {
            e.variant=variant;e.kind=IonHostileFleet.Role(variant);e.renderer.sprite=IonArt.RegularFleet[variant];
            e.renderer.transform.localScale=Vector3.one*IonArt.ScaleFor(e.renderer.sprite,variant==17?2.1f:variant==15?1.95f:1.65f);
            EncounteredVariants|=1UL<<variant;
        }
        Vector2 ReinforcementMotion(Enemy e,Vector2 target)
        {
            float t=e.age;
            switch(e.variant)
            {
                case 7:return target+new Vector2(Mathf.Sin(t*1.8f+e.index)*2,-Mathf.Max(0,Mathf.Sin(t*.9f))*(t>5?3:0));
                case 8:return target+Vector2.right*Mathf.Sin(t*.45f)*2;
                case 9:return target+new Vector2(Mathf.Cos(t+e.index),Mathf.Sin(t+e.index))*.8f;
                case 10:return target+Vector2.up*Mathf.Sin(t*.8f)*.7f;
                case 11:return target+Vector2.down*.45f;
                case 12:return target+Vector2.right*Mathf.Sin(t*.65f+e.index)*3;
                case 14:return target+Vector2.down*Mathf.Max(0,Mathf.Sin(t*.5f))*1.8f;
                case 16:return target+Vector2.right*Mathf.Sin(t*.4f)*2.5f;
                case 18:return target+new Vector2(Mathf.Sin(t*1.2f)*2,Mathf.Cos(t*.9f)*.6f);
                default:return target;
            }
        }
        void UpdateReinforcement(Enemy e,float dt)
        {
            if(e.variant==14&&e.age%7>5&&(playerPosition-e.position).sqrMagnitude<36)
            {playerPosition=ClampPlayer(Vector2.MoveTowards(playerPosition,e.position,dt*.9f));e.halo.color=new Color(.2f,1,.65f,.3f);}
            if(e.variant==11)e.halo.color=new Color(.2f,.65f,1,.23f);
            if(e.variant==12&&e.age%6<.65f){e.renderer.color=new Color(.7f,.5f,1,.3f);if(e.age>2)Ring(e.position,IonArt.Cyan,1.1f,.16f);}
        }
        void ReinforcementAttack(Enemy e,Vector2 p,Vector2 aim)
        {
            switch(e.variant)
            {
                case 7:Fan(p,aim,2,16,6);break;
                case 8:for(int i=-1;i<=1;i+=2){var b=Bolt(p+Vector2.right*i*.4f,Vector2.down*2.2f,false,.18f);if(b!=null){b.acceleration=.7f;b.damage=24;b.renderer.sprite=IonArt.BoltIcons[2,0];b.renderer.transform.localScale=new Vector3(.25f,.6f,1);}}break;
                case 9:for(int i=0;i<8;i++){float a=i*Mathf.PI/4+e.age*.4f;Bolt(p,new Vector2(Mathf.Sin(a),Mathf.Cos(a))*3.2f,false,.12f);}break;
                case 10:Fan(p,aim,3,16,3.3f);break;
                case 11:Fan(p,Vector2.down,3,24,3.1f,.2f);break;
                case 12:Ring(e.position,new Color(.7f,.4f,1),2,.6f);Fan(p,aim,4,10,5.2f);break;
                case 13:SpawnWorld(WorldKind.Mine,p,3,.45f,Vector2.down*1.2f);Fan(p,aim,1,0,4);break;
                case 14:Fan(p,aim,2,8,4.6f,.2f);Ring(e.position,new Color(.2f,1,.7f),5,.7f);break;
                case 15:
                    if(ActiveEnemies<18)for(int i=0;i<2;i++){int slot=FreeEnemy();if(slot<0)break;var child=SpawnEnemy(slot,1,e.position+new Vector2(i==0?-1:1,-1),3);SetEnemyVariant(child,7);child.entry=child.position=e.position;child.age=.8f;}
                    Fan(p,aim,3,20,3.6f);break;
                case 16:SpawnBeam(new Vector2(e.position.x,e.position.y-.5f),e);Ring(e.position,new Color(1,.3f,.9f),1.5f,.8f);break;
                case 17:Fan(p,Vector2.down,7,13,3.4f,.19f);break;
                case 18:Fan(p,aim,5,12,4.5f);Ring(p,new Color(.6f,.3f,1),3,.6f);break;
            }
        }
    }
}
