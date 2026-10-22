using System.Globalization;
using System.Text;
using UnityEngine;
namespace IonFrontier
{
    public sealed partial class IonGame
    {
        // Snapshots read the actual simulation. They exclude UI selection and wall-clock values.
        public string TestDraftSimulationSnapshot()
        {
            var s=new StringBuilder();System.Action<float> add=v=>s.Append(v.ToString("R",CultureInfo.InvariantCulture)).Append('|');
            foreach(float f in new[]{Elapsed,MissionTime,escapeTravel,ultimateTime,deathTime,BossCombatSeconds,targetAge,invincible,dashCooldown,dashTime,supplyClock,fireClock,moduleArcClock,dashModuleTime,armorModuleClock,encounterClock,counterWreckTime})add(f);
            add(Ammo);add(Health);add(Score);add(playerPosition.x);add(playerPosition.y);foreach(var e in enemies)if(e!=null&&e.active){add(e.serial);add(e.age);add(e.fire);add(e.position.x);add(e.position.y);add(e.health);add(e.motionTime);}
            foreach(var b in bullets)if(b!=null&&b.active){add(b.position.x);add(b.position.y);add(b.life);add(b.damage);}
            foreach(var o in world)if(o!=null&&o.active){add(o.position.x);add(o.position.y);add(o.age);add(o.progress);add(o.health);}
            foreach(var c in cells)if(c!=null&&c.active){add(c.position.x);add(c.position.y);add(c.age);}foreach(var b in beams)if(b!=null&&b.active)add(b.age);foreach(var p in sparks)if(p!=null&&p.active){add(p.position.x);add(p.position.y);add(p.life);}
            var backdrop=FindFirstObjectByType<IonBackdrop>();if(backdrop!=null)add(backdrop.MotionClock);return s.ToString();
        }
        public void TestDraftSnapshotDifference(string before)
        {
            string[] a=before.Split('|'),b=TestDraftSimulationSnapshot().Split('|');for(int i=0;i<Mathf.Min(a.Length,b.Length);i++)if(a[i]!=b[i])Debug.Log("ION_DRAFT_FREEZE_DIFF field="+i+" before="+a[i]+" after="+b[i]);if(a.Length!=b.Length)Debug.Log("ION_DRAFT_FREEZE_LENGTH "+a.Length+" -> "+b.Length);
        }
        public void TestHideBanner(){bannerTimer=0;hud.ShowBanner(false,Sector,SectorName);}
        public string TestTacticalProgressSnapshot()=>JsonUtility.ToJson(profile);
        public void TestPrepareUnupgradedCounterTrial(){flowBeat=2;flowReady=true;loadout.Reset();ResetModules();hud.Refresh();}
        public void TestPrepareDraftWorld()
        {
            SpawnWorld(WorldKind.Rock,new Vector2(5,4),8,.6f,Vector2.down);SpawnCell(new Vector2(-5,1),PickupKind.Ammo);Bolt(new Vector2(6,3),Vector2.down*3,false);Bolt(new Vector2(-6,0),Vector2.up*12,true);dashCooldown=1.8f;
        }
        public void TestPrepareMaxDraft()
        {
            loadout.Reset();ApplyPickupEffect(PickupKind.Spread,false);ApplyPickupEffect(PickupKind.Pierce,false);for(int i=0;i<12;i++){ApplyPickupEffect(PickupKind.Upgrade,false);ApplyPickupEffect(PickupKind.Damage,false);ApplyPickupEffect(PickupKind.FireRate,false);ApplyPickupEffect(PickupKind.Multishot,false);}TestGrantModule(CombatModule.PulseChamber);TestGrantModule(CombatModule.SalvageMagnet);Ammo=600;Health=MaxHealth;
        }
        public void TestPrepareModuleCombo()
        {
            TestTargets();foreach(var e in enemies)if(e.active)e.health=e.maxHealth=100000;for(int i=0;i<12;i++){ApplyPickupEffect(PickupKind.Damage,false);ApplyPickupEffect(PickupKind.FireRate,false);ApplyPickupEffect(PickupKind.Multishot,false);}ApplyPickupEffect(PickupKind.Spread,false);ApplyPickupEffect(PickupKind.Pierce,false);for(int i=0;i<3;i++)ApplyPickupEffect(PickupKind.Upgrade,false);TestGrantModule(CombatModule.FocusedSpread);TestGrantModule(CombatModule.PiercingFragments);
        }
        public bool TestDraftCardsValid()
        {
            for(int i=0;i<3;i++){var c=draftCards[i];if(c==null)return false;for(int j=0;j<i;j++)if(c.title==draftCards[j].title)return false;if(!c.points&&c.module<0&&!ValidPickup(c.pickup))return false;if(c.module>=0&&(ModuleCount>=2||HasModule((CombatModule)c.module)))return false;}return true;
        }
        public bool TestModuleSourceSafety(int test)
        {
            PrepareModuleProbe();
            if(test==0)
            {
                TestGrantModule(CombatModule.AftershockRounds);TestGrantModule(CombatModule.AmmoReclaimer);int initial=Ammo;
                for(int i=0;i<10;i++){TestClearProjectiles();var e=ModuleProbeTarget(0,Vector2.zero,.1f);var b=Bolt(e.position+Vector2.down*.7f,Vector2.up*10,true);b.damage=4;b.primary=false;UpdateBullets(.08f);}
                return Kills>=10&&ModuleShockwaves==0&&ModuleAmmoReturned==0&&Ammo==initial;
            }
            if(test==1)
            {
                ApplyPickupEffect(PickupKind.Pierce,false);ApplyPickupEffect(PickupKind.Homing,false);TestGrantModule(CombatModule.PiercingFragments);TestGrantModule(CombatModule.SeekingArc);TestCounterBegin();float before=CounterBodyHealth;
                for(int i=0;i<6;i++){TestClearProjectiles();BeginModuleVolley();var b=Bolt(new Vector2(0,2.2f),Vector2.up*10,true);ConfigureModuleBolt(b,4);b.homing=false;UpdateBullets(.08f);}
                return CounterActive&&CounterBodyHealth==before&&ModuleFragmentsCreated==0&&ModuleArcsCreated==0&&arcHits==0;
            }
            if(test==2||test==3)
            {
                ApplyPickupEffect(PickupKind.Spread,false);ApplyPickupEffect(test==2?PickupKind.Pierce:PickupKind.Homing,false);for(int i=0;i<8;i++)ApplyPickupEffect(PickupKind.Multishot,false);TestGrantModule(test==2?CombatModule.PiercingFragments:CombatModule.SeekingArc);
                var target=ModuleProbeTarget(0,new Vector2(0,-3.1f),1000);ModuleProbeTarget(1,new Vector2(2,-3),1000);
                for(int i=0;i<(test==2?1:6);i++){TestClearProjectiles();TryFire();UpdateBullets(.1f);}
                Debug.Log("ION_MODULE_FAN_PROBE test="+test+" hp="+target.health+" fragments="+ModuleFragmentsCreated+" hits="+arcHits+" arcs="+ModuleArcsCreated+" serial="+volleySerial);return target.health<1000&&(test==2?ModuleFragmentsCreated==4:arcHits==6&&ModuleArcsCreated==1);
            }
            if(test==4)
            {
                TestGrantModule(CombatModule.PiercingFragments);TestGrantModule(CombatModule.SeekingArc);var target=ModuleProbeTarget(0,Vector2.zero,1000);ModuleProbeTarget(1,new Vector2(2,0),1000);
                for(int i=0;i<6;i++){TestClearProjectiles();loadout.Reset();ApplyPickupEffect(PickupKind.Pierce,false);ApplyPickupEffect(PickupKind.Homing,false);BeginModuleVolley();var b=Bolt(new Vector2(0,-.7f),Vector2.up*10,true);ConfigureModuleBolt(b,4);b.homing=false;loadout.Reset();UpdateBullets(.08f);}
                bool retained=ModuleFragmentsCreated==12&&ModuleArcsCreated==1;TestClearProjectiles();BeginModuleVolley();var next=Bolt(new Vector2(0,-.7f),Vector2.up*10,true);ConfigureModuleBolt(next,4);ApplyPickupEffect(PickupKind.Pierce,false);ApplyPickupEffect(PickupKind.Homing,false);UpdateBullets(.08f);
                Debug.Log("ION_MODULE_SNAPSHOT_PROBE retained="+retained+" fragments="+ModuleFragmentsCreated+" hits="+arcHits+" arcs="+ModuleArcsCreated);return retained&&ModuleFragmentsCreated==12&&ModuleArcsCreated==1&&arcHits==6;
            }
            if(test==5)
            {
                TestGrantModule(CombatModule.PulseChamber);TestGrantModule(CombatModule.DashCapacitor);ModuleDash();for(int i=0;i<BulletCapacity;i++)Bolt(new Vector2(7,0),Vector2.zero,false);
                int before=Ammo,serial=volleySerial,charges=dashModuleShots;bool rejected=!TryFire()&&Ammo==before&&volleySerial==serial&&dashModuleShots==charges;
                TestClearProjectiles();return rejected&&TryFire()&&volleySerial==serial+1&&Mathf.Abs(LastVolleyDamage-GunDamage*1.25f)<.01f;
            }
            if(test==6)
            {
                ApplyPickupEffect(PickupKind.Pierce,false);TestGrantModule(CombatModule.PiercingFragments);TestCounterBegin();Vector2 p=CounterEnginePosition(0);BeginModuleVolley();var b=Bolt(p+Vector2.down*.7f,Vector2.up*10,true);ConfigureModuleBolt(b,4);b.pierce=1;UpdateBullets(.08f);
                int fragments=0;foreach(var child in bullets)if(child.active&&child.friendly&&!child.primary){fragments++;if((child.worldHitMask&(1UL<<60))==0)return false;}return fragments==2&&ModuleFragmentsCreated==2&&CounterLeftHealth==14&&CounterBodyHealth==42;
            }
            if(test==7)
            {
                for(int i=0;i<3;i++){TestClearProjectiles();ModuleProbeShot(ModuleProbeTarget(0,Vector2.zero,.1f),4);}TestGrantModule(CombatModule.AmmoReclaimer);
                for(int i=0;i<2;i++){TestClearProjectiles();ModuleProbeShot(ModuleProbeTarget(0,Vector2.zero,.1f),4);}bool initial=ModuleAmmoReturned==0;TestClearProjectiles();ModuleProbeShot(ModuleProbeTarget(0,Vector2.zero,.1f),4);return initial&&ModuleAmmoReturned==3;
            }
            return false;
        }
        void PrepareModuleProbe(){ResetCombat();ResetModules();loadout.Reset();State=FlightState.Playing;TestFreezeEnemies=true;waveLive=true;Wave=1;transition=1000;playerPosition=new Vector2(0,-4.7f);Ammo=500;Health=MaxHealth;}
        Enemy ModuleProbeTarget(int slot,Vector2 p,float hp){var e=SpawnEnemy(slot,1,p,Mathf.CeilToInt(hp));SetEnemyVariant(e,0);e.position=e.entry=e.target=p;e.renderer.transform.position=p;e.health=e.maxHealth=hp;e.age=2;return e;}
        void ModuleProbeShot(Enemy target,float damage=4)
        {
            BeginModuleVolley();var b=Bolt(target.position+Vector2.down*.7f,Vector2.up*10,true);ConfigureModuleBolt(b,damage);b.pierce=1;UpdateBullets(.08f);
        }
        float ModuleProbeFan(){float max=0;foreach(var b in bullets)if(b.active&&b.friendly)max=Mathf.Max(max,Mathf.Abs(b.velocity.x));return max;}
        public bool TestModuleEffect(int index)
        {
            PrepareModuleProbe();var module=(CombatModule)index;
            if(index==0)
            {
                ApplyPickupEffect(PickupKind.Spread,false);TryFire();float wide=ModuleProbeFan(),damage=LastVolleyDamage;TestClearProjectiles();TestGrantModule(module);TryFire();return Mathf.Abs(ModuleProbeFan()/wide-.6f)<.01f&&Mathf.Abs(LastVolleyDamage/damage-1.15f)<.01f;
            }
            if(index==1)
            {
                ApplyPickupEffect(PickupKind.Pierce,false);TestGrantModule(module);var target=ModuleProbeTarget(0,new Vector2(0,-1),100);ModuleProbeShot(target,3.4f);Debug.Log("ION_MODULE_FRAGMENT_PROBE hp="+target.health+" created="+ModuleFragmentsCreated+" live="+TestFriendlyCount+" mask="+moduleMask+" serial="+volleySerial);return target.health<100&&ModuleFragmentsCreated==2&&TestFriendlyCount==2;
            }
            if(index==2)
            {
                ApplyPickupEffect(PickupKind.Homing,false);TestGrantModule(module);var target=ModuleProbeTarget(0,new Vector2(0,0),100);var secondary=ModuleProbeTarget(1,new Vector2(2,0),100);for(int i=0;i<6;i++){TestClearProjectiles();ModuleProbeShot(target,4);}return ModuleArcsCreated==1&&Mathf.Abs(secondary.health-98)<.01f;
            }
            if(index==3)
            {
                TestGrantModule(module);ModuleDash();float expected=GunDamage;for(int i=0;i<3;i++){TestClearProjectiles();TryFire();if(Mathf.Abs(LastVolleyDamage-expected*1.25f)>.01f)return false;}TryFire();return Mathf.Abs(LastVolleyDamage-expected)<.01f;
            }
            if(index==4)
            {
                TestGrantModule(module);for(int i=0;i<7;i++){TryFire();TestClearProjectiles();}TryFire();return Mathf.Abs(LastVolleyDamage-GunDamage*1.35f)<.01f&&TestPiercingProjectileCount==LastVolleyCount;
            }
            if(index==5)
            {
                TestGrantModule(module);var secondary=ModuleProbeTarget(1,new Vector2(1,0),100);for(int i=0;i<5;i++){TestClearProjectiles();ModuleProbeShot(ModuleProbeTarget(0,Vector2.zero,.1f),4);}return ModuleShockwaves==1&&Mathf.Abs(secondary.health-97)<.01f;
            }
            if(index==6)
            {
                TestGrantModule(module);var near=Bolt(playerPosition+Vector2.right,Vector2.down,false);var far=Bolt(playerPosition+Vector2.right*4,Vector2.down,false);invincible=0;HurtPlayer(5);bool first=!near.active&&far.active&&ModuleArmorPulses==1;near=Bolt(playerPosition+Vector2.right,Vector2.down,false);invincible=0;HurtPlayer(5);return first&&near.active&&ModuleArmorPulses==1&&Health==MaxHealth-10;
            }
            if(index==7)
            {
                playerPosition=new Vector2(0,-2);SpawnCell(new Vector2(3,-2),PickupKind.Ammo);Cell gift=null;foreach(var c in cells)if(c.active){gift=c;break;}if(gift==null)return false;UpdateCells(.01f);float basic=gift.position.x;gift.position=new Vector2(3,-2);TestGrantModule(module);UpdateCells(.01f);return basic==3&&Mathf.Abs(gift.position.x-2.952f)<.001f&&GiftsCollected==0;
            }
            if(index==8)
            {
                TestGrantModule(module);int before=Ammo;for(int i=0;i<33;i++){TestClearProjectiles();ModuleProbeShot(ModuleProbeTarget(0,Vector2.zero,.1f),4);}return ModuleAmmoReturned==30&&Ammo==before+30;
            }
            return false;
        }
    }
}
