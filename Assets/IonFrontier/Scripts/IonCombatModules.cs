using UnityEngine;

namespace IonFrontier
{
    public enum CombatModule { FocusedSpread, PiercingFragments, SeekingArc, DashCapacitor, PulseChamber, AftershockRounds, ReactiveArmor, SalvageMagnet, AmmoReclaimer }
    public sealed partial class IonGame
    {
        static readonly string[] moduleNames={"NÒNG HỘI TỤ","ĐẠN PHÂN MẢNH","SÉT DẪN HƯỚNG","ĐỘNG CƠ PHẢN KÍCH","BUỒNG XUNG ĐIỆN","ĐẠN DƯ CHẤN","GIÁP PHẢN ỨNG","NAM CHÂM TIẾP TẾ","BỘ THU HỒI ĐẠN"};
        static readonly string[] moduleDescriptions={"Góc tỏa còn 60%. Sát thương súng +15%. Đổi độ phủ lấy tập trung.","Viên xuyên giảm 15% sát thương, tạo hai mảnh 30%. Tối đa bốn mảnh mỗi loạt.","Sáu loạt bắn trúng phóng sét sang địch gần, gây 50% sát thương viên kích hoạt.","Sau lướt, ba loạt tiếp trong hai giây gây thêm 25% sát thương.","Mỗi loạt thứ tám mạnh hơn 35% và xuyên thêm một mục tiêu.","Mỗi năm tàu thường do súng hạ tạo vụ nổ nhỏ gây 75% sát thương viên kết liễu.","Mất HP thật sẽ xóa đạn thường trong vùng gần. Hồi tám giây; không hồi máu.","Hút quà xa hơn 60%, nhanh hơn 20%. Không thay tỷ lệ rơi.","Mỗi ba tàu thường do súng hạ hoàn ba đạn. Tối đa 30 đạn mỗi màn."};
        int moduleMask,volleySerial,arcHits,moduleKills,reclaimKills,reclaimedAmmo,dashModuleShots;
        float dashModuleTime,armorModuleClock;
        readonly int[] moduleVolleyIds=new int[128],moduleFragments=new int[128];
        readonly bool[] moduleVolleyHit=new bool[128];
        public int ModuleCount {get{int n=0;for(int i=0;i<9;i++)if((moduleMask&(1<<i))!=0)n++;return n;}}
        public bool HasModule(CombatModule module)=>(moduleMask&(1<<(int)module))!=0;
        public bool ModuleActive(CombatModule module)=>HasModule(module)&&(module!=CombatModule.FocusedSpread||loadout.Has(GunKind.Spread))&&(module!=CombatModule.PiercingFragments||loadout.Has(GunKind.Pierce))&&(module!=CombatModule.SeekingArc||loadout.Has(GunKind.Homing));
        public string ModuleSummary {get{string s="";for(int i=0;i<9;i++)if(HasModule((CombatModule)i))s+=(s==""?"":" / ")+moduleNames[i]+(ModuleActive((CombatModule)i)?"":" (CẦN "+ModuleRequirement((CombatModule)i)+")");return s==""?"CHƯA CÓ MÔ-ĐUN":s;}}
        public static string ModuleName(CombatModule module)=>moduleNames[(int)module];
        public static string ModuleRequirement(CombatModule module)=>module==CombatModule.FocusedSpread?"TỎA":module==CombatModule.PiercingFragments?"XUYÊN":module==CombatModule.SeekingArc?"TRUY ĐUỔI":"";
        public CombatModule EquippedModule(int slot){for(int i=0;i<9;i++)if(HasModule((CombatModule)i)&&slot--==0)return (CombatModule)i;return (CombatModule)(-1);}
        public string ModuleStatus(CombatModule module)
        {
            if(!ModuleActive(module))return "CẦN "+ModuleRequirement(module);
            switch(module){case CombatModule.SeekingArc:return "SÉT "+(arcHits%6)+" / 6 LOẠT TRÚNG";case CombatModule.PulseChamber:return "XUNG "+(volleySerial%8)+" / 8 LOẠT";
            case CombatModule.DashCapacitor:return dashModuleTime>0&&dashModuleShots>0?"CÒN "+dashModuleShots+" LOẠT / "+dashModuleTime.ToString("0.0")+"s":"LƯỚT ĐỂ KÍCH HOẠT";
            case CombatModule.ReactiveArmor:return armorModuleClock>0?"HỒI "+armorModuleClock.ToString("0.0")+"s":"GIÁP SẴN SÀNG";
            case CombatModule.AftershockRounds:return "DƯ CHẤN "+(moduleKills%5)+" / 5 HẠ";case CombatModule.AmmoReclaimer:return reclaimedAmmo>=30?"ĐÃ THU HỒI 30 / 30":"THU HỒI "+reclaimedAmmo+" / 30 • HẠ "+(reclaimKills%3)+" / 3";
            case CombatModule.FocusedSpread:return "GÓC 60% • DAMAGE +15%";case CombatModule.PiercingFragments:return "TỐI ĐA 4 MẢNH / LOẠT";default:return "HÚT XA +60% • NHANH +20%";}
        }
        public int ModuleFragmentsCreated {get;private set;}
        public int ModuleArcsCreated {get;private set;}
        public int ModuleShockwaves {get;private set;}
        public int ModuleArmorPulses {get;private set;}
        public int ModuleAmmoReturned=>reclaimedAmmo;
        float ModuleGunMultiplier=>(ModuleActive(CombatModule.FocusedSpread)?1.15f:1)*(ModuleActive(CombatModule.PiercingFragments)?.85f:1);
        public bool TestGrantModule(CombatModule module){if(HasModule(module)||ModuleCount>=2)return false;moduleMask|=1<<(int)module;return true;}
        void ResetModules()
        {
            moduleMask=volleySerial=arcHits=moduleKills=reclaimKills=reclaimedAmmo=dashModuleShots=0;dashModuleTime=armorModuleClock=moduleArcClock=0;
            ModuleFragmentsCreated=ModuleArcsCreated=ModuleShockwaves=ModuleArmorPulses=0;
            System.Array.Clear(moduleVolleyIds,0,128);System.Array.Clear(moduleFragments,0,128);System.Array.Clear(moduleVolleyHit,0,128);
        }
        void TickModules(float dt){dashModuleTime=Mathf.Max(0,dashModuleTime-dt);armorModuleClock=Mathf.Max(0,armorModuleClock-dt);}
        void ModuleDash(){if(ModuleActive(CombatModule.DashCapacitor)){dashModuleShots=3;dashModuleTime=2;}}
        float BeginModuleVolley()
        {
            volleySerial++;int index=volleySerial%128;moduleVolleyIds[index]=volleySerial;moduleFragments[index]=0;moduleVolleyHit[index]=false;
            float multiplier=1;
            if(ModuleActive(CombatModule.DashCapacitor)&&dashModuleTime>0&&dashModuleShots>0){multiplier*=1.25f;dashModuleShots--;}
            if(ModuleActive(CombatModule.PulseChamber)&&volleySerial%8==0)multiplier*=1.35f;
            return multiplier;
        }
        void ConfigureModuleBolt(Projectile bolt,float damage)
        {
            if(bolt==null)return;bolt.primary=true;bolt.volley=volleySerial;bolt.damage=damage;bolt.fragmentModule=ModuleActive(CombatModule.PiercingFragments);bolt.arcModule=ModuleActive(CombatModule.SeekingArc);bolt.shockModule=ModuleActive(CombatModule.AftershockRounds);bolt.reclaimModule=ModuleActive(CombatModule.AmmoReclaimer);
            bolt.preFragmentDamage=ModuleActive(CombatModule.PiercingFragments)?damage/.85f:damage;
            if(ModuleActive(CombatModule.PulseChamber)&&volleySerial%8==0){bolt.pierce=Mathf.Min(4,bolt.pierce+1);bolt.renderer.color=IonArt.Gold;bolt.renderer.transform.localScale*=1.2f;}
        }
        void OnModuleHit(Projectile bolt,Enemy struck,float acceptedDamage)
        {
            if(bolt==null||!bolt.primary||acceptedDamage<=0)return;int index=bolt.volley%128;
            if(moduleVolleyIds[index]!=bolt.volley)return;
            if(bolt.fragmentModule&&!bolt.fragmented&&moduleFragments[index]<4)
            {
                bolt.fragmented=true;int count=Mathf.Min(2,4-moduleFragments[index]);moduleFragments[index]+=count;
                for(int i=0;i<count;i++)
                {
                    Vector2 direction=Quaternion.Euler(0,0,i==0?-18:18)*bolt.velocity.normalized;
                    var child=Bolt(bolt.position+direction*.2f,direction*20,true,.07f);
                    if(child!=null){child.primary=false;child.damage=bolt.preFragmentDamage*.3f;child.pierce=1;child.homing=false;child.life=1.5f;child.hitMask=1UL<<struck.index;child.worldHitMask=bolt.worldHitMask&((1UL<<60)|(1UL<<61));child.renderer.color=new Color(1,.6f,.9f);child.renderer.transform.localScale=new Vector3(.14f,.36f,1);child.halo.transform.localScale=new Vector3(2,.9f,1);ModuleFragmentsCreated++;}
                }
            }
            if(bolt.arcModule&&!moduleVolleyHit[index])
            {
                moduleVolleyHit[index]=true;arcHits++;
                if(arcHits%6==0)
                {
                    Enemy nearest=null;float range=9;foreach(var e in enemies)if(e.active&&e!=struck&&e.health>0){float d=(e.position-struck.position).sqrMagnitude;if(d<range){range=d;nearest=e;}}
                    if(nearest!=null){EnsureSpecialEffects();DrawLine(lightning[30],struck.position,nearest.position,.065f,IonArt.Gold);moduleArcClock=.16f;ModuleArcsCreated++;DamageEnemy(nearest,bolt.damage*.5f);}
                }
            }
        }
        float moduleArcClock;
        void TickModuleVisuals(float dt){if(moduleArcClock>0){moduleArcClock-=dt;if(moduleArcClock<=0&&lightning[30]!=null)lightning[30].gameObject.SetActive(false);}}
        void OnModuleKill(Projectile bolt,Enemy target)
        {
            if(bolt==null||!bolt.primary||target.kind>=8)return;if(bolt.shockModule)moduleKills++;if(bolt.reclaimModule)reclaimKills++;
            if(bolt.reclaimModule&&reclaimKills%3==0&&reclaimedAmmo<30){int amount=Mathf.Min(3,30-reclaimedAmmo);Ammo=Mathf.Min(600,Ammo+amount);reclaimedAmmo+=amount;}
            if(bolt.shockModule&&moduleKills%5==0)
            {
                ModuleShockwaves++;Explosion(target.position,1.6f);CounterAreaDamage(target.position,1.6f,bolt.damage*.75f);foreach(var other in enemies)if(other.active&&other!=target&&(other.position-target.position).sqrMagnitude<2.56f)DamageEnemy(other,bolt.damage*.75f);
            }
        }
        void OnModuleDamage()
        {
            if(!ModuleActive(CombatModule.ReactiveArmor)||armorModuleClock>0)return;armorModuleClock=8;ModuleArmorPulses++;
            foreach(var b in bullets)if(b.active&&!b.friendly&&(b.position-playerPosition).sqrMagnitude<3.24f)Release(b);Ring(playerPosition,IonArt.Cyan,3.6f,.35f);
        }
    }
}
