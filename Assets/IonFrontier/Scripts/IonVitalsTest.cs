using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace IonFrontier
{
    public sealed class IonVitalsTest : MonoBehaviour
    {
        [Serializable] public sealed class Report {public bool passed;public string[] checks,errors;public float averageFps;public int maxProjectiles;}
        readonly List<string> checks=new List<string>(),errors=new List<string>();
        IonGame game;string destination;float elapsed;int frames,maxBolts;
        void OnEnable(){Application.logMessageReceived+=Log;}
        void OnDisable(){Application.logMessageReceived-=Log;}
        void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(text);}
        void Update(){if(Time.unscaledDeltaTime>0){frames++;elapsed+=Time.unscaledDeltaTime;}if(game!=null)maxBolts=Mathf.Max(maxBolts,game.ActiveBullets);}
        IEnumerator Start()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-ion-wait-focus")>=0)while(!Application.isFocused)yield return null;
            game=GetComponent<IonGame>();game.TestAutoDraft=true;game.TestInput=true;game.TestFire=false;
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-ion-captures");destination=at>=0?args[at+1]:Application.persistentDataPath;Directory.CreateDirectory(destination);
            yield return new WaitForSecondsRealtime(.5f);game.TestSetProgress(13);game.SelectStage(4);
            var signatures=new HashSet<string>();float slowDistance=0,fastDistance=0;
            for(int hull=0;hull<8;hull++)
            {
                game.ReturnToMenu();game.SelectHull(hull);yield return Capture("hull-"+hull+"-stats.png");game.BeginRun();game.TestFire=false;game.TestProtection(10000);
                Check(game.Health==IonHullStats.Health[hull]&&game.MaxHealth==game.Health,"Hull "+hull+" starts with its own full HP capacity");
                signatures.Add(IonHullStats.Summary(hull));int ammo=game.Ammo;game.TestFireVolley();
                Check(Mathf.Abs(game.LastVolleyDamage-game.TestMaxProjectileDamage)<.001f&&Mathf.Abs(game.LastVolleyDamage-IonHullStats.Damage[hull])<.001f,"Hull "+hull+" multiplier reaches actual friendly projectile damage");
                Check(Mathf.Abs(game.LastVolleyDelay-.17f/IonHullStats.Rate[hull])<.001f&&game.Ammo==ammo-1,"Hull "+hull+" has distinct fire delay with one-round volley cost");
                game.TestGivePickup(PickupKind.Damage);game.TestGivePickup(PickupKind.FireRate);game.TestClearProjectiles();game.TestFireVolley();
                Check(Mathf.Abs(game.LastVolleyDamage-IonHullStats.Damage[hull]*1.2f)<.001f&&Mathf.Abs(game.LastVolleyDelay-.17f/(1.1f*IonHullStats.Rate[hull]))<.001f,"Hull "+hull+" combines hull and pickup multipliers without replacing either");
                Vector2 start=game.PlayerPosition;game.TestTarget=start+Vector2.right*10;yield return new WaitForSecondsRealtime(.5f);float distance=(game.PlayerPosition-start).magnitude;
                if(hull==2)slowDistance=distance;if(hull==7)fastDistance=distance;
                game.TestRemoveInvulnerability();game.HurtPlayer(7.5f);float afterLight=game.Health;game.TestRemoveInvulnerability();game.HurtPlayer(23.25f);
                Check(Mathf.Abs(afterLight-(game.MaxHealth-7.5f))<.01f&&Mathf.Abs(game.Health-(game.MaxHealth-30.75f))<.01f,"Hull "+hull+" subtracts fractional damage instead of counting hits");
                game.TestGivePickup(PickupKind.Shield);Check(Mathf.Abs(game.Health-(game.MaxHealth-.75f))<.01f,"Repair kit restores exactly 30 HP for hull "+hull);
                game.HealPlayer(999);Check(game.Health==game.MaxHealth,"Repair cannot exceed hull "+hull+" maximum HP");
            }
            Check(signatures.Count==8&&fastDistance>slowDistance*1.15f,"All hull profiles differ and faster craft travels farther in the real movement loop");
            game.ReturnToMenu();game.SelectHull(0);game.BeginRun();game.TestFire=false;game.TestTargets();game.TestHitSlot(0,.75f);
            Check(game.HasAttackTarget&&!game.AttackTargetIsBoss&&Mathf.Abs(game.AttackTargetHealth-19.25f)<.001f,"Regular enemy inspection shows exact HP and matching hull portrait");
            yield return Capture("target-regular-exact-hp.png");game.TestProtection(10000);game.TogglePause();float targetHp=game.AttackTargetHealth;yield return new WaitForSecondsRealtime(.3f);
            Check(game.AttackTargetHealth==targetHp&&game.HasAttackTarget,"Target details remain stable during pause");game.TogglePause();
            game.TestHitSlot(0,100);Check(game.AttackTargetHealth==0&&game.AttackTargetState=="ĐÃ HẠ","A defeated target displays zero HP and defeated state");
            game.TestReuseInspectedSlot();Check(!game.HasAttackTarget,"Reusing a pooled enemy slot cannot silently replace the inspected target");
            game.ReturnToMenu();game.SelectStage(5);game.BeginRun();game.TestProtection(10000);game.TestStartBossEncounter();yield return new WaitForSecondsRealtime(2.2f);
            game.TestHitSlot(0,10);Check(game.AttackTargetIsBoss&&game.AttackTargetHealth==game.AttackTargetMaxHealth&&game.AttackTargetState.Contains("KHIÊN"),"A shielded boss is inspected even when the hit deals zero damage");
            yield return Capture("target-boss-shielded.png");game.TestHitSlot(1,12.5f);
            Check(game.AttackTargetName=="KÌM HÃM"&&game.BossBarName=="KÌM HÃM"&&Mathf.Abs(game.AttackTargetHealth-137.5f)<.1f,"Target panel and single boss bar follow the attacked partner with exact HP");
            yield return Capture("target-boss-partner.png");game.ReturnToMenu();Check(!game.HasAttackTarget&&game.PendingBossWrecks==0,"Returning to hangar clears target inspection and combat effects");
            game.SelectStage(4);game.BeginRun();game.TestFire=false;game.TestTargets();game.TestProtection(0);game.TestRemoveInvulnerability();float hp=game.Health;
            game.TestIncomingShot(.12f);yield return new WaitForSecondsRealtime(.2f);float lightLoss=hp-game.Health;game.TestRemoveInvulnerability();hp=game.Health;
            game.TestIncomingShot(.23f);yield return new WaitForSecondsRealtime(.2f);float heavyLoss=hp-game.Health;
            Check(lightLoss>0&&heavyLoss>lightLoss*1.5f,"Actual hostile pooled bullets deliver distinct light/heavy HP damage");
            game.HealPlayer(999);game.TestRemoveInvulnerability();hp=game.Health;game.HurtPlayer(40*.25f,true);game.HurtPlayer(40*.25f,true);
            Check(Mathf.Abs(game.Health-(hp-20))<.01f,"Continuous beam damage integrates duration without hit-count cooldown");
            game.TogglePause();hp=game.Health;game.HurtPlayer(999);Check(game.Health==hp,"Paused gameplay rejects damage");game.TogglePause();
            game.TestRemoveInvulnerability();game.HurtPlayer(.125f);Check(game.Health%1!=0,"Fractional HP is preserved internally");
            game.TestRemoveInvulnerability();game.HurtPlayer(10000);Check(game.Health==0&&game.State==FlightState.Defeat,"Lethal damage clamps HP to zero and ends the run once");
            game.ReturnToMenu();game.BeginRun();Check(game.Health==game.MaxHealth&&!game.HasAttackTarget,"Replay restores the selected hull HP and clears prior target state");
            game.TestPropsDisplay();yield return Capture("finished-mission-props.png");
            Check(IonArt.MissionProps.Length==7&&new HashSet<Sprite>(IonArt.MissionProps).Count==7,"Seven finished mission prop models resolve as distinct sprites");
            var report=new Report{passed=errors.Count==0,checks=checks.ToArray(),errors=errors.ToArray(),averageFps=elapsed>0?frames/elapsed:0,maxProjectiles=maxBolts};
            File.WriteAllText(Path.Combine(destination,"vitals-report.json"),JsonUtility.ToJson(report,true));Debug.Log("ION_VITALS_TEST_"+(report.passed?"PASS":"FAIL"));yield return new WaitForSecondsRealtime(.2f);Application.Quit(report.passed?0:1);
        }
        IEnumerator Capture(string name){yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(destination,name));yield return new WaitForSecondsRealtime(.1f);}
        void Check(bool pass,string text){checks.Add((pass?"PASS: ":"FAIL: ")+text);if(!pass){errors.Add(text);Debug.LogWarning("VITALS_FAIL: "+text);}}
    }
    public sealed partial class IonGame
    {
        public void TestHitSlot(int slot,float damage){targetAge=1;DamageEnemy(enemies[slot],damage);}
        public void TestReuseInspectedSlot(){SpawnEnemy(0,1,new Vector2(0,3),20);}
        public void TestStartBossEncounter(){ResetCombat();EnsureMissionObjects();State=FlightState.Playing;SpawnExpandedBoss();}
        public void TestIncomingShot(float radius){var b=Bolt(playerPosition+Vector2.up*.6f,Vector2.down*12,false,radius);if(b!=null)b.damage=HostileShotDamage(radius);}
        public void TestPropsDisplay()
        {
            ResetCombat();TestFreezeEnemies=true;EnsureMissionObjects();waveLive=true;transition=1000;
            var kinds=new[]{WorldKind.Rock,WorldKind.Mine,WorldKind.Generator,WorldKind.Beacon,WorldKind.Zone,WorldKind.Convoy,WorldKind.Tether};
            for(int i=0;i<7;i++)SpawnWorld(kinds[i],new Vector2((i%4-1.5f)*5,i<4?1.5f:-2.5f),100,i==4?1.5f:1);
        }
    }
}
