using UnityEngine;

namespace IonFrontier
{
    // Two pattern slots and independent repeatable stat bonuses, shared by every player hull.
    public sealed class IonLoadout
    {
        public const int MaxDamageStacks=10,MaxRateStacks=10,MaxMultishotStacks=8;
        readonly GunKind[] patterns=new GunKind[2];
        readonly int[] levels={1,1,1,1};
        public int PatternCount { get; private set; }
        public int DamageStacks { get; private set; }
        public int RateStacks { get; private set; }
        public int MultishotStacks { get; private set; }
        public GunKind Latest => PatternCount==0?GunKind.Ion:patterns[PatternCount-1];
        public GunKind Visual => Has(GunKind.Pierce)?GunKind.Pierce:Has(GunKind.Homing)?GunKind.Homing:Has(GunKind.Spread)?GunKind.Spread:GunKind.Ion;
        public float DamageMultiplier => 1+.2f*DamageStacks;
        public float RateMultiplier => 1+.1f*RateStacks;
        public int PierceCount => Has(GunKind.Pierce)?3:1;
        public float FireDelay
        {
            get {float delay=IonCampaign.FireDelay(GunKind.Ion,levels[0]);for(int i=0;i<PatternCount;i++)delay=Mathf.Max(delay,IonCampaign.FireDelay(patterns[i],Level(patterns[i])));return delay/RateMultiplier;}
        }
        public float Damage
        {
            get {int value=IonCampaign.ShotDamage(GunKind.Ion,levels[0]);for(int i=0;i<PatternCount;i++)value=Mathf.Max(value,IonCampaign.ShotDamage(patterns[i],Level(patterns[i])));return value*DamageMultiplier;}
        }
        public int ShotCount
        {
            get {int count=IonCampaign.ShotCount(GunKind.Ion,levels[0]);for(int i=0;i<PatternCount;i++)count=Mathf.Max(count,IonCampaign.ShotCount(patterns[i],Level(patterns[i])));return count+MultishotStacks;}
        }
        public int Level(GunKind gun)=>levels[(int)gun];
        public GunKind Slot(int index)=>index>=0&&index<PatternCount?patterns[index]:GunKind.Ion;
        public bool Has(GunKind gun){for(int i=0;i<PatternCount;i++)if(patterns[i]==gun)return true;return false;}
        public string Summary
        {
            get
            {
                if(PatternCount==0)return "ION KÉP "+levels[0];
                string first=IonCampaign.GunNames[(int)patterns[0]]+" "+Level(patterns[0]);
                return PatternCount==1?first:first+" + "+IonCampaign.GunNames[(int)patterns[1]]+" "+Level(patterns[1]);
            }
        }
        // Repeated patterns level that effect; a third distinct pattern replaces the oldest slot.
        public string AddPattern(GunKind gun)
        {
            if(gun==GunKind.Ion)return "";
            if(Has(gun)){bool raised=levels[(int)gun]<3;levels[(int)gun]=Mathf.Min(3,levels[(int)gun]+1);return raised?"NÂNG "+IonCampaign.GunNames[(int)gun]:"KIỂU BẮN ĐÃ TỐI ĐA";}
            string replaced="";
            if(PatternCount==2){replaced=IonCampaign.GunNames[(int)patterns[0]];patterns[0]=patterns[1];patterns[1]=gun;}
            else patterns[PatternCount++]=gun;
            return replaced==""?"GHÉP "+IonCampaign.GunNames[(int)gun]:"THAY "+replaced+" BẰNG "+IonCampaign.GunNames[(int)gun];
        }
        public bool UpgradePatterns()
        {
            bool changed=false;
            if(PatternCount==0){changed=levels[0]<3;levels[0]=Mathf.Min(3,levels[0]+1);}
            else for(int i=0;i<PatternCount;i++){int index=(int)patterns[i];changed|=levels[index]<3;levels[index]=Mathf.Min(3,levels[index]+1);}
            return changed;
        }
        public bool AddStat(PickupKind kind)
        {
            if(kind==PickupKind.Damage&&DamageStacks<MaxDamageStacks){DamageStacks++;return true;}
            if(kind==PickupKind.FireRate&&RateStacks<MaxRateStacks){RateStacks++;return true;}
            if(kind==PickupKind.Multishot&&MultishotStacks<MaxMultishotStacks){MultishotStacks++;return true;}
            return false;
        }
        public void Reset(){PatternCount=DamageStacks=RateStacks=MultishotStacks=0;for(int i=0;i<4;i++)levels[i]=1;}
        public IonLoadout Copy(){var value=new IonLoadout{PatternCount=PatternCount,DamageStacks=DamageStacks,RateStacks=RateStacks,MultishotStacks=MultishotStacks};System.Array.Copy(patterns,value.patterns,2);System.Array.Copy(levels,value.levels,4);return value;}
    }
}
