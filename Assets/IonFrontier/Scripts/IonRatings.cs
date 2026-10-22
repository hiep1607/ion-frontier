using UnityEngine;

namespace IonFrontier
{
    public sealed partial class IonGame
    {
        public const int MaximumStars=5;
        public float DamageTaken {get;private set;}
        public float BossCombatSeconds {get;private set;}
        public int LastRatingMask {get;private set;}
        public bool LastWasNewRating {get;private set;}
        public readonly string[] LastRatingDetails=new string[MaximumStars];
        bool ratingBossSeen;
        public float BossParSeconds=>Sector==13?150:Sector==3||Sector==5||Sector==9||Sector==11||Sector==12?105:75;
        public bool RatingSecondaryMet=>Sector<=3?playerHits<=2:OptionalGoal;
        public string RatingSecondaryGoal=>Sector<=3?"Trúng không quá 2 đòn":Sector==4?"Phá 18 thiên thạch bằng hỏa lực":Sector==5?"Vận tải còn ít nhất 70% HP":Sector==6?"Xong nhiệm vụ trước boss trong 95s":Sector==7?"Bám tín hiệu đủ 35s":Sector==8?"Cứu đủ 4 trinh sát":Sector==9?"Trúng không quá 2 đòn":Sector==10?"Phá ít nhất 15 mìn":Sector==11?(route==1?"Vận tải còn ít nhất 70% HP":route==2?"Cứu đủ 4 trinh sát":"E: vận tải ≥70% / R: cứu đủ 4"):Sector==12?"Trúng dưới 3 đòn":"Xong nhiệm vụ trước boss trong 120s";
        public string RatingCriterionName(int index)=>index==0?"HOÀN THÀNH NHIỆM VỤ":index==1?"BẢO TOÀN PHI CƠ":index==2?"NÉ TRÁNH HIỆU QUẢ":index==3?"MỤC TIÊU PHỤ":"HẠ BOSS NHANH";
        public string RatingRequirement(int index)=>index==0?"Hạ mọi boss và thoát khỏi khu vực":index==1?"Kết thúc với ít nhất 80% HP":index==2?"Tổng HP đã mất không vượt 50% HP tối đa":index==3?RatingSecondaryGoal:"Hạ mọi boss trong "+BossParSeconds.ToString("0")+"s kể từ lúc xuất hiện";
        public bool RatingCriterionMet(int index)=>index==0?true:index==1?HealthFraction>=.8f:index==2?DamageTaken<=MaxHealth*.5f+.001f:index==3?RatingSecondaryMet:ratingBossSeen&&BossCombatSeconds<=BossParSeconds;
        public bool LastRatingEarned(int index)=>(LastRatingMask&(1<<index))!=0;
        string RatingMetric(int index)=>index==0?"Boss đã bị tiêu diệt":index==1?"Còn "+(HealthFraction*100).ToString("0.#")+"% HP / cần ≥80%":index==2?"Đã mất "+DamageTaken.ToString("0.#")+" / tối đa "+(MaxHealth*.5f).ToString("0.#")+" HP; sửa tàu không xóa số đã mất":index==3?RatingSecondaryGoal+" — "+(Sector==4?EscapeRocksCleared+"/18":RatingSecondaryMet?"hoàn thành":"chưa hoàn thành"):BossCombatSeconds.ToString("0.0")+" / "+BossParSeconds.ToString("0")+"s; không tính pause hoặc chuỗi nổ cuối";
        void ResetRating()
        {
            DamageTaken=BossCombatSeconds=0;ratingBossSeen=false;LastStars=LastRatingMask=0;LastWasNewRating=false;
            for(int i=0;i<MaximumStars;i++)LastRatingDetails[i]="";
        }
        void TickRating(float dt){if(BossActive&&!TutorialActive){ratingBossSeen=true;BossCombatSeconds+=dt;}}
        void CompleteRating()
        {
            LastStars=LastRatingMask=0;
            for(int i=0;i<MaximumStars;i++)
            {
                if(RatingCriterionMet(i)){LastStars++;LastRatingMask|=1<<i;}
                LastRatingDetails[i]=RatingMetric(i);
            }
            LastWasNewRating=LastStars>profile.stars[Sector-1];
            profile.stars[Sector-1]=Mathf.Max(profile.stars[Sector-1],LastStars);
        }
    }
}
