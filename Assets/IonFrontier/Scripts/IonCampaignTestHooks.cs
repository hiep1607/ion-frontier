using UnityEngine;

namespace IonFrontier
{
    public sealed partial class IonGame
    {
        Projectile slowInside,slowOutside;Vector2 slowInsideOrigin,slowOutsideOrigin;
        public float TestSlowRatio=>slowInside==null||slowOutside==null?1:(slowInside.position-slowInsideOrigin).magnitude/Mathf.Max(.001f,(slowOutside.position-slowOutsideOrigin).magnitude);
        public void TestSlowBolts(){slowInsideOrigin=playerPosition+new Vector2(1,3);slowOutsideOrigin=playerPosition+new Vector2(10,3);slowInside=Bolt(slowInsideOrigin,Vector2.down*2,false);slowOutside=Bolt(slowOutsideOrigin,Vector2.down*2,false);}
        public int ActiveWorldCount{get{int n=0;foreach(var o in world)if(o!=null&&o.active)n++;return n;}}
        public float TestChaseProgress=>chaseProgress;
        public int TestRescueAllies=>missionRescues;
        public float TestTotalEnemyHealth{get{float n=0;foreach(var e in enemies)if(e.active)n+=e.health;return n;}}
        public void TestSetProgress(int stage){profile.clearedStages=Mathf.Clamp(stage,0,13);profile.selectedHull=Mathf.Min(profile.selectedHull,UnlockedHulls-1);profile.tutorialDone=true;IonCampaign.Save(profilePath,profile);hud.Refresh();}
        public void TestMissionClock(float seconds){missionTime=seconds;}
        public void TestHitConvoy(float amount){convoyHitClock=0;foreach(var o in world)if(o!=null&&o.active&&o.kind==WorldKind.Convoy){DamageConvoy(o,amount);return;}}
        public void TestShootObjective()
        {
            foreach(var o in world)if(o!=null&&o.active&&(o.kind==WorldKind.Generator||o.kind==WorldKind.Tether||o.kind==WorldKind.Guard))
            {var shot=Bolt(o.position+Vector2.down*1.7f,Vector2.up*16,true);if(shot!=null){shot.damage=100;shot.homing=false;shot.pierce=1;}return;}
        }
        public void TestCaptureNextObjective()
        {
            foreach(var o in world)if(o!=null&&o.active&&!o.done&&(o.kind==WorldKind.Beacon||o.kind==WorldKind.Zone))
            {
                playerPosition=o.position;playerVelocity=Vector2.zero;TestTarget=playerPosition;
                int steps=o.kind==WorldKind.Zone?45:17;for(int i=0;i<steps&&State==FlightState.Playing;i++)UpdateMission(.25f);return;
            }
        }
        public void TestTrackSignal()
        {
            for(int i=0;i<240;i++){playerPosition=new Vector2(Mathf.Sin((missionTime+.25f)*.25f)*6,-2);UpdateMission(.25f);}
        }
        public void TestMineChain()
        {
            foreach(var o in world)if(o!=null&&o.active&&(o.kind==WorldKind.Mine||o.kind==WorldKind.Rock))ReleaseWorld(o);
            for(int i=0;i<3;i++)SpawnWorld(WorldKind.Mine,new Vector2(i*2,1),3,.45f);
            var shot=Bolt(new Vector2(0,-1),Vector2.up*16,true);if(shot!=null){shot.damage=5;shot.homing=false;shot.pierce=1;}
        }
        public void TestCollapseHit(){playerPosition=new Vector2(0,-6);invincible=0;UpdateMission(.05f);}
        public void TestBossSalvo(){if(BossActive)BossAttack(boss);}
        public void TestMagnetBolts(){for(int i=0;i<8;i++)Bolt(playerPosition+new Vector2((i%4-1.5f)*.3f,1.2f+i/4*.3f),Vector2.down,false);}
    }
}
