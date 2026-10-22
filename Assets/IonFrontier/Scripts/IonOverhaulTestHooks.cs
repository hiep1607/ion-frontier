using UnityEngine;

namespace IonFrontier
{
    public sealed partial class IonGame
    {
        // Opt-in harness helpers: accelerate holding/travel, but use real objective state,
        // pooled projectile collisions, damage gates and the mission director transitions.
        public void TestWorkMissionBeat()
        {
            if(missionBossStarted)return;
            int before=flowBeat;
            if(Sector==4)
            {
                for(int i=0;i<24&&State==FlightState.Playing&&!missionBossStarted&&flowBeat==before;i++){UpdateMission(.25f);TickEscapeMission(.25f);}return;
            }
            foreach(var e in enemies)if(e.active&&e.kind<8)DamageEnemy(e,10000);
            if(Sector==10&&flowBeat==0)TestMineChain();
            TestShootObjective();
            bool hold=RescueFlow&&flowBeat!=1||Sector==6&&flowBeat==1||Sector==9||Sector==12&&flowBeat==2||Sector==13&&flowBeat==2;
            if(hold)TestCaptureNextObjective();
            if(Sector==7&&flowBeat!=1)TestTrackSignal();
            if(EscortFlow&&flowBeat==1){playerPosition=ConvoyPosition;TestTarget=playerPosition;}
            for(int i=0;i<20&&State==FlightState.Playing&&!missionBossStarted&&flowBeat==before;i++)
            {
                if(EscortFlow&&flowBeat==1)playerPosition=ConvoyPosition;
                UpdateMission(.25f);TickMissionFlow(.25f);
            }
        }
        public bool TestBossArtMatches
        {
            get{foreach(var e in enemies)if(e.active&&e.kind>=8&&e.renderer.sprite!=IonArt.BossFleet[e.bossArt])return false;return BossActive;}
        }
        public int TestAttackAllBosses()
        {
            int n=0;foreach(var e in enemies)if(e.active&&e.kind>=8){BossAttack(e);n++;}return n;
        }
        public void TestHitOpenBoss()
        {
            foreach(var e in enemies)if(e.active&&e.kind>=8&&BossCanTakeDamage(e)){DamageEnemy(e,10000);return;}
        }
        public void TestOpenBossClock(){waveTime+=4.1f;}
        public void TestRevealBoss(){if(boss!=null)boss.age=4;}
        public bool TestClosedBossBlocks()
        {
            foreach(var e in enemies)if(e.active&&e.kind>=8&&!BossCanTakeDamage(e))
            {float hp=e.health;DamageEnemy(e,50);return e.health==hp;}return true;
        }
        public bool TestFreshEnemyAttack(int variant)
        {
            ResetCombat();waveLive=true;transition=1000;TestFreezeEnemies=true;
            var e=SpawnEnemy(0,1,new Vector2(0,3),20);SetEnemyVariant(e,variant);e.age=3;
            int count=ActiveBullets+ActiveWorldCount+ActiveEnemies+BeamCasts;EnemyAttack(e);
            return e.renderer.sprite==IonArt.RegularFleet[variant]&&ActiveBullets+ActiveWorldCount+ActiveEnemies+BeamCasts>count;
        }
    }
}
