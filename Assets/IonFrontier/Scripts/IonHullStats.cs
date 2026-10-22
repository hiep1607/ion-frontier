using UnityEngine;

namespace IonFrontier
{
    public static class IonHullStats
    {
        public static readonly float[] Health={100,90,130,110,95,120,100,85};
        public static readonly float[] Speed={1,1.14f,.88f,.96f,1.10f,.94f,1.05f,1.16f};
        public static readonly float[] Damage={1,.92f,1.15f,1.12f,.90f,1.05f,.98f,1.10f};
        public static readonly float[] Rate={1,1.12f,.90f,.96f,1.16f,.96f,1.08f,.92f};
        public static readonly string[] Roles={"CÂN BẰNG","TRINH SÁT CƠ ĐỘNG","THIẾT GIÁP HẠNG NẶNG","HỎA LỰC ĐỘT KÍCH","LIÊN THANH LINH HOẠT","HỘ VỆ BỀN BỈ","YỂM TRỢ CHÍNH XÁC","ĐÁNH NHANH RÚT GỌN"};
        public static string Summary(int hull)=>"HP "+Health[hull]+"  /  TỐC "+(10.5f*Speed[hull]).ToString("0.0")+"\nSM ×"+Damage[hull].ToString("0.00")+"  /  NHỊP ×"+Rate[hull].ToString("0.00");
    }
    public sealed partial class IonGame
    {
        public float Health {get;private set;}=100;
        public float MaxHealth=>IonHullStats.Health[SelectedHull];
        public float HealthFraction=>Mathf.Clamp01(Health/MaxHealth);
        public float MoveSpeed=>10.5f*IonHullStats.Speed[SelectedHull];
        public float GunDamage=>loadout.Damage*IonHullStats.Damage[SelectedHull]*ModuleGunMultiplier;
        public float GunDelay=>loadout.FireDelay/IonHullStats.Rate[SelectedHull];
        float damageFeedbackClock,damageContactClock;
        Enemy inspectedTarget;int inspectedSerial;float targetAge;
        bool targetBlocked;
        bool HasEnemyAttackTarget=>inspectedTarget!=null&&inspectedTarget.serial==inspectedSerial&&targetAge<8;
        public bool HasAttackTarget=>CounterTargetActive||HasEnemyAttackTarget;
        public Sprite AttackTargetSprite=>CounterTargetActive?CounterTargetSprite:HasEnemyAttackTarget?inspectedTarget.renderer.sprite:null;
        public float AttackTargetHealth=>CounterTargetActive?CounterTargetHealth:HasEnemyAttackTarget?Mathf.Max(0,inspectedTarget.health):0;
        public float AttackTargetMaxHealth=>CounterTargetActive?CounterTargetMaxHealth:HasEnemyAttackTarget?inspectedTarget.maxHealth:0;
        public float AttackTargetFraction=>AttackTargetMaxHealth>0?AttackTargetHealth/AttackTargetMaxHealth:0;
        public bool AttackTargetIsBoss=>!CounterTargetActive&&HasEnemyAttackTarget&&inspectedTarget.kind>=8;
        Enemy DisplayedBoss=>AttackTargetIsBoss&&inspectedTarget.active?inspectedTarget:boss;
        public float BossBarHealth=>DisplayedBoss!=null?Mathf.Max(0,DisplayedBoss.health):0;
        public float BossBarMaxHealth=>DisplayedBoss!=null?DisplayedBoss.maxHealth:1;
        public bool BossBarShielded=>DisplayedBoss!=null&&!BossCanTakeDamage(DisplayedBoss);
        public string BossBarName=>DisplayedBoss==null?BossName:DisplayedBoss.bossArt==13?"KÌM HÃM":DisplayedBoss.bossArt==14?"LÕI VỆ":DisplayedBoss.bossArt==15?"SONG SÁT LAM":IonCampaign.BossNames[DisplayedBoss.bossArt];
        public string AttackTargetName=>CounterTargetActive?CounterTargetName:!HasEnemyAttackTarget?"CHƯA KHÓA MỤC TIÊU":inspectedTarget.kind<8?IonHostileFleet.Names[inspectedTarget.variant]:inspectedTarget.bossArt==13?"KÌM HÃM":inspectedTarget.bossArt==14?"LÕI VỆ":inspectedTarget.bossArt==15?"SONG SÁT LAM":IonCampaign.BossNames[inspectedTarget.bossArt];
        public string AttackTargetState=>CounterTargetActive?CounterTargetState:!HasEnemyAttackTarget?"Bắn trúng để xem thông tin":!inspectedTarget.active?"ĐÃ HẠ":inspectedTarget.kind>=8&&!BossCanTakeDamage(inspectedTarget)?"KHIÊN ĐÓNG / ĐANG CHẶN SÁT THƯƠNG":targetBlocked?"ĐANG ẨN / KHÁNG SÁT THƯƠNG":inspectedTarget.kind>=8?"BOSS / LÕI MỞ":"ĐỊCH THƯỜNG / ĐANG GIAO CHIẾN";
        void TrackAttackTarget(Enemy target,bool blocked)
        {
            if(!CounterOwns(target))ClearCounterTarget();
            if(!HasEnemyAttackTarget||!inspectedTarget.active||inspectedTarget==target||targetAge>.35f)
            {inspectedTarget=target;inspectedSerial=target.serial;targetAge=0;targetBlocked=blocked;}
        }
        void UpdateVitals(float dt){targetAge+=dt;damageFeedbackClock=Mathf.Max(0,damageFeedbackClock-dt);damageContactClock=Mathf.Max(0,damageContactClock-dt);}
        void ClearAttackTarget(){ClearCounterTarget();inspectedTarget=null;targetAge=0;targetBlocked=false;}
        public void HealPlayer(float amount){Health=Mathf.Min(MaxHealth,Health+Mathf.Max(0,amount));}
        public void HurtPlayer(float amount=18,bool continuous=false,string source="TIẾP XÚC")
        {
            if(State!=FlightState.Playing||invincible>0||amount<=0)return;
            float actual=Mathf.Min(Health,amount);DamageTaken+=actual;RecordDamageSource(source,actual);Health=Mathf.Max(0,Health-amount);OnModuleDamage();
            if(!continuous)invincible=.55f;
            if(!continuous||damageContactClock<=0){playerHits++;damageContactClock=.8f;}
            if(damageFeedbackClock<=0)
            {
                damageFeedbackClock=.15f;shake=.12f;Explosion(playerPosition,.5f);Ring(playerPosition,IonArt.Cyan,1.6f,.3f);Play(IonArt.Blast,.8f);hud.FlashDamage();
            }
            if(Health<=0)Finish(false);
        }
        float HostileShotDamage(float radius)=>(radius>=.2f?24:radius>=.17f?18:radius>=.145f?14:10)*(Sector<=2?.8f:1);
    }
}
