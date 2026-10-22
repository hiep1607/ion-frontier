using System;
using System.IO;
using UnityEngine;

namespace IonFrontier
{
    public enum GunKind { Ion, Spread, Pierce, Homing }
    public enum PickupKind { Ammo, Shield, Upgrade, Spread, Pierce, Homing, Ultimate, Damage, FireRate, Multishot }

    [Serializable] public sealed class PilotProfile
    {
        public int version=4, clearedStages, selectedHull;
        public int[] stars=new int[13];
        public bool tutorialDone;
    }

    public static class IonCampaign
    {
        public const int Stages=13, Waves=3, MaxAmmo=600, MaxHealthReference=100;
        public static readonly int[] HullUnlocks={0,1,2,5,7,9,11,13};
        public static readonly string[] HullNames={"PHONG ƯNG","LÔI ĐIỂU","THIẾT VỆ","HỎA PHƯỢNG","ẢNH VŨ","TỪ LONG","TINH VÂN","HƯ KHÔNG"};
        public static readonly Color[] HullColors={IonArt.Cyan,IonArt.Gold,new Color(.3f,1,.64f),new Color(1,.35f,.18f),new Color(.7f,.5f,1),new Color(.2f,1,.8f),new Color(.4f,.6f,1),new Color(1,.35f,.9f)};
        public static readonly string[] StageNames={"VÀNH ĐAI TẬP SỰ","HÀNH LANG ĐỘT KÍCH","THÀNH TRÌ HẠM ĐỘI","THOÁT KHỎI VÀNH ĐAI","ĐOÀN TÀU CUỐI CÙNG","TRẠM CHẮN QUỸ ĐẠO","CUỘC SĂN SIÊU TỐC","TÍN HIỆU TRONG SƯƠNG","GIÀNH LẠI LÕI NĂNG LƯỢNG","BÃI MÌN SỐNG","HAI LỜI CẦU CỨU","VÙNG SỤP ĐỔ","TRÁI TIM HẠM ĐỘI"};
        public static readonly string[] BossNames={"BÃO LỬA","ẢNH MA","THIẾT GIÁP VƯƠNG","KẺ NGHIỀN ĐÁ","LIỆP HẠM","THÁP MẮT ĐỎ","ẢNH TIỄN","U LINH","HỘ VỆ TAM LÕI","NHỆN RẢI MÌN","SONG SÁT","KẺ TRUY DIỆT","ĐẾ HẠM HƯ KHÔNG"};
        public static readonly int[] BossHealth={180,320,650,440,480,560,520,580,650,680,450,760,1200};
        public static readonly string[] Briefings={
            "Tập lái, khai hỏa và đánh bại Bão Lửa.","Xuyên đội hình đột kích, hạ chỉ huy Ảnh Ma.","Phá thành trì đầu tiên của hạm đội.",
            "Bị truy đuổi trong vành đai đá: né thiên thạch và pháo khóa vị trí từ phía sau. Theo thanh tiến độ tới điểm nhảy; hạ Kẻ Nghiền Đá chặn lối.",
            "Bảo vệ tàu chở người sống sót. Địch sẽ nhắm vào đoàn tàu; phá móc giữ khi Liệp Hạm xuất hiện.",
            "Phá 3 máy phát khiên theo thứ tự tự chọn. Mỗi máy phát còn lại duy trì một kiểu phòng thủ.",
            "Bám theo tín hiệu chỉ huy: giữ cùng làn với mục tiêu. Né các đợt tăng tốc và chướng ngại.",
            "Tới gần 4 đèn hiệu và giữ vị trí để cứu trinh sát. Đồng minh được cứu sẽ bắn yểm trợ.",
            "Giữ từng vùng điều khiển đến khi chiếm đủ 3 lõi. Tiến độ được giữ khi rời vùng.",
            "Bắn mìn để kích nổ dây chuyền. Chọn đường vòng an toàn hoặc phá thẳng tuyến phong tỏa.",
            "Chọn cứu tàu tiếp tế để nhận tài nguyên, hoặc cứu phi đội để nhận hỏa lực hỗ trợ.",
            "Thoát khỏi vùng hủy diệt đang dâng lên. Đừng ở dưới vạch đỏ; phá vật cản để mở đường.",
            "Phá 3 trạm ngoài để tiếp cận kỳ hạm. Khi boss đổi giai đoạn, hạ các hộ vệ để mở lại lõi."};
        public static readonly Color[] StageColors={IonArt.Cyan,IonArt.Gold,IonArt.Orange,new Color(.65f,.45f,.25f),IonArt.Cyan,new Color(1,.3f,.3f),IonArt.Gold,new Color(.65f,.45f,1),new Color(.25f,1,.7f),new Color(1,.5f,.15f),new Color(.5f,.7f,1),new Color(1,.25f,.3f),new Color(.8f,.25f,1)};
        public static readonly string[] GunNames={"ION KÉP","ĐẠN TỎA","XUYÊN GIÁP","TRUY ĐUỔI"};
        public static readonly string[] GiftNames={"ĐẠN +60","SỬA TÀU +30 HP","CẤP KIỂU BẮN +1","KIỂU TỎA","KIỂU XUYÊN","KIỂU TRUY ĐUỔI","ĐẠI CHIÊU +1","SÁT THƯƠNG +20%","NHỊP BẮN +10%","MỖI LOẠT +1 VIÊN"};
        public static readonly Color[] GiftColors={new Color(.35f,.9f,1),new Color(.3f,.65f,1),IonArt.Gold,new Color(.35f,1,.6f),new Color(.8f,.45f,1),new Color(1,.6f,.25f),new Color(1,.25f,.45f),new Color(1,.4f,.25f),new Color(1,.95f,.45f),new Color(1,.55f,.9f)};
        public const double LootChance=.15;
        static readonly PickupKind[] LootTable={PickupKind.Ammo,PickupKind.Shield,PickupKind.Upgrade,PickupKind.Spread,PickupKind.Pierce,PickupKind.Homing,PickupKind.Ultimate,PickupKind.Damage,PickupKind.FireRate,PickupKind.Multishot};
        static readonly int[] LootWeights={20,12,6,6,6,6,3,16,14,14};
        public static bool RollLoot(System.Random random,out PickupKind kind)
        {
            kind=PickupKind.Ammo;if(random.NextDouble()>=LootChance)return false;
            int total=0;foreach(int weight in LootWeights)total+=weight;
            int choice=random.Next(total);
            for(int i=0;i<LootWeights.Length;i++){choice-=LootWeights[i];if(choice<0){kind=LootTable[i];return true;}}
            return false;
        }
        public static readonly string[] EnemyNames={"TRINH SÁT","ĐÁNH CHẶN","ĐỘT KÍCH","OANH TẠC","XẠ THỦ","HỘ VỆ","TINH NHUỆ"};
        public static readonly int[] EnemyHealth={2,3,3,5,3,7,5};
        public static int StartingAmmo(int stage)=>stage<=3?150+30*Mathf.Clamp(stage,1,3):300;
        public static int UnlockedHulls(PilotProfile p){int n=1;for(int i=1;i<HullUnlocks.Length;i++)if(p.clearedStages>=HullUnlocks[i])n++;return n;}
        public static int UnlockedStages(PilotProfile p)=>Mathf.Clamp(p.clearedStages+1,1,Stages);
        public static int RewardHull(int stage){for(int i=1;i<HullUnlocks.Length;i++)if(HullUnlocks[i]==stage)return i;return -1;}
        public static int EnemyKind(int stage,int wave,int index)
        {
            if(stage==1)return wave==1?0:wave==2?index%2:index%3;
            if(stage==2)return new[]{1,2,3,4,5}[(index+wave)%5];
            return (index+wave)%7;
        }
        public static int WaveCount(int stage,int wave)=>stage==1?4+wave*3:stage==2?8+wave*3:10+wave*4;
        public static float FireDelay(GunKind gun,int level)=>gun==GunKind.Spread?.24f:gun==GunKind.Pierce?.24f:gun==GunKind.Homing?.3f:.17f;
        public static int ShotCount(GunKind gun,int level)=>gun==GunKind.Spread?level*2+1:gun==GunKind.Homing?level==3?3:2:gun==GunKind.Pierce?level==3?2:1:level+1;
        public static int ShotDamage(GunKind gun,int level)=>gun==GunKind.Pierce?level:gun==GunKind.Homing&&level>1?2:1;
        public static PilotProfile Load(string path)
        {
            try
            {
                var p=File.Exists(path)?JsonUtility.FromJson<PilotProfile>(File.ReadAllText(path)):new PilotProfile();
                if(p==null)p=new PilotProfile();p.clearedStages=Mathf.Clamp(p.clearedStages,0,Stages);
                if(p.stars==null||p.stars.Length!=Stages){var old=p.stars;p.stars=new int[Stages];if(old!=null)Array.Copy(old,p.stars,Math.Min(old.Length,Stages));}
                for(int i=0;i<Stages;i++)p.stars[i]=Mathf.Clamp(p.stars[i],0,p.version<4?3:IonGame.MaximumStars);p.version=4;
                p.selectedHull=Mathf.Clamp(p.selectedHull,0,UnlockedHulls(p)-1);return p;
            }
            catch(Exception e) when(e is IOException||e is UnauthorizedAccessException||e is ArgumentException)
            {Debug.LogWarning("Không thể đọc tiến trình: "+e.Message);return new PilotProfile();}
        }
        public static void Save(string path,PilotProfile profile)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                // Keep the previous profile until the replacement has been written in full.
                string temp=path+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(profile,true));
                if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);
            }
            catch(Exception e) when(e is IOException||e is UnauthorizedAccessException)
            {Debug.LogWarning("Tiến trình được giữ cho phiên chơi này: "+e.Message);}
        }
    }
}
