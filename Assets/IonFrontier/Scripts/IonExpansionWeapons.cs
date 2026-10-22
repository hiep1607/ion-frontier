using UnityEngine;

namespace IonFrontier
{
    public sealed partial class IonGame
    {
        readonly SpriteRenderer[] wingmen=new SpriteRenderer[3];
        readonly Vector2[] burnZones=new Vector2[6];
        Vector2 specialCenter;int absorbedBolts,expansionPulse;float expansionFireClock;
        public int AbsorbedBolts=>absorbedBolts;
        float EnemySlow(Vector2 position)=>SelectedHull==7&&UltimateActive&&ultimateTime<3&&(position-specialCenter).sqrMagnitude<49?.32f:1;
        void StartExpansionUltimate()
        {
            specialCenter=playerPosition;absorbedBolts=expansionPulse=0;expansionFireClock=0;
            for(int i=0;i<3;i++)
            {
                if(wingmen[i]==null)wingmen[i]=IonArt.Sprite("Ultimate wingman "+i,IonArt.PlayerShips[SelectedHull],Vector2.zero,Vector2.one,Color.white,21,transform);
                wingmen[i].sprite=IonArt.PlayerShips[SelectedHull];wingmen[i].gameObject.SetActive(SelectedHull==4&&i<2||SelectedHull==6);
                wingmen[i].color=SelectedHull==4?new Color(.65f,.5f,1,.6f):Color.white;
                wingmen[i].transform.localScale=Vector3.one*IonArt.ScaleFor(wingmen[i].sprite,1.35f);
            }
            for(int i=0;i<6;i++)burnZones[i]=new Vector2(100,100);
        }
        void HideExpansionUltimate(){foreach(var sr in wingmen)if(sr!=null)sr.gameObject.SetActive(false);}
        Enemy NearestSpecialTarget(Vector2 p)
        {
            Enemy nearest=null;float distance=float.MaxValue;foreach(var e in enemies)if(e.active){float d=(e.position-p).sqrMagnitude;if(d<distance){nearest=e;distance=d;}}return nearest;
        }
        void SpecialArea(Vector2 p,float radius,float damage)
        {
            CounterAreaDamage(p,radius,damage);
            foreach(var e in enemies)if(e.active&&(e.position-p).sqrMagnitude<radius*radius){DamageEnemy(e,damage);if(State!=FlightState.Playing)return;}
            if(Sector>=4)foreach(var o in world)if(o.active&&(o.position-p).sqrMagnitude<radius*radius&&(o.kind==WorldKind.Generator||o.kind==WorldKind.Guard||o.kind==WorldKind.Mine||o.kind==WorldKind.Rock||o.kind==WorldKind.Tether))DamageWorld(o,damage);
        }
        void UpdateExpansionUltimate(float dt,float previous)
        {
            expansionFireClock-=dt;Color color=IonCampaign.HullColors[SelectedHull];
            if(SelectedHull==3)
            {
                if(expansionPulse<6&&ultimateTime>=.15f+expansionPulse*.4f)
                {
                    Enemy target=NearestSpecialTarget(playerPosition);Vector2 p=target==null?playerPosition+Vector2.up*5:target.position;
                    burnZones[expansionPulse]=p;expansionPulse++;UltimatePulseCount=expansionPulse;
                    Explosion(p,1.5f);SpecialArea(p,2.4f,10);if(State!=FlightState.Playing)return;Play(IonArt.Blast,.9f);
                }
                for(int i=0;i<expansionPulse;i++)
                {
                    SpecialArea(burnZones[i],2,3*dt);if(State!=FlightState.Playing)return;
                    if(expansionFireClock<=0)Emit(burnZones[i]+new Vector2(RandomRange(-1,1),RandomRange(-1,1)),Vector2.up*.8f,color,.45f,.65f);
                }
                if(expansionFireClock<=0)expansionFireClock=.12f;
                if(ultimateTime>=3)EndUltimate();
            }
            else if(SelectedHull==4)
            {
                for(int i=0;i<2;i++)
                {
                    Vector2 p=playerPosition+new Vector2((i==0?-1:1)*2,Mathf.Sin(ultimateTime*5)*.3f);
                    wingmen[i].transform.position=p;
                    if(expansionFireClock<=0)
                    {
                        Enemy target=NearestSpecialTarget(p);Vector2 direction=target==null?Vector2.up:(target.position-p).normalized;
                        var shot=Bolt(p+Vector2.up*.6f,direction*20,true);if(shot!=null){shot.damage=2.5f;shot.renderer.color=color;}
                    }
                }
                if(expansionFireClock<=0){expansionFireClock=.22f;UltimatePulseCount++;Play(IonArt.Laser,1.2f);}
                if(ultimateTime>=4)EndUltimate();
            }
            else if(SelectedHull==5)
            {
                fortressField.gameObject.SetActive(true);fortressField.transform.position=playerPosition;
                fortressField.transform.localScale=Vector3.one*(ultimateTime<1.6f?6:2);fortressField.color=new Color(.2f,1,.8f,.65f);
                if(ultimateTime<1.6f)
                {
                    foreach(var b in bullets)if(b.active&&!b.friendly&&(b.position-playerPosition).sqrMagnitude<64)
                    {
                        b.position=Vector2.MoveTowards(b.position,playerPosition,dt*12);b.renderer.transform.position=b.position;
                        if((b.position-playerPosition).sqrMagnitude<1){absorbedBolts++;Release(b);}
                    }
                }
                else if(expansionPulse==0)
                {
                    expansionPulse=1;UltimatePulseCount=1;Enemy target=NearestSpecialTarget(playerPosition);Vector2 aim=target==null?Vector2.up:(target.position-playerPosition).normalized;
                    for(int i=0;i<12;i++)
                    {
                        Vector2 direction=Quaternion.Euler(0,0,(i-5.5f)*5)*aim;var shot=Bolt(playerPosition+Vector2.up*.7f,direction*21,true,.16f);
                        if(shot!=null){shot.damage=8+Mathf.Min(7,absorbedBolts/4);shot.homing=true;shot.pierce=2;shot.renderer.color=color;}
                    }
                    Ring(playerPosition,color,8,.6f);Play(IonArt.Blast,.65f);
                }
                if(ultimateTime>=2.2f)EndUltimate();
            }
            else if(SelectedHull==6)
            {
                for(int i=0;i<3;i++)
                {
                    Vector2 p=playerPosition+new Vector2((i-1)*3,1.8f+Mathf.Sin(ultimateTime*4+i)*.5f);wingmen[i].transform.position=p;
                    if(expansionFireClock<=0)
                    {
                        Enemy target=NearestSpecialTarget(p);if(target!=null)
                        {DrawLine(lightning[i],p,target.position,.075f,color);Ring(target.position,color,.8f,.2f);DamageEnemy(target,4);if(State!=FlightState.Playing)return;}
                    }
                }
                if(expansionFireClock<=0){expansionFireClock=.45f;UltimatePulseCount++;Play(IonArt.Impact,1.3f);}
                else if(expansionFireClock<.25f)HideLightning();
                if(ultimateTime>=4)EndUltimate();
            }
            else
            {
                fortressField.gameObject.SetActive(true);fortressField.transform.position=specialCenter;
                fortressField.transform.localScale=Vector3.one*14;fortressField.color=new Color(.8f,.25f,1,.55f);
                if(ultimateTime>=3&&expansionPulse==0)
                {
                    expansionPulse=1;UltimatePulseCount=1;SpecialArea(specialCenter,7,90);if(State!=FlightState.Playing)return;
                    Ring(specialCenter,color,14,.7f);Explosion(specialCenter,2.5f);ClearHostileBullets();Play(IonArt.Blast,.35f);
                }
                if(ultimateTime>=3.8f)EndUltimate();
            }
        }
    }
}
