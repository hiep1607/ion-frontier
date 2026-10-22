using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace IonFrontier
{
    // Opt-in integration harness. Uses a separate fresh profile and never writes real unlocks.
    public sealed class IonPlaytest : MonoBehaviour
    {
        [Serializable] public sealed class Report
        {
            public bool passed;public string unityVersion;public string[] checks,errors;
            public int naturalKills,naturalAmmoSpent,maxProjectiles,naturalLootDrops,naturalLootRolls,simulatedLootDrops;public float averageFps;
        }
        IonGame game;IonHud hud;string destination;
        readonly List<string> checks=new List<string>(),errors=new List<string>();Report report=new Report();float frameTime;int frames;
        void OnEnable(){Application.logMessageReceived+=Log;}
        void OnDisable(){Application.logMessageReceived-=Log;}
        void Log(string value,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(value);}
        void Update()
        {
            if(game==null)return;
            if(game.TestInput&&game.State==FlightState.Playing)game.TestTarget=game.TestAimTarget();
            if(Time.unscaledDeltaTime>0){frames++;frameTime+=Time.unscaledDeltaTime;}
            report.maxProjectiles=Mathf.Max(report.maxProjectiles,game.ActiveBullets);
        }
        IEnumerator Start()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-wait-focus")>=0)while(!Application.isFocused)yield return null;
            game=GetComponent<IonGame>();game.TestAutoDraft=true;hud=FindFirstObjectByType<IonHud>();
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-ion-captures");destination=index>=0?args[index+1]:Application.persistentDataPath;
            Directory.CreateDirectory(destination);report.unityVersion=Application.unityVersion;game.TestInput=true;
            yield return new WaitForSecondsRealtime(1);yield return Capture("01-hangar-locked.png");
            Check(game.UnlockedHulls==1&&game.UnlockedStages==1,"Fresh profile unlocks exactly one ship and stage");
            Check(!game.SelectHull(1)&&!game.SelectStage(2),"Locked ship and stage cannot be selected through the game API");
            Check(hud.SupportsVietnamese(),"Vietnamese font contains all diacritics");
            Check(IonArt.PlayerShips.Length==8&&IonArt.EnemyShips.Length==10,"All eighteen artwork sprites resolve");
            game.BeginRun();Check(game.Ammo==180&&game.TutorialActive&&!game.AutoFire,"Stage one starts with 180 rounds and protected tutorial");
            yield return Capture("02-tutorial.png");
            float deadline=Time.realtimeSinceStartup+25;
            while(game.TutorialActive&&Time.realtimeSinceStartup<deadline)yield return null;
            Check(!game.TutorialActive&&game.TutorialCompletedMask==15,"Movement, actual target kill, dash and supply pickup finish all tutorial steps");
            game.TestProtection(120);yield return new WaitForSecondsRealtime(2.2f);
            Check(game.Wave==1&&!game.BossActive,"Stage one starts with its first normal wave");
            yield return new WaitForSecondsRealtime(7);
            report.naturalKills=game.Kills;report.naturalAmmoSpent=game.AmmoSpent;
            report.naturalLootDrops=game.EnemyLootDrops;report.naturalLootRolls=game.LootRolls;
            Check(game.Kills>0&&game.AmmoSpent>0,"Real player bolts consume ammunition, hit enemies and score kills");
            yield return Capture("03-combat.png");game.TogglePause();var p=game.PlayerPosition;float elapsed=game.Elapsed;int ammo=game.Ammo;
            yield return new WaitForSecondsRealtime(.4f);Check(game.PlayerPosition==p&&game.Elapsed==elapsed&&game.Ammo==ammo,"Pause freezes movement, combat and ammunition");game.TogglePause();
            game.TestFire=false;game.TestSetAmmo(0);Check(!game.TestFireVolley()&&game.Ammo==0,"Empty ammunition rejects firing and never becomes negative");
            game.TestGivePickup(PickupKind.Ammo);Check(game.Ammo==60&&game.TestFireVolley()&&game.Ammo==59,"Ammo crate restores firing; one volley consumes exactly one round");
            game.TestRemoveInvulnerability();game.HurtPlayer();game.TestGivePickup(PickupKind.Shield);Check(game.Health==game.MaxHealth,"Repair pickup restores HP and respects the selected hull maximum");game.TestProtection(120);
            foreach(var kind in new[]{PickupKind.Spread,PickupKind.Pierce,PickupKind.Homing})
            {
                game.TestGivePickup(kind);GunKind gun=kind==PickupKind.Spread?GunKind.Spread:kind==PickupKind.Pierce?GunKind.Pierce:GunKind.Homing;
                Check(game.Gun==gun,"Weapon pickup equips "+gun);
                game.TestGivePickup(PickupKind.Upgrade);game.TestGivePickup(PickupKind.Upgrade);game.TestGivePickup(PickupKind.Upgrade);
                Check(game.WeaponTier==3,"Weapon upgrade caps at level three for "+gun);
                int before=game.Ammo;game.TestFireVolley();Check(game.Ammo==before-1,"Upgraded "+gun+" still costs one round per volley");
            }
            yield return WeaponFusionChecks();
            game.TestGivePickup(PickupKind.Ultimate);game.TestGivePickup(PickupKind.Ultimate);Check(game.UltimateCharges==1,"Ultimate storage caps at one charge");
            Check(game.UseUltimate()&&game.UltimateCharges==0&&!game.UseUltimate(),"Ultimate consumes exactly one charge; repeated use is rejected");
            game.TestGiftDisplay();yield return new WaitForSecondsRealtime(.3f);yield return Capture("04-gifts.png");
            yield return ClearStage(1);Check(game.State==FlightState.StageClear&&game.UnlockedHulls==2&&game.UnlockedStages==2,"First boss defeat unlocks hull two and stage two");
            yield return Capture("05-first-unlock.png");game.TestReloadProgress();Check(game.ClearedStages==1&&game.ProfileFileExists,"Stage unlock persists and reloads from the separate test profile");
            game.NextStageHangar();Check(game.Sector==2&&game.SelectedHull==1,"Next-stage hangar offers the newly unlocked ship");yield return Capture("06-hangar-unlocked.png");
            game.BeginRun();Check(!game.TutorialActive&&game.Ammo==210,"Stage two skips tutorial and grants 210 rounds");game.TestProtection(120);yield return ClearStage(2);
            Check(game.State==FlightState.StageClear&&game.UnlockedHulls==3&&game.UnlockedStages==3,"Second boss defeat unlocks hull three and stage three");
            game.ReturnToMenu();float expectedDamage=-1;int expectedCount=-1;
            for(int hull=0;hull<3;hull++)
            {
                Check(game.SelectHull(hull),"Unlocked hull "+hull+" can be selected");game.BeginRun();int before=game.Ammo;game.TestFireVolley();
                if(hull==0){expectedDamage=game.LastVolleyDamage;expectedCount=game.LastVolleyCount;}
                Check(Mathf.Abs(game.LastVolleyDamage-expectedDamage*IonHullStats.Damage[hull])<.001f&&game.LastVolleyCount==expectedCount&&game.Ammo==before-1,"Hull "+hull+" applies its hull damage multiplier with unchanged pellet/ammo costs");
                yield return ShipUltimateChecks(hull);game.ReturnToMenu();
            }
            game.SelectStage(3);game.BeginRun();game.TestProtection(120);Check(game.Ammo==240,"Stage three grants 240 starting rounds");
            yield return AdvanceToBoss(3);game.TestFire=false;
            yield return new WaitForSecondsRealtime(4.5f);Check(game.BossPhase==1&&game.BossAttacks>0,"Final boss phase one attacks after all three waves");
            game.TestDamageBoss(230);yield return new WaitForSecondsRealtime(4);Check(game.BossPhase==2,"Final boss escalates below two-thirds hull");
            game.TestDamageBoss(230);yield return new WaitForSecondsRealtime(5);Check(game.BossPhase==3&&game.BossPhaseMask==14,"Final boss executes all three health phases");
            Check(game.BeamCasts>0,"Final boss casts charged heavy lasers");yield return Capture("07-final-boss.png");
            game.TestGivePickup(PickupKind.Ultimate);float fraction=game.BossFraction;game.UseUltimate();yield return new WaitForSecondsRealtime(.5f);Check(game.BossFraction<fraction,"Ultimate damages the final boss rather than only changing visuals");
            game.TestClearWave();Check(game.State==FlightState.BossDying&&game.ClearedStages==2,"Final boss enters destruction cinematic before campaign victory is saved");
            int deathScore=game.Score,deathAmmo=game.Ammo;float deathHealth=game.Health;
            yield return new WaitForSecondsRealtime(.8f);yield return Capture("16-boss-collapse.png");
            game.TogglePause();float deathTime=game.BossDeathTime;yield return new WaitForSecondsRealtime(.4f);
            Check(game.BossDeathTime==deathTime&&game.State==FlightState.Paused,"Pause freezes the boss destruction timeline");game.TogglePause();
            yield return new WaitForSecondsRealtime(1.65f);Check((game.BossDeathEvents&2)!=0,"Dreadnought hull breaks into separate moving sprite fragments");yield return Capture("17-boss-wreckage.png");
            yield return new WaitForSecondsRealtime(.9f);Check((game.BossDeathEvents&4)!=0,"Reactor collapse triggers the delayed final explosion and shockwave");yield return Capture("18-reactor-explosion.png");
            Check(game.Ammo==deathAmmo&&game.Health==deathHealth&&game.Score==deathScore&&!game.UseUltimate(),"Cinematic blocks combat, ammunition spending and duplicate boss rewards");
            yield return new WaitForSecondsRealtime(1.5f);Check(game.State==FlightState.StageClear&&game.ClearedStages==3&&game.UnlockedStages==4&&game.BossDeathEvents==15,"Chapter completion opens stage four only after all boss destruction phases");yield return Capture("08-victory.png");
            game.BeginRun();Check(game.Ammo==240&&game.Health==game.MaxHealth&&game.UltimateCharges==0&&game.WeaponTier==1,"Restart resets supplies, full HP, ultimate and gun upgrades");
            Check(game.Loadout.PatternCount==0&&game.Loadout.DamageStacks==0&&game.Loadout.RateStacks==0&&game.Loadout.MultishotStacks==0,"New stage resets pattern slots and all independent stack counters");
            for(int i=0;i<5;i++){game.TestRemoveInvulnerability();game.HurtPlayer(35);}Check(game.State==FlightState.Defeat&&game.Health==0,"Variable damage defeats the hull when its HP reaches zero");yield return Capture("09-defeat.png");
            game.ReturnToMenu();Check(game.UnlockedHulls==3&&game.UnlockedStages==4&&game.ActiveEnemies==0&&game.ActiveBullets==0,"Returning to hangar releases pools and preserves unlocks");
            Check(report.maxProjectiles<512,"Combat remains below projectile-pool capacity");report.averageFps=frameTime>0?frames/frameTime:0;report.checks=checks.ToArray();report.errors=errors.ToArray();report.passed=errors.Count==0;
            File.WriteAllText(Path.Combine(destination,"playtest-report.json"),JsonUtility.ToJson(report,true));Debug.Log("ION_PLAYTEST_"+(report.passed?"PASS":"FAIL"));yield return new WaitForSecondsRealtime(.3f);Application.Quit(report.passed?0:1);
        }
        IEnumerator ShipUltimateChecks(int hull)
        {
            game.TestFire=false;game.TestSpecialTargets();game.TestClearProjectiles();game.TestRemoveInvulnerability();game.HurtPlayer();
            float hpBefore=game.Health;int ammo=game.Ammo;game.TestGivePickup(PickupKind.Ultimate);
            Check(game.UseUltimate()&&game.LastUltimateHull==hull&&game.UltimateActive&&game.UltimateCharges==0,"Hull "+hull+" starts its own ultimate and consumes one charge");
            game.TestGivePickup(PickupKind.Ultimate);Check(!game.UseUltimate()&&game.UltimateCharges==1,"Hull "+hull+" rejects reactivation while its ultimate is running without spending the stored charge");
            yield return new WaitForSecondsRealtime(.55f);yield return Capture((13+hull)+"-ultimate-hull-"+hull+".png");
            game.TogglePause();float time=game.UltimateElapsed,health=game.TestTargetHealth(0);yield return new WaitForSecondsRealtime(.3f);
            Check(game.UltimateElapsed==time&&game.TestTargetHealth(0)==health,"Hull "+hull+" ultimate pauses both visuals and damage");game.TogglePause();
            game.HurtPlayer();Check(game.Health==(hull==2?Mathf.Min(game.MaxHealth,hpBefore+20):hpBefore),"Hull "+hull+" ultimate protects the pilot; fortress also repairs 20 HP");
            yield return new WaitForSecondsRealtime(hull==2?3.7f:1.2f);
            Check(!game.UltimateActive&&game.Ammo==ammo,"Hull "+hull+" ultimate ends on time and consumes no gun ammo");
            Check(Mathf.Abs(game.TestTargetHealth(0)-110)<.05f&&Mathf.Abs(game.TestTargetHealth(2)-110)<.05f,"Hull "+hull+" ultimate deals 90 actual damage to reachable targets");
            Check(hull==0?Mathf.Abs(game.TestTargetHealth(1)-200)<.05f:Mathf.Abs(game.TestTargetHealth(1)-110)<.05f&&game.UltimatePulseCount==3,
                hull==0?"Celestial cannon pierces aligned targets and leaves off-axis targets intact":"Hull "+hull+" performs exactly three area or chain attacks");
            game.UseUltimate();game.ReturnToMenu();Check(!game.UltimateActive&&!game.BossDeathActive,"Returning to hangar cancels all special effects for hull "+hull);
        }
        IEnumerator WeaponFusionChecks()
        {
            var rng=new System.Random(421);
            for(int i=0;i<10000;i++)if(IonCampaign.RollLoot(rng,out PickupKind kind))report.simulatedLootDrops++;
            Check(report.simulatedLootDrops>1300&&report.simulatedLootDrops<1700,"Ten thousand seeded kill rolls follow the reduced 15 percent loot rate");
            Check(IonArt.GiftIcons.Length==10,"All ten distinct gift icons are present");
            game.TestResetLoadout();
            for(int i=0;i<3;i++)game.TestGivePickup(PickupKind.Damage);
            for(int i=0;i<2;i++)game.TestGivePickup(PickupKind.FireRate);
            for(int i=0;i<3;i++)game.TestGivePickup(PickupKind.Multishot);
            game.TestClearProjectiles();int ammo=game.Ammo;game.TestFireVolley();
            Check(Mathf.Abs(game.LastVolleyDamage-1.6f)<.001f&&Mathf.Abs(game.TestMaxProjectileDamage-1.6f)<.001f,"Three damage stacks increase actual pooled projectile damage by 60 percent");
            Check(Mathf.Abs(game.LastVolleyDelay-.17f/1.2f)<.001f,"Two fire-rate stacks reduce the real firing cooldown");
            Check(game.LastVolleyCount==5&&game.TestFriendlyCount==5&&game.Ammo==ammo-1,"Three multishot stacks produce five real bolts for one ammo");
            game.TestFire=true;int spent=game.AmmoSpent;yield return new WaitForSecondsRealtime(1.2f);game.TestFire=false;
            Check(game.AmmoSpent-spent>=7&&game.AmmoSpent-spent<=10,"Automatic update loop respects the boosted firing cadence");
            game.TestGivePickup(PickupKind.Spread);game.TestGivePickup(PickupKind.Pierce);game.TestClearProjectiles();game.TestFireVolley();
            Check(game.Loadout.PatternCount==2&&game.Loadout.Has(GunKind.Spread)&&game.Loadout.Has(GunKind.Pierce),"Spread and pierce occupy exactly two pattern slots");
            Check(game.TestFriendlyCount==6&&game.TestPiercingProjectileCount==6&&game.TestFanVelocities,"Spread plus pierce makes every fan projectile pierce, including multishot bolts");
            Check(game.Loadout.DamageStacks==3&&game.Loadout.RateStacks==2&&game.Loadout.MultishotStacks==3,"Pattern pickups retain all independent stat stacks");
            game.TestGivePickup(PickupKind.Homing);
            Check(game.Loadout.PatternCount==2&&game.Loadout.Slot(0)==GunKind.Pierce&&game.Loadout.Slot(1)==GunKind.Homing&&!game.Loadout.Has(GunKind.Spread),"Third distinct pattern replaces the oldest slot and never creates a third slot");
            game.TestGivePickup(PickupKind.Homing);Check(game.Loadout.PatternCount==2&&game.Loadout.Level(GunKind.Homing)==2,"Duplicate pattern levels the owned effect instead of occupying another slot");
            game.TestClearProjectiles();game.TestFireVolley();
            Check(game.TestCombinedProjectileCount==game.LastVolleyCount&&game.TestCombinedProjectileCount>0,"Pierce and homing are both enabled on every actual bolt");
            game.TestResetLoadout();game.TestGivePickup(PickupKind.Pierce);game.TestGivePickup(PickupKind.Homing);game.TestGivePickup(PickupKind.Damage);
            game.TestTargets();game.TestFireVolley();yield return new WaitForSecondsRealtime(1.2f);
            Check(game.TestTargetHealth(0)<20&&game.TestTargetHealth(1)<20&&game.TestTargetHealth(2)<20,"Homing piercing projectiles damage three successive real targets");
            float health=game.TestTargetHealth(0);
            Check(Mathf.Abs(health-Mathf.Round(health))>.05f,"Twenty-percent damage bonuses survive collision without integer rounding");
            for(int i=0;i<20;i++){game.TestGivePickup(PickupKind.Damage);game.TestGivePickup(PickupKind.FireRate);game.TestGivePickup(PickupKind.Multishot);}
            Check(game.Loadout.DamageStacks==10&&game.Loadout.RateStacks==10&&game.Loadout.MultishotStacks==8,"Repeatable stat bonuses obey their independent 10/10/8 safety caps");
            game.TestGivePickup(PickupKind.Spread);game.TestGivePickup(PickupKind.Upgrade);game.TestGivePickup(PickupKind.Upgrade);
            Check(game.Loadout.PatternCount==2&&game.Loadout.Has(GunKind.Homing)&&game.Loadout.Has(GunKind.Spread),"A different two-pattern pair still combines after many stat upgrades");
            game.TestSetAmmo(400);game.TestFire=true;yield return new WaitForSecondsRealtime(1.2f);yield return Capture("10-stacked-fusion.png");game.TestFire=false;
            Check(game.ActiveBullets<512&&game.Loadout.ShotCount==15,"Maximum multishot plus spread and homing stays within the projectile pool");
            game.TestGiftDisplay();yield return new WaitForSecondsRealtime(.15f);yield return Capture("11-ten-pickups.png");
            game.TogglePause();hud.ShowHelp();yield return Capture("12-upgrade-guide.png");game.TogglePause();
            game.ReturnToMenu();game.BeginRun();game.TestProtection(120);game.TestFire=false;game.TestSetAmmo(0);game.TestCollectAmmo=true;
            float deadline=Time.realtimeSinceStartup+25;while(game.Ammo==0&&Time.realtimeSinceStartup<deadline)yield return null;
            Check(game.Ammo>0,"Scarcer loot still provides and collects emergency ammo when completely dry");
            game.TestCollectAmmo=false;game.ReturnToMenu();game.BeginRun();game.TestFire=false;game.TestProtection(120);
        }
        IEnumerator AdvanceToBoss(int stage)
        {
            for(int wave=Mathf.Max(1,game.Wave);wave<=3;wave++)
            {
                float deadline=Time.realtimeSinceStartup+6;while(game.Wave<wave&&Time.realtimeSinceStartup<deadline)yield return null;
                Check(game.Sector==stage&&game.Wave==wave&&!game.BossActive,"Stage "+stage+" wave "+wave+" precedes boss");game.TestClearWave();yield return new WaitForSecondsRealtime(.1f);
            }
            float bossDeadline=Time.realtimeSinceStartup+6;while(!game.BossActive&&Time.realtimeSinceStartup<bossDeadline)yield return null;
            Check(game.BossActive&&game.Wave==4&&game.DraftOpenCount==2&&game.DraftChoicesMade==2,"Stage "+stage+" boss spawns last after exactly two resolved checkpoint drafts");
        }
        IEnumerator ClearStage(int stage)
        {
            yield return AdvanceToBoss(stage);yield return new WaitForSecondsRealtime(2);yield return Capture("boss-"+stage+".png");game.TestClearWave();yield return new WaitForSecondsRealtime(5.1f);
            Check(game.BossDestructionsStarted==1&&game.BossDestructionsCompleted==1&&game.BossDeathEvents==15,"Stage "+stage+" boss completes hull breakup and reactor explosion before unlocking");
        }
        IEnumerator Capture(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(destination,name));yield return new WaitForSecondsRealtime(.12f);}
        void Check(bool passed,string description){checks.Add((passed?"PASS: ":"FAIL: ")+description);if(!passed)errors.Add(description);}
    }
}
