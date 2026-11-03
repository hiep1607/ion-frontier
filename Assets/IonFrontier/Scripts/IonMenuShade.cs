using UnityEngine;
using UnityEngine.UI;

namespace IonFrontier
{
    // UI-native gradient: no texture generation and no source artwork edits.
    public sealed class IonMenuShade : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();var r=rectTransform.rect;float[] stops={0,.34f,.68f,1};float[] alphas={.94f,.87f,.35f,.22f};
            for(int i=0;i<4;i++)
            {
                float x=Mathf.Lerp(r.xMin,r.xMax,stops[i]);var tint=new Color(.015f,.035f,.06f,alphas[i]);
                mesh.AddVert(new Vector3(x,r.yMin),tint,Vector2.zero);mesh.AddVert(new Vector3(x,r.yMax),tint,Vector2.one);
                if(i>0){int j=i*2;mesh.AddTriangle(j-2,j-1,j);mesh.AddTriangle(j,j-1,j+1);}
            }
        }
    }
}
