using UnityEngine;

namespace IonFrontier
{
    public sealed partial class IonGame
    {
        public void TestCounterBegin()
        {
            if(Sector!=4||State!=FlightState.Playing)return;
            escapeTravel=2500;BeginCounterFlypast();EnterCounterWindow();escapeCounterWindow=true;
            if(!CounterOwns(counterElite))return;
            counterElite.position=new Vector2(0,3);counterElite.renderer.transform.position=counterElite.position;
            UpdateCounterEnemy(counterElite,0);
        }
        public void TestCounterDamageEngine(int side,float damage){DamageCounterEngine(side,damage);}
        public void TestCounterDamageBody(float damage){if(CounterOwns(counterElite)&&counterElite.active)DamageEnemy(counterElite,damage);}
        public void TestCounterShootEngine(int side)
        {
            if(!CounterCanReceive||side<0||side>1)return;Vector2 p=CounterEnginePosition(side);
            var b=Bolt(p+Vector2.down*1.6f,Vector2.up*16,true);if(b!=null){b.damage=6;b.homing=false;b.pierce=1;}
        }
        public void TestCounterRetreat(){escapeTravel=Mathf.Max(escapeTravel,3200);ExpireCounterattack();}
        public void TestCounterTargetEngine(int side){if(side>=0&&side<2)TrackCounterTarget(side);}
        public bool TestCounterAllClean
        {
            get{if(counterWreck||CounterActive||CounterWarningsAlive>0||CounterTargetActive)return false;foreach(var sr in counterEngines)if(sr!=null&&sr.gameObject.activeSelf)return false;foreach(var sr in counterRings)if(sr!=null&&sr.gameObject.activeSelf)return false;foreach(var sr in counterLines)if(sr!=null&&sr.gameObject.activeSelf)return false;foreach(var sr in counterPieces)if(sr!=null&&sr.gameObject.activeSelf)return false;return true;}
        }
        public Vector2 TestCounterEnginePosition(int side)=>CounterEnginePosition(side);
    }
}
