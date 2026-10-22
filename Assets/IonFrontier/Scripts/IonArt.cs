using UnityEngine;

namespace IonFrontier
{
    public static class IonArt
    {
        public static readonly Color Cyan = new Color(.22f, .86f, 1f);
        public static readonly Color Orange = new Color(1f, .36f, .16f);
        public static readonly Color Gold = new Color(1f, .76f, .34f);
        public static readonly Color Ink = new Color(.025f, .045f, .085f);
        public static Sprite Pixel, Glow, Disc, Ring, Background,Rock,StationNode,BeaconNode,MineNode;
        public static Sprite[] Ships,PlayerShips,EnemyShips,GiftIcons,RegularFleet,BossFleet,MissionProps,StageBackdrops;
        public static Sprite[,] BoltIcons;
        public static Sprite HostileBolt;
        public static AudioClip Laser, Impact, Blast, Pickup, Dash, Music;

        public static void Initialize()
        {
            if (Pixel != null) return;
            Pixel = MakeTexture(2, (x, y) => Color.white);
            Glow = MakeTexture(64, (x, y) => new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - new Vector2(x,y).magnitude), 2.5f)));
            Disc = MakeTexture(48, (x, y) => new Color(1, 1, 1, Mathf.Clamp01((1 - new Vector2(x,y).magnitude) * 18)));
            Ring = MakeTexture(96, (x, y) => new Color(1, 1, 1, Mathf.Clamp01((.065f - Mathf.Abs(new Vector2(x,y).magnitude - .86f)) * 35)));
            Rock=MakeTexture(128,(x,y)=>{float a=Mathf.Atan2(y,x),r=Mathf.Sqrt(x*x+y*y),edge=.78f+.09f*Mathf.Sin(a*5)+.06f*Mathf.Sin(a*9);float shade=.65f+.2f*x-.2f*y+.12f*Mathf.Sin(x*25+y*17);return new Color(shade,shade*.9f,shade*.8f,r<edge?1:0);});
            StationNode=MakeTexture(128,(x,y)=>{float ax=Mathf.Abs(x),ay=Mathf.Abs(y),r=Mathf.Sqrt(x*x+y*y);bool mask=ax<.5f&&ay<.5f&&(ax>.3f||ay>.3f)||r<.22f||ax>.55f&&ax<.85f&&ay<.7f||ay>.55f&&ay<.85f&&ax<.18f;return new Color(1,1,1,mask?1:0);});
            BeaconNode=MakeTexture(128,(x,y)=>{float r=Mathf.Sqrt(x*x+y*y);bool mask=Mathf.Abs(r-.6f)<.07f||Mathf.Abs(x)<.15f&&y<.6f&&y>-.8f||y>.1f&&y<.45f&&x>.1f&&x<.65f;return new Color(1,1,1,mask?1:0);});
            MineNode=MakeTexture(128,(x,y)=>{float r=Mathf.Sqrt(x*x+y*y),a=Mathf.Atan2(y,x);bool mask=r<.48f||r<.9f&&Mathf.Abs(Mathf.Sin(a*4))<.15f;return new Color(1,1,1,mask?1:0);});
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
            var library=Resources.Load<IonFleetLibrary>("FleetLibrary");
            if(library==null||library.playerShips.Length!=8||library.enemyShips.Length!=10)throw new System.InvalidOperationException("FleetLibrary must contain 8 player sprites and 10 enemies.");
            PlayerShips=library.playerShips;EnemyShips=library.enemyShips;
            StageBackdrops=library.stageBackdrops;if(StageBackdrops==null||StageBackdrops.Length!=13)throw new System.InvalidOperationException("Missing thirteen sector backgrounds");foreach(var background in StageBackdrops)if(background==null)throw new System.InvalidOperationException("Missing sector background sprite");
            MissionProps=library.missionProps;if(MissionProps==null||MissionProps.Length!=7)throw new System.InvalidOperationException("Missing seven finished mission props");foreach(var prop in MissionProps)if(prop==null)throw new System.InvalidOperationException("Missing mission prop sprite");
            if(library.reinforcements==null||library.reinforcements.Length!=12||library.bossFleet==null||library.bossFleet.Length!=16)throw new System.InvalidOperationException("Missing hostile overhaul fleet: 12 reinforcements / 16 bosses.");
            RegularFleet=new Sprite[19];for(int i=0;i<7;i++)RegularFleet[i]=EnemyShips[i];
            for(int i=0;i<12;i++)RegularFleet[i+7]=library.reinforcements[i];BossFleet=library.bossFleet;
            foreach(var sprite in RegularFleet)if(sprite==null)throw new System.InvalidOperationException("Missing regular enemy sprite.");
            foreach(var sprite in BossFleet)if(sprite==null)throw new System.InvalidOperationException("Missing unique boss sprite.");
            foreach(var sprite in PlayerShips)if(sprite==null)throw new System.InvalidOperationException("Missing player sprite.");
            foreach(var sprite in EnemyShips)if(sprite==null)throw new System.InvalidOperationException("Missing hostile sprite.");
            GiftIcons=new Sprite[IonCampaign.GiftNames.Length];
            for(int i=0;i<GiftIcons.Length;i++){int icon=i;GiftIcons[i]=MakeTexture(96,(x,y)=>new Color(1,1,1,GiftMask(icon,x,y)?1:0));}
            BoltIcons=new Sprite[4,8];
            for(int g=0;g<4;g++)for(int h=0;h<8;h++){int gun=g,hull=h;BoltIcons[g,h]=MakeTexture(64,(x,y)=>new Color(1,1,1,BoltMask(gun,hull%3,x,y)?1:0));}
            HostileBolt=MakeTexture(48,(x,y)=>new Color(1,1,1,Mathf.Clamp01((1-new Vector2(x,y).magnitude)*12)));
            Laser = Tone(.075f, 1100, 270, .15f, 0);
            Impact = Tone(.08f, 220, 70, .13f, .45f);
            Blast = Tone(.38f, 120, 30, .3f, .8f);
            Pickup = Tone(.25f, 500, 1450, .2f, 0);
            Dash = Tone(.21f, 480, 100, .16f, .25f);
            Music = Ambient();
        }


        public static float ScaleFor(Sprite sprite,float size)=>size/Mathf.Max(sprite.bounds.size.x,sprite.bounds.size.y);
        static bool Stroke(float x,float y,float ax,float ay,float bx,float by,float width)
        {
            var a=new Vector2(ax,ay);var d=new Vector2(bx-ax,by-ay);float t=Mathf.Clamp01(Vector2.Dot(new Vector2(x,y)-a,d)/d.sqrMagnitude);
            return (new Vector2(x,y)-a-d*t).sqrMagnitude<width*width;
        }
        static bool GiftMask(int kind,float x,float y)
        {
            float ax=Mathf.Abs(x),ay=Mathf.Abs(y),r=Mathf.Sqrt(x*x+y*y);
            switch(kind)
            {
                case 0:return (Mathf.Abs(x+.53f)<.15f||ax<.15f||Mathf.Abs(x-.53f)<.15f)&&y>-.72f&&y<.5f+(.18f-ax% .53f);
                case 1:return y<.8f&&y>-.85f&&ax<.73f&&ax<.85f+y*.5f&&(ax>.45f||y>.52f||y<-.5f||ax<.12f||ay<.12f);
                case 2:return Stroke(x,y,-.65f,-.35f,0,.3f,.12f)||Stroke(x,y,0,.3f,.65f,-.35f,.12f)||Stroke(x,y,-.65f,.15f,0,.8f,.12f)||Stroke(x,y,0,.8f,.65f,.15f,.12f);
                case 3:return Stroke(x,y,0,-.7f,-.7f,.55f,.12f)||Stroke(x,y,0,-.7f,0,.8f,.12f)||Stroke(x,y,0,-.7f,.7f,.55f,.12f);
                case 4:return ax<.24f*(1-ay)&&ay<.95f||Stroke(x,y,-.55f,-.4f,-.55f,.4f,.07f)||Stroke(x,y,.55f,-.4f,.55f,.4f,.07f);
                case 5:return Mathf.Abs(r-.55f)<.09f||ax<.08f&&ay<.9f||ay<.08f&&ax<.9f;
                case 6:return r<.3f||Mathf.Abs(x*y)<.06f&&r<.9f||Mathf.Abs(ax-ay)<.1f&&r<.8f;
                case 7:return Stroke(x,y,-.65f,-.65f,.65f,.65f,.12f)||Stroke(x,y,-.65f,.65f,.65f,-.65f,.12f)||r<.22f;
                case 8:return Stroke(x,y,.35f,.85f,-.4f,.05f,.14f)||Stroke(x,y,-.4f,.05f,.35f,.05f,.14f)||Stroke(x,y,.35f,.05f,-.35f,-.85f,.14f);
                default:return new Vector2(x,y-.55f).magnitude<.19f||new Vector2(x+.55f,y+.4f).magnitude<.19f||new Vector2(x-.55f,y+.4f).magnitude<.19f||Stroke(x,y,0,.25f,-.4f,-.1f,.07f)||Stroke(x,y,0,.25f,.4f,-.1f,.07f);
            }
        }
        static bool BoltMask(int gun,int hull,float x,float y)
        {
            float ax=Mathf.Abs(x),ay=Mathf.Abs(y);
            if(gun==2)return ax<(.52f*(1-ay))||ax>.65f&&ax<.8f&&ay<.55f;
            if(gun==3)return ax<.45f&&y>-.6f&&y<.8f-ax*.5f||y<-.3f&&y>-.85f&&ax<.8f&&ax>.42f;
            if(gun==1)return ax<.7f*(1-ay)&&ay<.85f;
            return hull==0?ax<.45f&&ay<.9f:hull==1?Mathf.Abs(ax-.45f)<.18f&&ay<.9f:ax<.65f&&ay<.8f&&(ax>.4f||ay>.6f||ax<.2f);
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


