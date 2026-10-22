using UnityEngine;
using UnityEngine.UI;

namespace IonFrontier
{
    // Vector UI star: stays sharp at every Canvas resolution without a bitmap asset.
    public sealed class IonRatingStar : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var rect=rectTransform.rect;Vector2 center=rect.center;float radius=Mathf.Min(rect.width,rect.height)*.5f;
            vh.AddVert(center,color,Vector2.one*.5f);
            for(int i=0;i<10;i++)
            {
                float angle=(90+i*36)*Mathf.Deg2Rad;Vector2 p=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius*(i%2==0?1:.44f);
                vh.AddVert(p,color,Vector2.zero);
            }
            for(int i=0;i<10;i++)vh.AddTriangle(0,1+i,1+(i+1)%10);
        }
    }
}
