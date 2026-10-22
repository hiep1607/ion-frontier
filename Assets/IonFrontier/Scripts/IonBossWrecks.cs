using System.Collections.Generic;
using UnityEngine;

namespace IonFrontier
{
    public sealed partial class IonGame
    {
        sealed class BossWreck
        {
            public bool active;public Enemy target;public Vector2 origin;public float time,burst;public int events;
            public readonly SpriteRenderer[] pieces=new SpriteRenderer[6];
            public readonly Vector2[] offsets=new Vector2[6],velocities=new Vector2[6];
        }
        readonly BossWreck[] wrecks=new BossWreck[4];
        readonly Dictionary<Sprite,Sprite[]> wreckSlices=new Dictionary<Sprite,Sprite[]>();
        public int PendingBossWrecks{get{int n=0;foreach(var w in wrecks)if(w!=null&&w.active)n++;return n;}}
        public int BossDestructionsStarted {get;private set;}
        public int BossDestructionsCompleted {get;private set;}
        Enemy lastWreckTarget;
        void BeginBossDeath(Enemy target)
        {
            if(State!=FlightState.Playing||runResolved)return;
            target.dying=true;target.engine.gameObject.SetActive(false);target.halo.gameObject.SetActive(false);
            int slot=-1;for(int i=0;i<wrecks.Length;i++)if(wrecks[i]==null||!wrecks[i].active){slot=i;break;}
            if(slot<0)throw new System.InvalidOperationException("Boss wreck pool exhausted");
            if(wrecks[slot]==null)
            {
                var created=new BossWreck();for(int i=0;i<6;i++){created.pieces[i]=IonArt.Sprite("Boss hull fragment",IonArt.Disc,Vector2.zero,Vector2.one,Color.white,12,transform);created.pieces[i].gameObject.SetActive(false);}wrecks[slot]=created;
            }
            var w=wrecks[slot];w.active=true;w.target=target;w.origin=target.position;w.time=w.burst=0;w.events=1;
            Sprite sprite=target.renderer.sprite;
            if(!wreckSlices.TryGetValue(sprite,out var slices))
            {
                slices=new Sprite[6];var r=sprite.rect;
                for(int i=0;i<6;i++)slices[i]=UnityEngine.Sprite.Create(sprite.texture,new Rect(r.x+i%3*r.width/3,r.y+i/3*r.height/2,r.width/3,r.height/2),Vector2.one*.5f,sprite.pixelsPerUnit);
                wreckSlices.Add(sprite,slices);
            }
            for(int i=0;i<6;i++)
            {
                w.pieces[i].sprite=slices[i];w.offsets[i]=new Vector2((i%3-1)*sprite.rect.width/3,(i/3-.5f)*sprite.rect.height/2)/sprite.pixelsPerUnit;
                w.pieces[i].gameObject.SetActive(false);
            }
            BossDestructionsStarted++;lastWreckTarget=target;deathTime=0;BossDeathEvents=1;
            boss=null;foreach(var e in enemies)if(e.active&&e.kind>=8){boss=e;break;}
            if(boss==null)
            {
                EndUltimate();State=FlightState.BossDying;foreach(var b in bullets)Release(b);ClearBeams();for(int i=0;i<cells.Length;i++)ReleaseCell(i);HideBossOrnaments();
                hud.ShowBanner(false,Sector,SectorName);hud.Refresh();hud.Toast(BossName+" / LÕI PHẢN ỨNG SỤP ĐỔ");
            }
            else {ClearHostileBullets();hud.Toast("MỘT BOSS ĐÃ GỤC / ĐỘI HÌNH CÒN LẠI PHẢN CÔNG");}
            Play(IonArt.Blast,.55f);
        }
        void UpdateBossDeath(float dt)
        {
            foreach(var w in wrecks)
            {
                if(w==null||!w.active)continue;w.time+=dt;w.burst-=dt;var e=w.target;float scale=e.renderer.transform.localScale.x;
                if(w.time<2.2f)
                {
                    float t=w.time/2.2f;e.position=w.origin+new Vector2(Mathf.Sin(w.time*37)*.04f,-t*t*.9f);
                    e.renderer.transform.position=e.position;e.renderer.transform.rotation=Quaternion.Euler(0,0,-t*24+Mathf.Sin(w.time*29)*1.2f);
                    e.renderer.color=Color.Lerp(Color.white,new Color(.4f,.11f,.07f),t*.8f);
                    if(w.burst<=0)
                    {
                        w.burst=Mathf.Lerp(.3f,.1f,t);Vector2 p=e.position+new Vector2(RandomRange(-1.5f,1.5f),RandomRange(-1.2f,1.2f));
                        Explosion(p,.5f+t*.4f);Emit(p,Vector2.up*.6f,new Color(.2f,.18f,.18f),1.1f,1.1f);Play(IonArt.Blast,RandomRange(.65f,.95f));
                    }
                    shake=Mathf.Max(shake,.035f+t*.07f);
                }
                else
                {
                    if((w.events&2)==0)
                    {
                        w.events|=2;e.renderer.gameObject.SetActive(false);
                        for(int i=0;i<6;i++)
                        {
                            var p=w.pieces[i];p.gameObject.SetActive(true);p.transform.localScale=Vector3.one*scale;
                            p.transform.position=e.position+(Vector2)(e.renderer.transform.rotation*(Vector3)(w.offsets[i]*scale));p.transform.rotation=e.renderer.transform.rotation;
                            w.velocities[i]=new Vector2((i%3-1)*2+RandomRange(-.4f,.4f),(i/3-.5f)*1.5f-1);
                        }
                        Explosion(e.position,2.2f);if(State==FlightState.BossDying)hud.FlashBlast(.45f);Play(IonArt.Blast,.45f);
                    }
                    for(int i=0;i<6;i++)
                    {
                        var p=w.pieces[i];w.velocities[i]+=Vector2.down*dt*1.2f;p.transform.position+=(Vector3)(w.velocities[i]*dt);p.transform.Rotate(0,0,(i%2==0?1:-1)*(24+i*8)*dt);
                        p.color=new Color(.7f,.45f,.35f,Mathf.Clamp01((4.4f-w.time)/.9f));if(w.burst<=0)Emit(p.transform.position,Vector2.up*.8f,IonArt.Orange,.5f,.6f);
                    }
                    if(w.burst<=0)w.burst=.1f;
                    if(w.time>=3.4f&&(w.events&4)==0)
                    {
                        w.events|=4;Explosion(e.position,State==FlightState.BossDying?4.5f:2.8f);Ring(e.position,Color.white,8,1);Ring(e.position,IonArt.Orange,12,1.1f);
                        if(State==FlightState.BossDying)hud.FlashBlast(.85f);Play(IonArt.Blast,.28f);shake=Mathf.Max(shake,State==FlightState.BossDying?.3f:.12f);
                    }
                }
                if(w.time>=4.8f){w.events|=8;w.active=false;e.dying=false;foreach(var p in w.pieces)p.gameObject.SetActive(false);BossDestructionsCompleted++;}
                if(e==lastWreckTarget){deathTime=w.time;BossDeathEvents=w.events;}
            }
            if(State==FlightState.BossDying)
            {
                shake=Mathf.MoveTowards(shake,0,dt*.25f);view.transform.position=new Vector3(Mathf.Sin(deathTime*83)*shake,Mathf.Sin(deathTime*67)*shake*.65f,-10);
                if(PendingBossWrecks==0){boss=null;view.transform.position=new Vector3(0,0,-10);Finish(true);}
            }
        }
        void ResetWrecks()
        {
            foreach(var w in wrecks)if(w!=null){w.active=false;if(w.target!=null)w.target.dying=false;foreach(var p in w.pieces)p.gameObject.SetActive(false);}
            lastWreckTarget=null;BossDestructionsStarted=BossDestructionsCompleted=0;
        }
    }
}
