using UnityEngine;
namespace IonFrontier
{
    public sealed partial class IonGame
    {
        public string TestDraftInstruction="";
        public bool TestClarityTerminalRace()
        {
            TestRatingBeginBoss();boss.position=Vector2.zero;boss.age=4;boss.health=1;
            var shot=Bolt(new Vector2(0,-.7f),Vector2.up*10,true);shot.damage=2;shot.homing=false;
            Health=5;DamageTaken=95;invincible=0;HurtPlayer(10);bool lost=State==FlightState.Defeat;
            UpdateBullets(.02f);DamageEnemy(boss,100);BeginBossDeath(boss);for(int i=0;i<50;i++)UpdateBossDeath(.1f);Finish(true);
            return lost&&State==FlightState.Defeat&&Health==0&&ClearedStages==3&&StarsAt(4)==0&&IonCampaign.Load(profilePath).clearedStages==3;
        }
        public bool TestClarityWorldAfterLoss()
        {
            Health=5;invincible=0;SpawnWorld(WorldKind.Rock,playerPosition,5,.6f);UpdateMission(.02f);float time=MissionTime,distance=escapeTravel;int rocks=EscapeRocksCleared;
            UpdateMission(.1f);TickEscapeMission(.1f);return State==FlightState.Defeat&&Health==0&&MissionTime==time&&escapeTravel==distance&&EscapeRocksCleared==rocks;
        }
        Enemy ClaritySource(int slot,float x){var e=SpawnEnemy(slot,1,new Vector2(x,3),30);SetEnemyVariant(e,16);e.position=e.target;e.entry=e.target;e.age=3;return e;}
        public bool TestClarityBeam(int scenario)
        {
            TestFreezeEnemies=true;ClearBeams();var source=ClaritySource(0,0);var other=ClaritySource(1,4);
            if(scenario!=5)SpawnBeam(new Vector2(0,5),source);SpawnBeam(new Vector2(4,5),other);SpawnBeam(new Vector2(-4,5));
            playerPosition=new Vector2(7,-3);if(scenario==1)for(int i=0;i<65;i++)UpdateBeams(.02f);
            if(scenario==2){source.active=false;ClaritySource(0,0);}else if(scenario==3)source.serial++;else if(scenario==4)RetireEncounterEnemy(source);else DamageEnemy(source,1000);
            invincible=0;playerPosition=new Vector2(0,-3);float hp=Health;
            for(int i=0;i<65;i++)UpdateBeams(.02f);
            bool noOwnerBeam=true;foreach(var b in beams)if(b!=null&&b.active&&b.source==source)noOwnerBeam=false;
            bool otherAlive=false,worldAlive=false;foreach(var b in beams)if(b!=null&&b.active){if(b.source==other)otherAlive=true;if(b.source==null)worldAlive=true;}
            // Firing scenario already advanced1.3s, so the survivors have ended naturally. Other cases still have .6s left.
            return noOwnerBeam&&Health==hp&&(scenario==1||otherAlive&&worldAlive);
        }
        public bool TestClarityBeamDamageSlice()
        {
            ClearBeams();playerPosition=new Vector2(0,-3);invincible=0;float hp=Health;SpawnBeam(new Vector2(0,5));UpdateBeams(1);bool safe=Health==hp;UpdateBeams(.2f);return safe&&Mathf.Abs((hp-Health)-2)<.01f;
        }
        public bool TestClarityInputCallback(int slot)
        {
            TestOpenDraft(0);int gifts=GiftsCollected;hud.TestClickDraftCard(slot);hud.TestClickDraftCard((slot+1)%3);
            return DraftSelected==slot&&DraftChoicesMade==1&&GiftsCollected==gifts;
        }
        public void TestPrepareClarityInput()
        {
            TestFreezeEnemies=true;TestInput=false;TestFire=false;TestClearProjectiles();foreach(var o in world)if(o!=null&&o.active)ReleaseWorld(o);ClaritySource(0,0);waveLive=true;Wave=1;
        }
        public bool TestClarityPreview()
        {
            ApplyPickupEffect(PickupKind.Spread,false);ApplyPickupEffect(PickupKind.Pierce,false);TestGrantModule(CombatModule.FocusedSpread);
            string before=loadout.Summary;var card=PickupCard(PickupKind.Homing);return card.description.Contains("SẼ NGỪNG")&&card.description.Contains("→")&&before==loadout.Summary&&ModuleActive(CombatModule.FocusedSpread);
        }
        public bool TestClarityModuleCounters()
        {
            TestGrantModule(CombatModule.PulseChamber);TestGrantModule(CombatModule.ReactiveArmor);for(int i=0;i<3;i++)TryFire();invincible=0;HurtPlayer(5);
            return ModuleStatus(CombatModule.PulseChamber).Contains("3 / 8")&&ModuleStatus(CombatModule.ReactiveArmor).Contains("HỒI")&&ModuleArmorPulses==1;
        }
        public void TestClarityHudSample()
        {
            TestTargets();foreach(var e in enemies)if(e.active)e.health=e.maxHealth=10000;TestGrantModule(CombatModule.PulseChamber);TestGrantModule(CombatModule.ReactiveArmor);invincible=0;HurtPlayer(5);TestProtection(10000);TestInput=true;TestFire=true;TestTarget=playerPosition;
        }
        public void TestClarityModuleDraft(){ApplyPickupEffect(PickupKind.Spread,false);ApplyPickupEffect(PickupKind.Pierce,false);TestOpenDraft(0);}
        public void TestClarityDashCooldown(){dashCooldown=2.4f;}
        public void TestClarityStress()
        {
            TestRatingBeginBoss();foreach(var e in enemies)if(e.active&&e.kind>=8)e.health=e.maxHealth=100000;
            for(int i=3;i<28;i++){var e=SpawnEnemy(i,1,new Vector2((i%8-3.5f)*2.2f,2+(i/8)*.65f),10000);SetEnemyVariant(e,7+i%12);e.position=e.entry=e.target;e.age=3;e.fire=.4f+i*.06f;}
            for(int i=0;i<10;i++){ApplyPickupEffect(PickupKind.Damage,false);ApplyPickupEffect(PickupKind.FireRate,false);ApplyPickupEffect(PickupKind.Multishot,false);}ApplyPickupEffect(PickupKind.Spread,false);ApplyPickupEffect(PickupKind.Pierce,false);for(int i=0;i<3;i++)ApplyPickupEffect(PickupKind.Upgrade,false);TestGrantModule(CombatModule.FocusedSpread);TestGrantModule(CombatModule.PiercingFragments);TestInput=true;TestFire=true;TestFreezeEnemies=false;TestTarget=new Vector2(0,-4.7f);TestProtection(10000);Ammo=600;
        }
        public void TestClarityModuleLegend(){hud.TestModuleLegend();}
        public Vector2 TestBalanceAim()
        {
            float desired=CounterActive?CounterIonAim.x:Mathf.Sin(MissionTime*.65f)*3;
            float best=float.MaxValue,bestX=playerPosition.x;float y=-1.8f;
            for(int i=0;i<21;i++)
            {
                float x=Mathf.Lerp(-HalfWidth+1,HalfWidth-1,i/20f);Vector2 p=new Vector2(x,y);float cost=Mathf.Abs(x-desired)*.35f+Mathf.Abs(x-playerPosition.x)*.12f;
                foreach(var o in world)if(o!=null&&o.active&&(o.kind==WorldKind.Rock||o.kind==WorldKind.Mine)){float t=Mathf.Clamp((y-o.position.y)/Mathf.Min(-.1f,o.velocity.y),0,1);float d=(o.position+o.velocity*t-p).magnitude;cost+=Mathf.Max(0,o.radius+1.0f-d)*9;}
                foreach(var b in bullets)if(b.active&&!b.friendly){float d=Mathf.Sqrt(SegmentDistance(b.position,b.position+b.velocity*.65f,p));cost+=Mathf.Max(0,.85f-d)*7;}
                if(cost<best){best=cost;bestX=x;}
            }
            return new Vector2(bestX,y);
        }
    }
}
