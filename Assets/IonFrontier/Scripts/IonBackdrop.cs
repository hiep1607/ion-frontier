using UnityEngine;

namespace IonFrontier
{
    public sealed class IonBackdrop : MonoBehaviour
    {
        struct Star { public Transform transform; public float speed, phase, size; public SpriteRenderer renderer; }
        Star[] stars; float width;
        public void Initialize(float halfWidth)
        {
            width=halfWidth;
            IonArt.Sprite("Deep space",IonArt.Background,Vector2.zero,new Vector2(halfWidth*2+2,16),Color.white,-100,transform);
            var rng=new System.Random(1024); stars=new Star[145];
            for(int i=0;i<stars.Length;i++)
            {
                float depth=(float)rng.NextDouble(); float size=Mathf.Lerp(.025f,.085f,depth);
                var sr=IonArt.Sprite("Star",IonArt.Disc,new Vector2((float)rng.NextDouble()*width*2-width,(float)rng.NextDouble()*15-7.5f),Vector2.one*size,new Color(.55f,.75f,1,.3f+depth*.5f),-90,transform);
                stars[i]=new Star {transform=sr.transform,renderer=sr,speed=.16f+depth*.65f,phase=(float)rng.NextDouble()*6.28f,size=size};
            }
            var planet=IonArt.Sprite("Distant planet",IonArt.Disc,new Vector2(width-1.1f,4.8f),new Vector2(5.4f,5.4f),new Color(.06f,.14f,.22f,.6f),-95,transform);
            IonArt.Sprite("Planet atmosphere",IonArt.Ring,planet.transform.position,Vector2.one*5.7f,new Color(.15f,.5f,.7f,.08f),-94,transform);
        }
        void Update()
        {
            if(stars==null) return;
            float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);
            for(int i=0;i<stars.Length;i++)
            {
                var s=stars[i]; var p=s.transform.position; p.y-=s.speed*dt;
                if(p.y<-7.6f) p.y=7.6f; s.transform.position=p;
                s.renderer.color=new Color(.58f,.79f,1,.42f+Mathf.Sin(Time.unscaledTime*.8f+s.phase)*.16f);
            }
        }
    }
}
