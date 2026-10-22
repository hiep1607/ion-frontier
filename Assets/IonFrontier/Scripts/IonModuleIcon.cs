using UnityEngine;
using UnityEngine.UI;
namespace IonFrontier
{
    // Nine code-native vector symbols, consistent with the existing HUD star graphic.
    public sealed class IonModuleIcon : Graphic
    {
        int module=-1;
        public void SetModule(int value){if(module==value)return;module=value;SetVerticesDirty();}
        Vector2 Point(Vector2 p)=>rectTransform.rect.center+p*Mathf.Min(rectTransform.rect.width,rectTransform.rect.height);
        void Line(VertexHelper vh,Vector2 a,Vector2 b,float width=.055f)
        {
            Vector2 d=(b-a).normalized,n=new Vector2(-d.y,d.x)*width*.5f;int k=vh.currentVertCount;
            vh.AddVert(Point(a+n),color,Vector2.zero);vh.AddVert(Point(b+n),color,Vector2.zero);vh.AddVert(Point(b-n),color,Vector2.zero);vh.AddVert(Point(a-n),color,Vector2.zero);
            vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
        }
        void Ring(VertexHelper vh,float radius,int count=28,float start=0,float sweep=360)
        {for(int i=0;i<count;i++){float a=(start+sweep*i/count)*Mathf.Deg2Rad,b=(start+sweep*(i+1)/count)*Mathf.Deg2Rad;Line(vh,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,.045f);}}
        void Diamond(VertexHelper vh,Vector2 p,float r){Line(vh,p+Vector2.up*r,p+Vector2.right*r);Line(vh,p+Vector2.right*r,p+Vector2.down*r);Line(vh,p+Vector2.down*r,p+Vector2.left*r);Line(vh,p+Vector2.left*r,p+Vector2.up*r);}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(module<0)return;
            switch((CombatModule)module)
            {
                case CombatModule.FocusedSpread:
                    for(int i=-1;i<=1;i++)Line(vh,new Vector2(i*.3f,-.36f),new Vector2(i*.07f,.28f));Line(vh,new Vector2(-.13f,.17f),new Vector2(0,.4f));Line(vh,new Vector2(0,.4f),new Vector2(.13f,.17f));break;
                case CombatModule.PiercingFragments:
                    Diamond(vh,Vector2.zero,.17f);for(int i=0;i<4;i++){float a=(45+90*i)*Mathf.Deg2Rad;Diamond(vh,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.35f,.08f);}break;
                case CombatModule.SeekingArc:
                    Line(vh,new Vector2(.12f,.4f),new Vector2(-.2f,.02f));Line(vh,new Vector2(-.2f,.02f),new Vector2(.16f,.05f));Line(vh,new Vector2(.16f,.05f),new Vector2(-.12f,-.4f));Line(vh,new Vector2(.05f,-.12f),new Vector2(.35f,-.15f));break;
                case CombatModule.DashCapacitor:
                    for(int i=0;i<2;i++){float x=i*.28f-.2f;Line(vh,new Vector2(x-.12f,-.3f),new Vector2(x+.12f,0));Line(vh,new Vector2(x+.12f,0),new Vector2(x-.12f,.3f));}Line(vh,new Vector2(-.43f,-.14f),new Vector2(-.28f,-.14f));break;
                case CombatModule.PulseChamber:
                    Ring(vh,.35f);Line(vh,new Vector2(0,-.2f),new Vector2(0,.2f),.09f);Line(vh,new Vector2(-.2f,0),new Vector2(.2f,0),.09f);break;
                case CombatModule.AftershockRounds:
                    Ring(vh,.12f,16);Ring(vh,.3f,24);for(int i=0;i<4;i++){Vector2 d=new Vector2(Mathf.Cos(i*Mathf.PI/2),Mathf.Sin(i*Mathf.PI/2));Line(vh,d*.35f,d*.46f);}break;
                case CombatModule.ReactiveArmor:
                    Vector2[] p={new Vector2(-.32f,.3f),new Vector2(.32f,.3f),new Vector2(.27f,-.14f),new Vector2(0,-.4f),new Vector2(-.27f,-.14f)};for(int i=0;i<p.Length;i++)Line(vh,p[i],p[(i+1)%p.Length]);Line(vh,new Vector2(-.12f,0),new Vector2(.12f,0));Line(vh,new Vector2(0,-.12f),new Vector2(0,.12f));break;
                case CombatModule.SalvageMagnet:
                    Line(vh,new Vector2(-.3f,.33f),new Vector2(-.3f,-.2f),.1f);Line(vh,new Vector2(-.3f,-.2f),new Vector2(0,-.35f),.1f);Line(vh,new Vector2(0,-.35f),new Vector2(.3f,-.2f),.1f);Line(vh,new Vector2(.3f,-.2f),new Vector2(.3f,.33f),.1f);Line(vh,new Vector2(-.4f,.12f),new Vector2(-.2f,.12f));Line(vh,new Vector2(.2f,.12f),new Vector2(.4f,.12f));break;
                case CombatModule.AmmoReclaimer:
                    Ring(vh,.4f,24,35,285);Line(vh,new Vector2(.34f,.25f),new Vector2(.12f,.27f));Line(vh,new Vector2(.34f,.25f),new Vector2(.32f,.04f));for(int i=-1;i<=1;i++)Line(vh,new Vector2(i*.14f,-.17f),new Vector2(i*.14f,.14f),.065f);break;
            }
        }
    }
}
