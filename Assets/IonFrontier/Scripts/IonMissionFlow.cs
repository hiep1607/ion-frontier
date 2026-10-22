using UnityEngine;

namespace IonFrontier
{
    public sealed partial class IonGame
    {
        int flowBeat,flowKillStart,flowWorldStart,flowCaptureStart;float flowBeatTime,flowTrackStart,flowHold,flowEventClock;bool flowReady;
        public int MissionBeat=>flowBeat;
        public float MissionBeatTime=>flowBeatTime;
        public int MissionMarkedAlive{get{int n=0;foreach(var e in enemies)if(e.active&&e.missionMarked)n++;return n;}}
        public bool FlowJamming=>Sector==8&&flowBeat==1&&MissionMarkedAlive>0&&!missionBossStarted;
        static readonly string[][] BeatNames={
            new[]{"CẮT ĐUÔI QUA MẢNH VỠ","VƯỢT MƯA THIÊN THẠCH","BỨT TỚI ĐIỂM NHẢY"},
            new[]{"HỘ TỐNG QUA PHỤC KÍCH","CẮT MÓC VÀ SỬA TÀU","ĐƯA VẬN TẢI TỚI CỔNG"},
            new[]{"HẠ LƯỚI PHÒNG THỦ","CHIẾM QUYỀN ĐIỀU KHIỂN","PHÁ LÒ PHẢN ỨNG PHỤ"},
            new[]{"BẮT TÍN HIỆU","HẠ ĐỘI CHẶN HẬU","BÁM ĐUÔI TRONG LÀN LASER"},
            new[]{"CỨU HAI TRINH SÁT","PHÁ ĐỘI GÂY NHIỄU","MỞ HÀNH LANG CỨU HỘ"},
            new[]{"CHIẾM LÕI ĐẦU TIÊN","ĐẨY LÙI ĐỘI TÁI CHIẾM","ĐỒNG BỘ LÕI TRUNG TÂM"},
            new[]{"KÍCH NỔ MỞ ĐƯỜNG","PHÁ BA TRẠM RẢI MÌN","HỘ TỐNG TÀU DÒ ĐƯỜNG"},
            new[]{"TIẾP CẬN TÍN HIỆU","ĐÁNH TAN ĐỘI BAO VÂY","HOÀN TẤT GIẢI CỨU"},
            new[]{"THOÁT VÙNG SỤP ĐỔ","MỞ CỬA PHONG TỎA","PHÁ TRUY KÍCH, TỚI ĐIỂM NHẢY"},
            new[]{"CÔNG PHÁ TRẠM NGOÀI","HẠ TÀU ĐIỀU PHỐI","MỞ CỔNG VÀO KỲ HẠM"}};
        public string MissionFlowName=>Sector<4?"":BeatNames[Sector==11&&route==1?1:Sector-4][Mathf.Clamp(flowBeat,0,2)];
        int WorldAlive(WorldKind kind){int n=0;foreach(var o in world)if(o!=null&&o.active&&o.kind==kind&&!o.done)n++;return n;}
        bool EscortFlow=>Sector==5||Sector==11&&route==1;
        bool RescueFlow=>Sector==8||Sector==11&&route==2;
        public string MissionFlowText
        {
            get
            {
                string marked="MỤC TIÊU ƯU TIÊN CÒN "+MissionMarkedAlive;
                if(EscortFlow)return flowBeat==1?"PHÁ MÓC CÒN "+WorldAlive(WorldKind.Tether)+" / SỬA GẦN TÀU "+Mathf.FloorToInt(flowHold)+"/3s":"HỘ TỐNG "+Mathf.FloorToInt(Mathf.Min(18,flowBeatTime))+"/18s / "+marked+"\nTHÂN TÀU "+Mathf.CeilToInt(ConvoyHealth)+"%";
                if(RescueFlow)return flowBeat==1?marked+" / PHÁ NHIỄU ĐỂ CỨU TIẾP":"CỨU "+objectiveCount+"/"+(flowBeat==0?2:3)+" / PHỤ: ĐỦ 4 ĐÈN HIỆU";
                switch(Sector)
                {
                    case 4:return EscapeFraction>=1?"ĐIỂM NHẢY BỊ CHẶN / CHUẨN BỊ GIAO CHIẾN":"SỐNG SÓT TỚI ĐIỂM NHẢY / "+Mathf.FloorToInt(EscapeFraction*100)+"%\nNÉ THIÊN THẠCH VÀ PHÁO TỪ PHÍA SAU";
                    case 6:return flowBeat==1?"GIỮ VÒNG ĐIỀU KHIỂN / NÉ LASER": "MÁY PHÁT CÒN "+WorldAlive(WorldKind.Generator);
                    case 7:return flowBeat==1?marked:"BÁM CÙNG LÀN "+Mathf.FloorToInt(chaseProgress-flowTrackStart)+"/"+(flowBeat==0?12:15)+"s\n"+(flowBeat==2?marked:"");
                    case 9:return flowBeat==0?"CHIẾM MỘT LÕI / GIỮ TRONG VÒNG":flowBeat==1?"CHIẾM ĐỦ 3 LÕI / "+marked:"ĐỒNG BỘ VÒNG MỚI / "+marked;
                    case 10:return flowBeat==0?"KÍCH NỔ "+(worldDestroyed-flowWorldStart)+"/8 MÌN":flowBeat==1?"TRẠM RẢI MÌN CÒN "+WorldAlive(WorldKind.Generator):"HỘ TỐNG TÀU DÒ "+Mathf.FloorToInt(Mathf.Min(18,flowBeatTime))+"/18s / "+marked;
                    case 12:return flowBeat==0?"VƯỢT VÙNG ĐỎ "+Mathf.FloorToInt(Mathf.Min(20,flowBeatTime))+"/20s / HẠ "+(Kills-flowKillStart)+"/2 TÀU":flowBeat==1?"PHÁ CỔNG CÒN "+WorldAlive(WorldKind.Generator):marked+"\nTỚI VÒNG ĐIỂM NHẢY Ở PHÍA TRƯỚC";
                    case 13:return flowBeat==0?"TRẠM NGOÀI CÒN "+WorldAlive(WorldKind.Generator):flowBeat==1?marked:"GIỮ VÒNG CỔNG NHẢY / "+marked;
                    default:return "";
                }
            }
        }
        public string MissionFlowTip=>Sector==4?"Tàu tự tiến tới điểm thoát. Lách qua khe đá; vạch cam báo hướng pháo đã khóa.":flowBeat==1&&EscortFlow?"Đứng gần vận tải để sửa khẩn cấp sau khi phá móc.":FlowJamming?"Đội gây nhiễu làm mờ địch ở xa. Ưu tiên tàu có vòng đỏ.":Sector==7&&flowBeat==2?"Vạch sáng báo trước làn laser. Đổi làn để né rồi bám lại tín hiệu.":Sector==9?"Địch gần vòng làm chậm chiếm giữ. Hạ đội công binh để mở vòng.":"Hoàn thành mục tiêu để qua chặng ngay. Tàu có vòng đỏ là mục tiêu ưu tiên.";
        void StartMissionFlow(){flowReady=false;if(Sector>=4)EnterMissionBeat(0);}
        void ResetMissionFlow(){flowReady=false;flowBeat=0;flowBeatTime=flowHold=0;}
        void EnterMissionBeat(int beat)
        {
            flowBeat=beat;flowReady=true;flowBeatTime=flowHold=0;flowKillStart=Kills;flowWorldStart=worldDestroyed;flowTrackStart=chaseProgress;flowCaptureStart=captureCount;flowEventClock=5;
            missionSpawnClock=beat==0?1.5f:6;hazardClock=1;
            if(RoutePending)return;
            if(beat>0){SpawnCell(playerPosition+Vector2.up*2,PickupKind.Ammo);ClearHostileBullets();}
            if(EscortFlow)
            {
                if(beat==0||beat==2)SpawnPriorityGroup(beat==0?3:2,beat==0?8:14);
                else for(int i=0;i<2;i++)SpawnWorld(WorldKind.Tether,new Vector2(i==0?-6:-2,.2f),14,.65f);
            }
            else if(RescueFlow){if(beat==1)SpawnPriorityGroup(3,16);}
            else switch(Sector)
            {
                case 4:escapeRockClock=Mathf.Min(escapeRockClock,1.6f);break;
                case 6:if(beat==1)SpawnWorld(WorldKind.Zone,new Vector2(0,-.8f),1,1.6f);if(beat==2)SpawnFlowGenerators(3,26);break;
                case 7:if(beat==1)SpawnPriorityGroup(3,12);if(beat==2)SpawnPriorityGroup(2,7);break;
                case 9:if(beat==1)SpawnPriorityGroup(3,10);if(beat==2){SpawnWorld(WorldKind.Zone,new Vector2(0,-.5f),1,1.6f);SpawnPriorityGroup(2,11);}break;
                case 10:if(beat==1)SpawnFlowGenerators(3,18);if(beat==2){SpawnConvoy();SpawnPriorityGroup(2,13);}break;
                case 12:if(beat==1)SpawnFlowGenerators(2,20);if(beat==2){SpawnWorld(WorldKind.Zone,new Vector2(0,2),1,1.4f);SpawnPriorityGroup(3,18);}break;
                case 13:if(beat==1)SpawnPriorityGroup(2,15,28);if(beat==2){SpawnWorld(WorldKind.Zone,new Vector2(0,-.5f),1,1.6f);SpawnPriorityGroup(2,17,22);}break;
            }
            hud.Toast("CHẶNG "+(beat+1)+"/3 / "+MissionFlowName);if(beat>0)RequestUpgradeChoice(beat-1);
        }
        void SpawnFlowGenerators(int count,int hp){for(int i=0;i<count;i++)SpawnWorld(WorldKind.Generator,new Vector2((i-(count-1)*.5f)*5.5f,2),hp,.85f);}
        void SpawnPriorityGroup(int count,int variant,int health=0)
        {
            for(int i=0;i<count;i++)
            {
                int slot=FreeEnemy();if(slot<0)return;var e=SpawnEnemy(slot,1,new Vector2((i-(count-1)*.5f)*3.4f,3.3f),health>0?health:IonHostileFleet.Health[variant]+3);
                SetEnemyVariant(e,variant);e.missionMarked=true;e.fire=2.5f+i*.5f;
            }
        }
        void TickMissionFlow(float dt)
        {
            if(!flowReady)EnterMissionBeat(0);if(RoutePending)return;if(Sector==4){TickEscapeMission(dt);return;}flowBeatTime+=dt;
            if(EscortFlow&&flowBeat==1&&WorldAlive(WorldKind.Tether)==0&&(playerPosition-ConvoyPosition).sqrMagnitude<6){flowHold+=dt;RepairConvoyDuringHold(dt);}
            bool done=false;
            if(EscortFlow)done=flowBeat==1?WorldAlive(WorldKind.Tether)==0&&flowHold>=3:flowBeatTime>=18&&MissionMarkedAlive==0;
            else if(RescueFlow)done=flowBeat==1?MissionMarkedAlive==0:objectiveCount>=(flowBeat==0?2:3);
            else switch(Sector)
            {
                case 6:done=flowBeat==1?captureCount>flowCaptureStart:WorldAlive(WorldKind.Generator)==0;break;
                case 7:done=flowBeat==1?MissionMarkedAlive==0:chaseProgress-flowTrackStart>=(flowBeat==0?12:15)&&(flowBeat==0||MissionMarkedAlive==0);break;
                case 9:done=flowBeat==0?captureCount>=1:flowBeat==1?captureCount>=3&&MissionMarkedAlive==0:captureCount>flowCaptureStart&&MissionMarkedAlive==0;break;
                case 10:done=flowBeat==0?worldDestroyed-flowWorldStart>=8:flowBeat==1?WorldAlive(WorldKind.Generator)==0:flowBeatTime>=18&&MissionMarkedAlive==0;break;
                case 12:done=flowBeat==0?flowBeatTime>=20&&Kills-flowKillStart>=2:flowBeat==1?WorldAlive(WorldKind.Generator)==0:flowBeatTime>=12&&captureCount>flowCaptureStart&&MissionMarkedAlive==0;break;
                case 13:done=flowBeat==0?WorldAlive(WorldKind.Generator)==0:flowBeat==1?MissionMarkedAlive==0:captureCount>flowCaptureStart&&MissionMarkedAlive==0;break;
            }
            if(done&&flowBeatTime>.2f)
            {
                if(flowBeat<2)EnterMissionBeat(flowBeat+1);else if(PendingBossWrecks==0)SpawnExpandedBoss();return;
            }
            flowEventClock-=dt;if(flowEventClock<=0){flowEventClock=flowBeat==2?7:10;MissionBeatEvent();}
        }
        void RepairConvoyDuringHold(float dt){foreach(var o in world)if(o.active&&o.kind==WorldKind.Convoy)o.health=Mathf.Min(100,o.health+dt*3);}
        void MissionBeatEvent()
        {
            if(Sector==7&&flowBeat==2||Sector==6&&flowBeat==1)
            {SpawnBeam(new Vector2(Mathf.Clamp(playerPosition.x,-9,9),5));hud.Toast("LÀN ĐANG NẠP / ĐỔI VỊ TRÍ RỒI QUAY LẠI MỤC TIÊU");}
            if(EscortFlow&&flowBeat!=1||Sector==10&&flowBeat==2)
            {
                int slot=FreeEnemy();if(slot>=0){var e=SpawnEnemy(slot,1,new Vector2(ConvoyPosition.x+(Wave%2==0?4:-4),2.5f),7);SetEnemyVariant(e,8);e.fire=2;}
            }
            if(Sector==8&&flowBeat==1)Ring(new Vector2(0,2),new Color(.6f,.2f,1),10,.8f);
            if(Sector==13&&flowBeat==2)SpawnBeam(new Vector2(Wave%2==0?-5:5,5));
        }
    }
}
