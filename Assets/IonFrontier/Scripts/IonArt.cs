using UnityEngine;

namespace IonFrontier
{
    public static class IonArt
    {
        public static readonly Color Cyan = new Color(.22f, .86f, 1f);
        public static readonly Color Orange = new Color(1f, .36f, .16f);
        public static readonly Color Gold = new Color(1f, .76f, .34f);
        public static readonly Color Ink = new Color(.025f, .045f, .085f);
        public static Sprite Pixel, Glow, Disc, Ring, Background;
        public static Sprite[] Ships;
        public static AudioClip Laser, Impact, Blast, Pickup, Dash, Music;

        public static void Initialize()
        {
            if (Pixel != null) return;
            Pixel = MakeTexture(2, (x, y) => Color.white);
            Glow = MakeTexture(64, (x, y) => new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - new Vector2(x,y).magnitude), 2.5f)));
            Disc = MakeTexture(48, (x, y) => new Color(1, 1, 1, Mathf.Clamp01((1 - new Vector2(x,y).magnitude) * 18)));
            Ring = MakeTexture(96, (x, y) => new Color(1, 1, 1, Mathf.Clamp01((.065f - Mathf.Abs(new Vector2(x,y).magnitude - .86f)) * 35)));
            Background = MakeTexture(256, (x, y) =>
            {
                float a = Mathf.Exp(-((x+.65f)*(x+.65f)*2.3f+(y-.45f)*(y-.45f)*5));
                float b = Mathf.Exp(-((x-.75f)*(x-.75f)*3+(y+.5f)*(y+.5f)*3));
                return new Color(.014f+.027f*a+.012f*b, .023f+.055f*a+.018f*b, .05f+.09f*a+.055f*b);
            });
            var atlas = Resources.Load<Texture2D>("ion-fleet");
            Ships = new Sprite[4];
            if (atlas != null)
            {
                float w=atlas.width, h=atlas.height;
                // Atlas regions follow the actual transparent gutters; no spacecraft tips are clipped.
                Ships[0] = UnityEngine.Sprite.Create(atlas, new Rect(0,h*.55f,w*.5f,h*.45f), Vector2.one*.5f,w*.5f,0,SpriteMeshType.FullRect);
                Ships[1] = UnityEngine.Sprite.Create(atlas, new Rect(w*.5f,h*.55f,w*.5f,h*.45f), Vector2.one*.5f,w*.5f,0,SpriteMeshType.FullRect);
                Ships[2] = UnityEngine.Sprite.Create(atlas, new Rect(0,0,w*.48f,h*.55f), Vector2.one*.5f,w*.48f,0,SpriteMeshType.FullRect);
                Ships[3] = UnityEngine.Sprite.Create(atlas, new Rect(w*.5f,0,w*.5f,h*.55f), Vector2.one*.5f,w*.5f,0,SpriteMeshType.FullRect);
            }
            else for(int i=0;i<4;i++) Ships[i]=Disc;
            Laser = Tone(.075f, 1100, 270, .15f, 0);
            Impact = Tone(.08f, 220, 70, .13f, .45f);
            Blast = Tone(.38f, 120, 30, .3f, .8f);
            Pickup = Tone(.25f, 500, 1450, .2f, 0);
            Dash = Tone(.21f, 480, 100, .16f, .25f);
            Music = Ambient();
        }


        static Sprite MakeTexture(int size, System.Func<float,float,Color> color)
        {
            var tex = new Texture2D(size,size,TextureFormat.RGBA32,false);
            var pixels = new Color[size*size];
            for(int y=0;y<size;y++) for(int x=0;x<size;x++) pixels[y*size+x] = color(2f*x/(size-1)-1,2f*y/(size-1)-1);
            tex.SetPixels(pixels); tex.Apply(); tex.wrapMode=TextureWrapMode.Clamp;
            return UnityEngine.Sprite.Create(tex,new Rect(0,0,size,size),Vector2.one*.5f,size);
        }

        public static SpriteRenderer Sprite(string name, Sprite sprite, Vector2 position, Vector2 size, Color tint, int layer, Transform parent=null)
        {
            var go=new GameObject(name); if(parent!=null) go.transform.SetParent(parent,false);
            var sr=go.AddComponent<SpriteRenderer>(); sr.sprite=sprite; sr.color=tint; sr.sortingOrder=layer;
            sr.transform.position=position; sr.transform.localScale=new Vector3(size.x,size.y,1); return sr;
        }

        static AudioClip Tone(float duration, float start, float end, float volume, float noise)
        {
            const int rate=22050; var samples=new float[Mathf.CeilToInt(rate*duration)]; float phase=0;
            var rng=new System.Random(27);
            for(int i=0;i<samples.Length;i++)
            {
                float t=(float)i/samples.Length, env=Mathf.Min(1,t*20)*Mathf.Pow(1-t,2);
                phase+=Mathf.Lerp(start,end,t)/rate*Mathf.PI*2;
                samples[i]=(Mathf.Sin(phase)*(1-noise)+((float)rng.NextDouble()*2-1)*noise)*env*volume;
            }
            var clip=AudioClip.Create("Ion synth",samples.Length,1,rate,false); clip.SetData(samples,0); return clip;
        }

        static AudioClip Ambient()
        {
            const int rate=22050, seconds=16; var samples=new float[rate*seconds];
            float[] notes={65.406f,77.782f,58.270f,51.913f};
            for(int i=0;i<samples.Length;i++)
            {
                float t=(float)i/rate, beat=t*2; float bass=notes[(int)(t/4)%4];
                float pulse=Mathf.Exp(-((beat%1)*14));
                float fade=Mathf.Min(1,Mathf.Min(t,seconds-t)*2);
                samples[i]=fade*(Mathf.Sin(t*bass*Mathf.PI*2)*.018f + Mathf.Sin(t*bass*2*Mathf.PI*2)*.009f + Mathf.Sin((beat%1)*Mathf.PI*2*38)*pulse*.021f);
            }
            var clip=AudioClip.Create("Orbital ambient",samples.Length,1,rate,false); clip.SetData(samples,0); return clip;
        }
    }
}


