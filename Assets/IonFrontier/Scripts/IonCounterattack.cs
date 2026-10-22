using UnityEngine;

namespace IonFrontier
{
    public sealed partial class IonGame
    {
        Enemy counterElite;int counterSerial;
        readonly float[] counterEngineHealth={18,18},counterFire={1.3f,2.4f};
        readonly Vector2[] counterAim=new Vector2[2];
        readonly bool[] counterWarning=new bool[2];
        readonly SpriteRenderer[] counterEngines=new SpriteRenderer[2],counterRings=new SpriteRenderer[2],counterLines=new SpriteRenderer[2],counterPieces=new SpriteRenderer[6];
        readonly Vector2[] counterPieceVelocity=new Vector2[6];
        Vector2 counterEntry,counterWreckOrigin;float counterWreckTime,counterBurst,counterTargetAge;
        int counterTargetPart=-2,counterWreckEvents;bool counterStarted,counterSucceeded,counterExpired,counterWreck;
        public bool CounterActive=>CounterOwns(counterElite)&&counterElite.active&&escapeTravel>=2400&&escapeTravel<3200;
        public bool CounterSucceeded=>counterSucceeded;
        public bool CounterExpired=>counterExpired;
        public bool CounterWreckActive=>counterWreck;
        public float CounterLeftHealth=>counterEngineHealth[0];
        public float CounterRightHealth=>counterEngineHealth[1];
        public float CounterBodyHealth=>CounterOwns(counterElite)?Mathf.Max(0,counterElite.health):0;
        public int CounterEnginesAlive=>(counterEngineHealth[0]>0?1:0)+(counterEngineHealth[1]>0?1:0);
        public int CounterWarningsAlive=>(counterWarning[0]?1:0)+(counterWarning[1]?1:0);
        public bool CounterTargetActive=>counterTargetPart!=-2&&counterTargetAge<8&&CounterOwns(counterElite);
        public string CounterTargetName=>counterTargetPart==0?"ĐỘNG CƠ TRÁI":counterTargetPart==1?"ĐỘNG CƠ PHẢI":"TRUY KÍCH HẠNG NẶNG";
        public Sprite CounterTargetSprite=>counterTargetPart>=0?IonArt.MissionProps[2]:CounterOwns(counterElite)?counterElite.renderer.sprite:null;
        public float CounterTargetHealth=>counterTargetPart>=0?counterEngineHealth[counterTargetPart]:CounterBodyHealth;
        public float CounterTargetMaxHealth=>counterTargetPart>=0?18:42;
        public string CounterTargetState=>counterSucceeded?"ĐÃ HẠ / ĐƯỜNG THOÁT ĐÃ MỞ":counterTargetPart>=0?(counterEngineHealth[counterTargetPart]>0?"ĐIỂM YẾU / PHÁ ĐỂ TẮT CỤM PHÁO":"ĐÃ PHÁ / CỤM PHÁO NGỪNG HOẠT ĐỘNG"):CounterEnginesAlive>0?"GIÁP ĐÓNG / PHÁ HAI ĐỘNG CƠ":"LÕI ĐÃ LỘ / CÓ THỂ TẤN CÔNG";
        public string CounterEngineText=>"TRÁI "+(counterEngineHealth[0]>0?Mathf.CeilToInt(counterEngineHealth[0])+"/18":"ĐÃ PHÁ")+"  •  PHẢI "+(counterEngineHealth[1]>0?Mathf.CeilToInt(counterEngineHealth[1])+"/18":"ĐÃ PHÁ");
        public string CounterPhaseText=>counterSucceeded?"PHẢN CÔNG THÀNH CÔNG":counterExpired?"BỨT TỚI ĐIỂM NHẢY":escapeTravel>=2500?(CounterEnginesAlive>0?"PHẢN CÔNG / PHÁ HAI ĐỘNG CƠ":"PHẢN CÔNG / LÕI ĐÃ LỘ"):escapeTravel>=2400?"MỤC TIÊU ĐANG VÀO LÀN":escapeTravel>=2000?"TRUY KÍCH HẠNG NẶNG VƯỢT LÊN":"CẮT ĐUÔI TRUY KÍCH";
        public string CounterMissionText=>Sector!=4||missionBossStarted?"":CounterActive?CounterPhaseText+" / CÒN "+Mathf.CeilToInt((3200-escapeTravel)/50)+"s":counterSucceeded?"ĐÃ HẠ TRUY KÍCH / ĐƯỜNG THOÁT BỚT HỎA LỰC":counterExpired?"TRUY KÍCH ĐÃ RÚT / TỚI ĐIỂM NHẢY":"";
        public Vector2 CounterIonAim=>new Vector2(CounterOwns(counterElite)?CounterEnginePosition(counterEngineHealth[0]>0?0:counterEngineHealth[1]>0?1:-1).x:0,-1.8f);
        bool CounterOwns(Enemy e)=>e!=null&&e==counterElite&&e.serial==counterSerial;
        bool CounterCanReceive=>CounterActive&&escapeTravel>=2500;
        Vector2 CounterEnginePosition(int side)=>!CounterOwns(counterElite)?Vector2.zero:counterElite.position+(side<0?Vector2.zero:new Vector2(side==0?-1.15f:1.15f,-.48f));
        void ClearCounterTarget(){counterTargetPart=-2;counterTargetAge=0;}
        void TrackCounterTarget(int part){counterTargetPart=part;counterTargetAge=0;}

        void EnsureCounterVisuals()
        {
            for(int i=0;i<2;i++)if(counterEngines[i]==null)
            {
                counterEngines[i]=IonArt.Sprite("Counterattack engine weakpoint",IonArt.MissionProps[2],Vector2.zero,Vector2.one,Color.white,16,transform);
                counterRings[i]=IonArt.Sprite("Counterattack engine marker",IonArt.Ring,Vector2.zero,Vector2.one,IonArt.Cyan,17,transform);
                counterLines[i]=IonArt.Sprite("Counterattack cannon warning",IonArt.Pixel,Vector2.zero,Vector2.one,Color.clear,7,transform);
            }
            for(int i=0;i<6;i++)if(counterPieces[i]==null)counterPieces[i]=IonArt.Sprite("Counterattack hull fragment",IonArt.Disc,Vector2.zero,Vector2.one,Color.white,12,transform);
            HideCounterVisuals();
        }
        void HideCounterVisuals()
        {
            for(int i=0;i<2;i++){if(counterEngines[i]!=null)counterEngines[i].gameObject.SetActive(false);if(counterRings[i]!=null)counterRings[i].gameObject.SetActive(false);if(counterLines[i]!=null)counterLines[i].gameObject.SetActive(false);counterWarning[i]=false;}
            foreach(var p in counterPieces)if(p!=null)p.gameObject.SetActive(false);
        }
        void ResetCounterattack()
        {
            HideCounterVisuals();
            if(CounterOwns(counterElite)){counterElite.active=false;counterElite.dying=false;counterElite.renderer.gameObject.SetActive(false);counterElite.engine.gameObject.SetActive(false);counterElite.halo.gameObject.SetActive(false);}
            counterElite=null;counterSerial=0;counterStarted=counterSucceeded=counterExpired=counterWreck=false;counterWreckTime=counterBurst=0;counterWreckEvents=0;
            counterEngineHealth[0]=counterEngineHealth[1]=18;counterFire[0]=1.3f;counterFire[1]=2.4f;ClearCounterTarget();
        }
        void BeginCounterFlypast()
        {
            if(counterStarted)return;int slot=FreeEnemy();if(slot<0)return;counterStarted=true;EnsureCounterVisuals();
            counterElite=SpawnEnemy(slot,1,new Vector2(0,3),42);counterSerial=counterElite.serial;SetEnemyVariant(counterElite,17);
            counterElite.renderer.transform.localScale=Vector3.one*IonArt.ScaleFor(counterElite.renderer.sprite,4.5f);
            counterEntry=new Vector2(HalfWidth+1,-5.5f);counterElite.entry=counterElite.position=counterEntry;counterElite.age=0;counterElite.fire=99;
            counterElite.renderer.transform.position=counterEntry;hud.Toast("TRUY KÍCH HẠNG NẶNG ĐANG VƯỢT LÊN / CHUẨN BỊ PHẢN CÔNG!");
        }
        void EnterCounterWindow()
        {
            HideEscapeWarnings();
            foreach(var e in enemies)if(e.active&&e.escapePursuer){e.active=false;e.escapeWarning=false;e.renderer.gameObject.SetActive(false);e.engine.gameObject.SetActive(false);e.halo.gameObject.SetActive(false);}
            ClearHostileBullets();foreach(var o in world)if(o.active&&o.kind==WorldKind.Rock)ReleaseWorld(o);
            escapeRockClock=2.6f;
            if(!counterStarted)BeginCounterFlypast();
            hud.Toast("CƠ HỘI PHẢN CÔNG / PHÁ HAI ĐỘNG CƠ ĐỂ LỘ LÕI!");
        }
        bool UpdateCounterEnemy(Enemy e,float dt)
        {
            if(!CounterOwns(e))return false;
            if(State!=FlightState.Playing)return true;
            e.age+=dt*EnemySlow(e.position);e.motionTime+=dt;e.flash=Mathf.Max(0,e.flash-dt);
            float t=escapeTravel/50;
            if(t<48)e.position=new Vector2(HalfWidth+1,-5.5f+11.3f*Mathf.Clamp01((t-40)/8));
            else if(t<50)e.position=Vector2.Lerp(new Vector2(HalfWidth+1,5.8f),new Vector2(0,3),Mathf.SmoothStep(0,1,(t-48)/2));
            else e.position=new Vector2(Mathf.Sin((t-50)*.45f)*2,3);
            e.renderer.transform.position=e.position;e.renderer.transform.rotation=Quaternion.Euler(0,0,counterEngineHealth[0]<=0&&counterEngineHealth[1]>0?-9:counterEngineHealth[1]<=0&&counterEngineHealth[0]>0?9:Mathf.Sin(t)*2);
            e.renderer.color=e.flash>0?new Color(1,.6f,.35f):CounterEnginesAlive>0?new Color(.7f,.8f,1):Color.white;
            e.engine.transform.position=e.position+Vector2.up*1.4f;e.engine.transform.localScale=new Vector3(1,.7f,1);e.halo.transform.position=e.position;e.halo.transform.localScale=Vector3.one*4.7f;e.halo.color=new Color(.2f,.7f,1,CounterEnginesAlive>0?.18f:.05f);
            for(int i=0;i<2;i++)
            {
                Vector2 point=CounterEnginePosition(i);bool show=t>=48&&t<64;
                counterEngines[i].gameObject.SetActive(show);counterRings[i].gameObject.SetActive(show);
                counterEngines[i].transform.position=counterRings[i].transform.position=point;
                counterEngines[i].transform.localScale=Vector3.one*IonArt.ScaleFor(counterEngines[i].sprite,.8f);counterRings[i].transform.localScale=Vector3.one*(1.25f+Mathf.Sin(t*7)*.08f);
                counterEngines[i].color=counterEngineHealth[i]>0?Color.white:new Color(.24f,.16f,.12f);counterRings[i].color=counterEngineHealth[i]>0?new Color(.3f,1,.85f,.9f):new Color(1,.28f,.1f,.24f);
                if(counterEngineHealth[i]<=0){if(RandomRange(0,1)<dt*7)Emit(point,Vector2.up*.5f,new Color(.3f,.2f,.18f),.6f,.5f);continue;}
                if(t<50)continue;counterFire[i]-=dt*EnemySlow(e.position);
                if(!counterWarning[i]&&counterFire[i]<=.85f){counterWarning[i]=true;counterAim[i]=(playerPosition-point).normalized;}
                if(counterWarning[i])
                {
                    DrawLine(counterLines[i],point,point+counterAim[i]*12,.04f,new Color(1,.55f,.1f,.5f));
                    if(counterFire[i]<=0)
                    {
                        for(int shot=-1;shot<=1;shot++){var b=Bolt(point,(Vector2)(Quaternion.Euler(0,0,shot*10)*counterAim[i])*4.5f,false,.13f);if(b!=null)b.damage=10;}
                        counterFire[i]=3.1f;counterWarning[i]=false;counterLines[i].gameObject.SetActive(false);Play(IonArt.Impact,1.1f);
                    }
                }
            }
            return true;
        }
        float ModifyCounterDamage(Enemy e,float damage)
        {
            if(!CounterOwns(e))return damage;TrackCounterTarget(-1);
            if(!CounterCanReceive||CounterEnginesAlive>0){Ring(e.position,IonArt.Cyan,2,.12f);return 0;}return damage;
        }
        void DamageCounterEngine(int side,float damage,Projectile source=null)
        {
            if(!CounterCanReceive||side<0||side>1||counterEngineHealth[side]<=0||damage<=0)return;
            float accepted=Mathf.Min(counterEngineHealth[side],damage);counterEngineHealth[side]=Mathf.Max(0,counterEngineHealth[side]-damage);TrackCounterTarget(side);
            Ring(CounterEnginePosition(side),IonArt.Gold,.8f,.2f);Play(IonArt.Impact,1.3f);
            if(source!=null&&source.primary)OnModuleHit(source,counterElite,accepted);
            if(counterEngineHealth[side]<=0)
            {
                counterWarning[side]=false;counterLines[side].gameObject.SetActive(false);Explosion(CounterEnginePosition(side),.8f);
                hud.Toast(CounterEnginesAlive==0?"HAI ĐỘNG CƠ ĐÃ PHÁ / LÕI ĐÃ LỘ!":"ĐÃ PHÁ ĐỘNG CƠ / MỘT CỤM PHÁO ĐÃ TẮT!");
            }
        }
        bool HitCounterattack(Projectile b,Vector2 old)
        {
            if(!b.active||!b.friendly||!CounterCanReceive)return false;
            for(int i=0;i<2;i++)
            {
                ulong bit=1UL<<(60+i);if(counterEngineHealth[i]<=0||(b.worldHitMask&bit)!=0)continue;
                float radius=.5f+b.radius;if(SegmentDistance(old,b.position,CounterEnginePosition(i))>radius*radius)continue;
                b.worldHitMask|=bit;b.hitMask|=1UL<<counterElite.index;DamageCounterEngine(i,b.damage,b);if(--b.pierce<=0)Release(b);return true;
            }
            ulong bodyBit=1UL<<counterElite.index;if((b.hitMask&bodyBit)!=0||SegmentDistance(old,b.position,counterElite.position)>(1.0f+b.radius)*(1.0f+b.radius))return false;
            b.hitMask|=bodyBit;DamageEnemy(counterElite,b.damage,b);if(--b.pierce<=0)Release(b);return true;
        }
        bool CounterHomingTarget(Projectile b,out Vector2 point)
        {
            point=Vector2.zero;if(!b.friendly||!CounterCanReceive)return false;float nearest=100;bool found=false;
            for(int i=0;i<2;i++)if(counterEngineHealth[i]>0&&(b.worldHitMask&(1UL<<(60+i)))==0)
            {Vector2 p=CounterEnginePosition(i);if(p.y<b.position.y-.5f)continue;float d=(p-b.position).sqrMagnitude;if(d<nearest){nearest=d;point=p;found=true;}}
            return found;
        }
        void CounterUltimatePulse(float damage)
        {
            if(!CounterCanReceive)return;DamageCounterEngine(0,damage);DamageCounterEngine(1,damage);
        }
        void CounterAreaDamage(Vector2 p,float radius,float damage)
        {
            if(!CounterCanReceive)return;for(int i=0;i<2;i++)if((CounterEnginePosition(i)-p).sqrMagnitude<radius*radius)DamageCounterEngine(i,damage);
            // The caller's normal enemy loop applies body damage once, through ModifyCounterDamage.
        }
        bool OnCounterEnemyKilled(Enemy e)
        {
            if(!CounterOwns(e))return false;
            counterSucceeded=true;counterWreck=true;counterWreckTime=counterBurst=0;counterWreckEvents=0;counterWreckOrigin=e.position;e.dying=true;
            for(int i=0;i<2;i++){counterWarning[i]=false;counterLines[i].gameObject.SetActive(false);counterRings[i].gameObject.SetActive(false);counterEngines[i].gameObject.SetActive(false);}
            e.engine.gameObject.SetActive(false);e.halo.gameObject.SetActive(false);ClearHostileBullets();Score+=600;Ammo=Mathf.Min(IonCampaign.MaxAmmo,Ammo+40);
            TrackCounterTarget(-1);hud.Toast("PHẢN CÔNG THÀNH CÔNG / +600 ĐIỂM / +40 ĐẠN — BỨT THOÁT!");hud.Refresh();
            return true;
        }
        void ExpireCounterattack()
        {
            if(counterExpired||counterSucceeded)return;counterExpired=true;
            if(CounterOwns(counterElite)){counterElite.active=false;counterElite.renderer.gameObject.SetActive(false);counterElite.engine.gameObject.SetActive(false);counterElite.halo.gameObject.SetActive(false);}
            HideCounterVisuals();ClearCounterTarget();ClearHostileBullets();hud.Toast("TRUY KÍCH ĐÃ RÚT / TIẾP TỤC TỚI ĐIỂM NHẢY!");
        }
        void RetireCounterForBoss()
        {
            HideCounterVisuals();ClearCounterTarget();counterWreck=false;
            if(!CounterOwns(counterElite))return;
            counterElite.active=false;counterElite.dying=false;
            counterElite.renderer.gameObject.SetActive(false);counterElite.engine.gameObject.SetActive(false);counterElite.halo.gameObject.SetActive(false);
        }
        void TickCounterWreck(float dt)
        {
            counterTargetAge+=dt;if(!counterWreck)return;
            if(!CounterOwns(counterElite)){counterWreck=false;HideCounterVisuals();return;}
            counterWreckTime+=dt;counterBurst-=dt;var e=counterElite;
            if(counterWreckTime<1.2f)
            {
                e.position=counterWreckOrigin+new Vector2(Mathf.Sin(counterWreckTime*29)*.035f,-counterWreckTime*.3f);e.renderer.transform.position=e.position;e.renderer.transform.rotation=Quaternion.Euler(0,0,-counterWreckTime*16);e.renderer.color=Color.Lerp(Color.white,new Color(.35f,.12f,.07f),counterWreckTime/1.2f);
                if(counterBurst<=0){counterBurst=.14f;Explosion(e.position+new Vector2(RandomRange(-1,1),RandomRange(-.8f,.8f)),.6f);}
                return;
            }
            if(counterWreckEvents==0)
            {
                counterWreckEvents=1;e.renderer.gameObject.SetActive(false);Sprite sprite=e.renderer.sprite;
                if(!wreckSlices.TryGetValue(sprite,out var slices))
                {slices=new Sprite[6];var r=sprite.rect;for(int i=0;i<6;i++)slices[i]=Sprite.Create(sprite.texture,new Rect(r.x+i%3*r.width/3,r.y+i/3*r.height/2,r.width/3,r.height/2),Vector2.one*.5f,sprite.pixelsPerUnit);wreckSlices.Add(sprite,slices);}
                float scale=e.renderer.transform.localScale.x;
                for(int i=0;i<6;i++)
                {
                    var p=counterPieces[i];p.sprite=slices[i];p.transform.localScale=Vector3.one*scale;p.transform.position=e.position+new Vector2((i%3-1)*.9f,(i/3-.5f)*1.2f);p.transform.rotation=e.renderer.transform.rotation;p.gameObject.SetActive(true);counterPieceVelocity[i]=new Vector2((i%3-1)*1.9f,(i/3-.5f)*1.7f-1);
                }
                Explosion(e.position,2);Ring(e.position,IonArt.Orange,6,.8f);Play(IonArt.Blast,.5f);
            }
            for(int i=0;i<6;i++){var p=counterPieces[i];counterPieceVelocity[i]+=Vector2.down*dt;p.transform.position+=(Vector3)(counterPieceVelocity[i]*dt);p.transform.Rotate(0,0,(i%2==0?1:-1)*dt*(35+i*6));p.color=new Color(.7f,.4f,.25f,Mathf.Clamp01((3.4f-counterWreckTime)/1.0f));}
            if(counterWreckTime>=2.2f&&counterWreckEvents==1){counterWreckEvents=2;Explosion(e.position,2.5f);Play(IonArt.Blast,.35f);}
            if(counterWreckTime>=3.4f){counterWreck=false;e.dying=false;foreach(var p in counterPieces)p.gameObject.SetActive(false);}
        }
    }
}
