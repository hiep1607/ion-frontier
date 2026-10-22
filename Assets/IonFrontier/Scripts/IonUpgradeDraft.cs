using System;
using System.Collections.Generic;
using UnityEngine;

namespace IonFrontier
{
    public sealed partial class IonGame
    {
        sealed class DraftCard {public int module=-1,pointValue;public PickupKind pickup;public bool points;public string title,description;}
        readonly DraftCard[] draftCards=new DraftCard[3];
        System.Random draftRandom;int draftConsumed;float draftResume;bool draftWaitRelease;
        public bool TestAutoDraft;
        public int TestDraftSeed;
        public int DraftOpenCount {get;private set;}
        public int DraftChoicesMade {get;private set;}
        public int DraftSelected {get;private set;}=-1;
        public bool DraftActive=>State==FlightState.UpgradeChoice||State==FlightState.Paused&&pausedFrom==FlightState.UpgradeChoice;
        public string DraftCardTitle(int index)=>index>=0&&index<3&&draftCards[index]!=null?draftCards[index].title:"";
        public string DraftCardDescription(int index)=>index>=0&&index<3&&draftCards[index]!=null?draftCards[index].description:"";
        public Sprite DraftCardIcon(int index)=>draftCards[index]!=null&&draftCards[index].module<0&&!draftCards[index].points?IonArt.GiftIcons[(int)draftCards[index].pickup]:IonArt.GiftIcons[(int)PickupKind.Upgrade];
        public int DraftCardModule(int index)=>index>=0&&index<3&&draftCards[index]!=null?draftCards[index].module:-1;
        public string DraftResumeText=>DraftSelected<0?"CHỌN BẰNG CHUỘT HOẶC PHÍM 1 / 2 / 3":"ĐÃ NHẬN / TIẾP TỤC SAU "+Mathf.Max(0,draftResume).ToString("0.0")+"s";
        void ResetDraft(){draftConsumed=0;DraftOpenCount=DraftChoicesMade=0;DraftSelected=-1;draftResume=0;draftWaitRelease=false;int seed=TestDraftSeed!=0?TestDraftSeed:Environment.TickCount^Sector*7919;draftRandom=new System.Random(seed);Debug.Log("ION_DRAFT_SEED: "+seed+"; sector="+Sector);for(int i=0;i<3;i++)draftCards[i]=null;}
        void CancelDraft(){DraftSelected=-1;draftResume=0;draftWaitRelease=false;}
        public void TestOpenDraft(int checkpoint){RequestUpgradeChoice(checkpoint);}
        void RequestUpgradeChoice(int checkpoint)
        {
            if(State!=FlightState.Playing||TutorialActive||RoutePending||checkpoint<0||checkpoint>1||(draftConsumed&(1<<checkpoint))!=0)return;
            draftConsumed|=1<<checkpoint;DraftOpenCount++;DraftSelected=-1;GenerateDraft();State=FlightState.UpgradeChoice;hud.ShowBanner(false,Sector,SectorName);hud.Refresh();
            if(TestAutoDraft)TestResolveDraft();
        }
        DraftCard PickupCard(PickupKind kind)
        {
            string detail=kind==PickupKind.Ammo?"Nhận 60 đạn kho, tối đa 600.":kind==PickupKind.Shield?"Sửa 30 HP cho phi cơ và 15 HP cho vận tải nếu có.":kind==PickupKind.Upgrade?"Nâng cấp các kiểu bắn đang dùng, tối đa cấp 3.":kind==PickupKind.Damage?"Thêm 20% sát thương cơ bản. Cộng dồn tối đa 10 lần.":kind==PickupKind.FireRate?"Thêm 10% tốc độ bắn cơ bản. Cộng dồn tối đa 10 lần.":kind==PickupKind.Multishot?"Thêm một viên mỗi loạt, vẫn tốn một đạn kho. Tối đa tám cấp.":"Thêm hoặc nâng kiểu bắn, kèm 20 đạn.";
            if(kind>=PickupKind.Spread&&kind<=PickupKind.Homing&&loadout.PatternCount==2&&!loadout.Has((GunKind)((int)kind-2)))detail+=" Thay "+IonCampaign.GunNames[(int)loadout.Slot(0)]+" trong ô cũ nhất.";
            if(kind==PickupKind.Upgrade||kind>=PickupKind.Spread&&kind!=PickupKind.Ultimate)
            {
                var preview=loadout.Copy();if(kind==PickupKind.Upgrade)preview.UpgradePatterns();else if(kind>=PickupKind.Damage)preview.AddStat(kind);else preview.AddPattern((GunKind)((int)kind-2));
                float multiplier=(HasModule(CombatModule.FocusedSpread)&&preview.Has(GunKind.Spread)?1.15f:1)*(HasModule(CombatModule.PiercingFragments)&&preview.Has(GunKind.Pierce)?.85f:1);
                detail+="\nSM "+GunDamage.ToString("0.0")+" → "+(preview.Damage*IonHullStats.Damage[SelectedHull]*multiplier).ToString("0.0")+" • NHỊP "+(1/GunDelay).ToString("0.0")+" → "+(IonHullStats.Rate[SelectedHull]/preview.FireDelay).ToString("0.0")+"/s\nVIÊN "+loadout.ShotCount+" → "+preview.ShotCount;
                for(int i=0;i<3;i++)if(ModuleActive((CombatModule)i)&&!preview.Has((GunKind)(i+1)))detail+="\n"+moduleNames[i]+" SẼ NGỪNG.";
            }
            return new DraftCard{pickup=kind,title=IonCampaign.GiftNames[(int)kind],description=detail};
        }
        bool ValidPickup(PickupKind kind)
        {
            if(kind==PickupKind.Ammo)return Ammo<600;if(kind==PickupKind.Shield)return Health<MaxHealth||MissionConvoyActive&&ConvoyHealth<100;
            if(kind==PickupKind.Damage)return loadout.DamageStacks<10;if(kind==PickupKind.FireRate)return loadout.RateStacks<10;if(kind==PickupKind.Multishot)return loadout.MultishotStacks<8;
            if(kind==PickupKind.Upgrade){if(loadout.PatternCount==0)return loadout.Level(GunKind.Ion)<3;for(int i=0;i<loadout.PatternCount;i++)if(loadout.Level(loadout.Slot(i))<3)return true;return false;}
            GunKind gun=(GunKind)((int)kind-2);return !loadout.Has(gun)||loadout.Level(gun)<3;
        }
        void GenerateDraft()
        {
            var weapons=new List<DraftCard>();foreach(var p in new[]{PickupKind.Spread,PickupKind.Pierce,PickupKind.Homing,PickupKind.Upgrade,PickupKind.Damage,PickupKind.FireRate,PickupKind.Multishot})if(ValidPickup(p))weapons.Add(PickupCard(p));
            var modules=new List<DraftCard>();if(ModuleCount<2)for(int i=0;i<9;i++)
            {
                if(HasModule((CombatModule)i)||i==0&&!loadout.Has(GunKind.Spread)||i==1&&!loadout.Has(GunKind.Pierce)||i==2&&!loadout.Has(GunKind.Homing))continue;
                modules.Add(new DraftCard{module=i,title=moduleNames[i],description=moduleDescriptions[i]+(i<3?" Cần "+IonCampaign.GunNames[i+1]+" đang trang bị.":"")});
            }
            draftCards[0]=weapons.Count>0?weapons[draftRandom.Next(weapons.Count)]:modules.Count>0?modules[0]:new DraftCard{points=true,pointValue=500,title="CHIẾN CÔNG +500",description="Kho vũ khí đã đầy. Nhận 500 điểm."};
            draftCards[1]=modules.Count>0?modules[draftRandom.Next(modules.Count)]:null;
            if(HealthFraction<.6f)draftCards[2]=PickupCard(PickupKind.Shield);else if(Ammo<60)draftCards[2]=PickupCard(PickupKind.Ammo);else draftCards[2]=null;
            var fallback=new List<DraftCard>();fallback.AddRange(modules);fallback.AddRange(weapons);if(ValidPickup(PickupKind.Ammo))fallback.Add(PickupCard(PickupKind.Ammo));if(ValidPickup(PickupKind.Shield))fallback.Add(PickupCard(PickupKind.Shield));
            for(int slot=1;slot<3;slot++)
            {
                if(draftCards[slot]!=null&&draftCards[slot].title!=draftCards[0].title&&(slot==1||draftCards[slot].title!=draftCards[1].title))continue;
                var candidates=fallback.FindAll(c=>c.title!=draftCards[0].title&&(slot==1||c.title!=draftCards[1].title));
                draftCards[slot]=candidates.Count>0?candidates[draftRandom.Next(candidates.Count)]:new DraftCard{points=true,pointValue=300+slot*100,title="THÀNH TÍCH +"+(300+slot*100),description="Nhận "+(300+slot*100)+" điểm khi các nâng cấp đã đầy."};
            }
        }
        public bool ChooseUpgrade(int index)
        {
            if(State!=FlightState.UpgradeChoice||DraftSelected>=0||index<0||index>2)return false;var card=draftCards[index];if(card==null)return false;
            DraftSelected=index;DraftChoicesMade++;if(card.module>=0)TestGrantModule((CombatModule)card.module);else if(card.points)Score+=card.pointValue;else ApplyPickupEffect(card.pickup,false);
            draftResume=.5f;draftWaitRelease=true;Play(IonArt.Pickup);hud.Refresh();return true;
        }
        public void TestResolveDraft(int choice=0){if(State!=FlightState.UpgradeChoice)return;if(DraftSelected<0)ChooseUpgrade(choice);if(DraftSelected>=0){draftResume=0;draftWaitRelease=false;ResumeDraft();}}
        void ResumeDraft(){State=FlightState.Playing;invincible=Mathf.Max(invincible,.5f);lastMouse=Input.mousePosition;fireClock=Mathf.Max(fireClock,.15f);hud.Refresh();}
        void TickDraft(float dt)
        {
            if(DraftSelected<0){if(Input.GetKeyDown(KeyCode.Alpha1))ChooseUpgrade(0);else if(Input.GetKeyDown(KeyCode.Alpha2))ChooseUpgrade(1);else if(Input.GetKeyDown(KeyCode.Alpha3))ChooseUpgrade(2);}
            else {draftResume=Mathf.Max(0,draftResume-dt);if(!Input.GetMouseButton(0))draftWaitRelease=false;if(draftResume<=0&&!draftWaitRelease)ResumeDraft();}
            hud.TickDraftInterface();
        }
    }
}
