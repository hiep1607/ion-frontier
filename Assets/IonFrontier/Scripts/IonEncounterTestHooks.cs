using UnityEngine;

namespace IonFrontier
{
    public sealed partial class IonGame
    {
        public void TestStartEncounter(int type)
        {
            ClearEncountersForBoss();foreach(var e in enemies)if(e.active&&e.kind<8)RetireEncounterEnemy(e);ClearHostileBullets();ClearBeams();
            foreach(var o in world)if(o!=null&&o.active&&(o.kind==WorldKind.Mine||o.kind==WorldKind.Rock))ReleaseWorld(o);
            TestFreezeEnemies=true;encounterRecovery=0;BeginEncounter(Mathf.Clamp(type,0,5),EncounterVariants(Mathf.Clamp(type,0,5)),null);
        }
        public void TestAdvanceEncounter(float seconds)
        {
            bool frozen=TestFreezeEnemies;TestFreezeEnemies=false;
            try
            {
                double remaining=System.Math.Max(0,seconds);
                while(remaining>.000001&&State==FlightState.Playing)
                {float step=(float)System.Math.Min(.05,remaining);TickEncounters(step);foreach(var e in enemies)if(e.active&&EncounterOwns(e))UpdateEncounterEnemy(e,step);remaining-=step;}
            }
            finally{TestFreezeEnemies=frozen;}
        }
        public float TestEncounterDamageRatio()
        {
            var source=EncounterRole(0);var target=EncounterRole(1);if(target==null)return -1;if(source!=null)target.position=source.position+Vector2.right;
            float before=target.health;DamageEnemy(target,1);return before-target.health;
        }
        public void TestKillEncounterRole(int role){var e=EncounterRole(role);if(e!=null)DamageEnemy(e,10000);}
        public void TestSetEncounterCarrierHealth(float hp){var e=EncounterRole(0);if(e!=null)e.health=Mathf.Clamp(hp,.1f,e.maxHealth);}
        public float TestEncounterCarrierHealth=>EncounterRole(0)!=null?EncounterRole(0).health:0;
        public float TestEncounterClock=>encounterClock;
        public void TestEncounterSetSeed(int seed){encounterSeed=seed;encounterRandom=new System.Random(seed);}
        public bool TestEncounterPullActive=>encounterRunning&&encounterType==1&&EncounterRole(0)!=null&&EncounterRole(1)!=null&&encounterClock>=2.75f&&(encounterClock-2)%7<1.75f&&(encounterClock-2)%7>=.75f;
        public Vector2 TestEncounterRolePosition(int role){var e=EncounterRole(role);return e!=null?e.position:Vector2.zero;}
        public void TestDestroyAllEncounterMembers(){foreach(var m in encounterMembers)if(EncounterMemberAlive(m))DamageEnemy(m.enemy,10000);}
        public bool TestEncounterReusedSlotSafe()
        {
            var e=EncounterRole(1);if(e==null)return false;int slot=e.index;RetireEncounterEnemy(e);var replacement=SpawnEnemy(slot,1,new Vector2(4,3),5);SetEnemyVariant(replacement,0);return !EncounterOwns(replacement);
        }
        public bool TestEncounterLinksVisible{get{foreach(var s in encounterLinks)if(s!=null&&s.gameObject.activeSelf)return true;return false;}}
        public void TestClearEncounter(){ClearEncountersForBoss();}
    }
}
