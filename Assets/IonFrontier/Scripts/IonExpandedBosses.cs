using UnityEngine;

namespace IonFrontier
{
    public sealed partial class IonGame
    {
        int bossGuardMask,chaseDashCycle;float bossMineClock;
        bool BossCanTakeDamage(Enemy target)
        {
            if(target.kind<8||Sector<4)return true;
            if(Sector==9&&ActiveBossCount>1)
            {
                int open=(int)(waveTime/3)%ActiveBossCount,index=0;
                foreach(var e in enemies)if(e.active&&e.kind>=8){if(e==target)return index==open;index++;}
            }
            if(Sector==11&&ActiveBossCount>1)return (int)(waveTime/4)%2==target.index%2;
            if(Sector==5&&target.bossArt==4)foreach(var e in enemies)if(e.active&&e.kind>=8&&e.bossArt==13)return false;
            if(target==boss)
            {
                foreach(var o in world)if(o!=null&&o.active&&(o.kind==WorldKind.Guard||o.kind==WorldKind.Tether))return false;
                if(Sector==8&&target.age%8<3)
                {
                    foreach(var o in world)if(o.active&&o.kind==WorldKind.Beacon&&o.done&&(o.position-playerPosition).sqrMagnitude<5)return true;
                    return false;
                }
            }
            return true;
        }
        public bool BossShielded=>BossActive&&!BossCanTakeDamage(boss);
        public string BossHint=>Sector==4?"NÉ MŨI KHOAN / BẮN ĐÁ ĐỂ PHẢN ĐÒN":Sector==5?"HẠ HAI KÌM HÃM, CẮT MÓC RỒI ĐÁNH TÀU MẸ":Sector==6?"PHÁ 3 Ụ KHIÊN ĐỂ MỞ LÕI THÁP":Sector==7?"TRÁNH LÀN LAO / ĐỢI BOSS GIẢM TỐC":Sector==8?"ĐỨNG GẦN ĐÈN HIỆU XANH ĐỂ LỘ THÂN THẬT":Sector==9?"BẮN TÀU CÓ NHÃN LÕI MỞ / KHIÊN ĐỔI SAU 3 GIÂY":Sector==10?"KÍCH NỔ MÌN ĐỂ MỞ KHE THOÁT":Sector==11?"KHIÊN ĐỔI SAU 4 GIÂY / TÀU CÒN LẠI CUỒNG NỘ":Sector==12?"TRÁNH ĐƯỜNG TRUY KÍCH VÀ LASER":Sector==13?"PHÁ HỘ VỆ MỖI GIAI ĐOẠN ĐỂ MỞ LÕI":"";
        Vector2 BossMovement(Enemy e)
        {
            float t=e.motionTime;
            if(Sector==1)return new Vector2(Mathf.Sin(t*.55f)*4.2f,Mathf.Cos(t)*.35f);
            if(Sector==2)return new Vector2(Mathf.Sin(t*.7f)*5.8f,Mathf.Sin(t*1.4f)*.9f);
            if(Sector==3)return new Vector2(Mathf.Sin(t*.4f)*3.8f,Mathf.Cos(t*.6f)*.3f);
            if(Sector==5||Sector==9)return new Vector2(Mathf.Sin(t*.6f+e.index*2)*1.2f,Mathf.Cos(t*.8f+e.index)*.4f);
            if(Sector==7)return new Vector2(Mathf.Sin(t*1.2f)*6,Mathf.Cos(t*.8f)*.6f);
            if(Sector==11)return new Vector2(Mathf.Sin(t*.7f+e.index*Mathf.PI)*2.2f,Mathf.Sin(t*.9f+e.index)*.7f);
            if(Sector==12){float dive=Mathf.Sin(Mathf.Clamp01((e.age%12-7)/4)*Mathf.PI);return new Vector2(Mathf.Sin(t*.7f)*5,-dive*7);}
            return new Vector2(Mathf.Sin(t*.53f)*4.5f,Mathf.Cos(t*.8f)*.35f);
        }
        Color EnemyTint(Enemy e)
        {
            Color tint=Color.white;
            if(e.kind>=8&&Sector>=4)tint=Color.Lerp(Color.white,IonCampaign.StageColors[Sector-1],.25f);
            if(e.kind>=8&&!BossCanTakeDamage(e))tint=new Color(.45f,.55f,.8f,1);
            if(e.variant==18&&e.age%6>4.5f)tint=new Color(.6f,.3f,1,.28f);
            if(Sector==8)
            {
                float distance=(e.position-playerPosition).sqrMagnitude;
                foreach(var o in world)if(o!=null&&o.active&&o.done&&o.kind==WorldKind.Beacon)distance=Mathf.Min(distance,(e.position-o.position).sqrMagnitude);
                if(distance>(FlowJamming?9:25))tint.a=FlowJamming?.15f:.3f;
            }
            return tint;
        }
        float ModifyEnemyDamage(Enemy e,float damage)
        {
            if(e.kind>=8&&!BossCanTakeDamage(e)){Ring(e.position,IonArt.Cyan,2,.15f);return 0;}
            if(e.variant==18&&e.age%6>4.5f)return 0;
            if(e==boss&&Sector==13)
            {
                float threshold=expandedPhase==1?e.maxHealth*.66f:expandedPhase==2?e.maxHealth*.33f:0;
                if(threshold>0&&e.health>threshold&&damage>e.health-threshold)return e.health-threshold+.01f;
                if(threshold>0&&e.health<=threshold)return 0;
            }
            if(e.kind<8&&e.kind!=6&&!EncounterOwns(e))foreach(var guard in enemies)if(guard.active&&guard.kind==6&&!EncounterOwns(guard)&&(guard.position-e.position).sqrMagnitude<6)
            {Ring(e.position,IonArt.Cyan,.8f,.12f);return damage*.65f;}
            return damage;
        }
        void OnExpandedEnemyKilled(Enemy e)
        {
            if(e.kind>=8&&Sector==11&&e==boss)
            {
                foreach(var other in enemies)if(other.active&&other.kind>=8){boss=other;hud.Toast("SONG SÁT / TÀU CÒN LẠI CUỒNG NỘ!");break;}
            }
        }
        void SpawnExpandedBoss()
        {
            ClearEncountersForBoss();if(Sector==4){HideEscapeWarnings();RetireCounterForBoss();}
            ratingBossSeen=true;missionBossStarted=true;waveLive=true;Wave=4;waveTime=0;bossGuardMask=0;expandedPhase=1;bossMineClock=3;chaseDashCycle=-1;
            foreach(var e in enemies)if(e.active){e.active=false;e.renderer.gameObject.SetActive(false);e.engine.gameObject.SetActive(false);e.halo.gameObject.SetActive(false);}
            foreach(var b in bullets)Release(b);ClearBeams();
            foreach(var o in world)if(o.active&&(o.kind==WorldKind.Mine||o.kind==WorldKind.Rock))ReleaseWorld(o);
            foreach(var o in world)if(o.active&&o.kind==WorldKind.Zone)ReleaseWorld(o);
            pursuitSignal.gameObject.SetActive(false);collapseWall.gameObject.SetActive(false);
            boss=SpawnEnemy(0,10,new Vector2(Sector==11?-4:0,Sector==13?1.5f:2f),Sector==9?260:IonCampaign.BossHealth[Sector-1]);boss.fire=2.7f;
            if(Sector==5)for(int i=1;i<=2;i++)SpawnBossPartner(i,13,new Vector2(i==1?-4.8f:4.8f,1.1f),150,3.1f);
            if(Sector==9)for(int i=1;i<=2;i++)SpawnBossPartner(i,14,new Vector2(i==1?-4.8f:4.8f,1.5f),210,3.4f);
            if(Sector==11)SpawnBossPartner(1,15,new Vector2(4,2.3f),430,4.7f);
            if(Sector==13)boss.renderer.transform.localScale*=1.3f;
            if(Sector==5)for(int i=0;i<2;i++)SpawnWorld(WorldKind.Tether,new Vector2(i==0?-6:-2,.2f),18,.65f);
            if(Sector==6||Sector==13)SpawnBossGuards();
            Ammo=Mathf.Min(IonCampaign.MaxAmmo,Ammo+120);UltimateCharges=1;HealPlayer(20);
            if(Sector==11&&route==1){Ammo=Mathf.Min(600,Ammo+100);loadout.AddStat(PickupKind.Damage);loadout.AddStat(PickupKind.FireRate);}
            bannerTimer=0;hud.ShowBanner(false,Sector,SectorName);hud.Refresh();hud.Toast("CẢNH BÁO / "+BossName+" — "+BossHint);Play(IonArt.Blast,.65f);
        }
        void SpawnBossGuards()
        {
            for(int i=0;i<3;i++)SpawnWorld(WorldKind.Guard,new Vector2((i-1)*5,0),Sector==13?35:22,.7f);
            bossGuardMask|=1<<expandedPhase;ClearHostileBullets();hud.Toast("LÕI ĐÓNG / PHÁ 3 HỘ VỆ ĐỂ MỞ ĐIỂM YẾU");
        }
        void SpawnBossPartner(int slot,int artwork,Vector2 position,int hp,float size)
        {
            var e=SpawnEnemy(slot,9,position,hp);e.bossArt=artwork;e.renderer.sprite=IonArt.BossFleet[artwork];e.renderer.transform.localScale=Vector3.one*IonArt.ScaleFor(e.renderer.sprite,size);e.fire=3+slot*.6f;
        }
        void UpdateExpandedBoss(float dt)
        {
            if(!BossActive)return;
            if(Sector==13)
            {
                int phase=BossFraction>.66f?1:BossFraction>.33f?2:3;
                if(phase>expandedPhase){expandedPhase=phase;SpawnBossGuards();for(int i=0;i<2;i++){int slot=FreeEnemy();if(slot<0)break;var guard=SpawnEnemy(slot,1,new Vector2(i==0?-6:6,3.8f),10);SetEnemyVariant(guard,phase==2?15:18);}}
            }
            for(int i=0;i<bossOrnaments.Length;i++)
            {
                var sr=bossOrnaments[i];sr.gameObject.SetActive(i<3);if(i>=3)continue;
                if(Sector==7&&i<2)
                {
                    sr.sprite=boss.renderer.sprite;sr.transform.position=boss.position+new Vector2((i==0?-1:1)*3.5f,Mathf.Sin(waveTime*2+i)*.5f);
                    sr.transform.localScale=boss.renderer.transform.localScale*.8f;sr.transform.rotation=boss.renderer.transform.rotation;sr.color=new Color(1,.8f,.35f,.22f);continue;
                }
                sr.sprite=IonArt.Ring;sr.transform.position=boss.position+new Vector2((i-1)*1.2f,0);sr.transform.localScale=Vector3.one*(i==1?1:.6f);sr.color=BossShielded?new Color(.2f,.6f,1,.55f):new Color(1,.45f,.15f,.55f);
            }
            if(Sector==12&&boss.age%12>6&&boss.age%12<7&&chaseDashCycle!=(int)(boss.age/12))
            {chaseDashCycle=(int)(boss.age/12);hud.Toast("TRUY DIỆT / BOSS ĐANG VÒNG PHÍA SAU — RỜI ĐÁY MÀN!");Ring(boss.position,IonArt.Orange,4,1);}
            if(Sector==10)
            {
                bossMineClock-=dt;if(bossMineClock<=0){bossMineClock=4;for(int i=0;i<3;i++)SpawnWorld(WorldKind.Mine,boss.position+new Vector2((i-1)*2,-1.5f),3,.45f,Vector2.down*1.3f);}
            }
        }
        void ExpandedBossAttack(Enemy e,Vector2 p,Vector2 aim,int phase)
        {
            switch(Sector)
            {
                case 4:
                    Fan(p,aim,3+phase,14,4.6f);for(int i=0;i<3;i++)SpawnWorld(WorldKind.Rock,p+new Vector2((i-1)*2,0),6,.65f,Vector2.down*2);break;
                case 5:
                    if(e.bossArt==13){Fan(p,aim,2+phase,17,4.5f);if(BossAttacks%3==0)SpawnBeam(new Vector2(e.position.x,5),e);}
                    else {Fan(p,MissionConvoyActive?(ConvoyPosition-p).normalized:aim,3,18,3.6f);Fan(p,aim,phase+2,14,4);}
                    break;
                case 6:
                    for(int i=0;i<3;i++){float a=waveTime+i*Mathf.PI*2/3;Fan(p,new Vector2(Mathf.Sin(a),Mathf.Cos(a)),3,14,3.5f);}
                    if(phase>1)SpawnBeam(p+Vector2.right*2,e);break;
                case 7:
                    Fan(p,aim,3,10,6.3f);if(BossAttacks%3==0){SpawnBeam(new Vector2(playerPosition.x,5),e);hud.Toast("ẢNH TIỄN / TRÁNH LÀN ĐANG NẠP");}break;
                case 8:
                    for(int side=-1;side<=1;side+=2)Fan(p+Vector2.right*side*3,aim,phase+2,16,3.7f);
                    Ring(p,IonArt.Cyan,4,.6f);break;
                case 9:
                    if(BossCanTakeDamage(e))
                    {int total=9+phase*2;for(int i=0;i<total;i++){float a=i*Mathf.PI*2/total+waveTime*.3f+e.index;Bolt(p,new Vector2(Mathf.Sin(a),Mathf.Cos(a))*3.6f,false,.14f);}}
                    else Fan(p,aim,2,18,4.2f);
                    break;
                case 10:
                    Fan(p,Vector2.down,7+phase*2,14,3.8f);if(phase==3)Fan(p,aim,3,15,5);break;
                case 11:
                    bool enraged=ActiveBossCount==1;Fan(p,aim,enraged?7:4,13,enraged?5.5f:4);
                    if(enraged){SpawnBeam(new Vector2(-4,5),e);SpawnBeam(new Vector2(4,5),e);}break;
                case 12:
                    Fan(p,aim,phase+3,11,5.4f);if(BossAttacks%2==0)SpawnBeam(new Vector2(playerPosition.x,5),e);break;
                case 13:
                    if(BossAttacks%3==0){for(int side=-1;side<=1;side+=2)SpawnBeam(p+Vector2.right*side*2,e);}
                    else if(BossAttacks%3==1)Fan(p,Vector2.down,9+phase*2,12,4.1f);
                    else
                    {int gap=BossAttacks%7-3;for(int i=-6;i<=6;i++)if(Mathf.Abs(i-gap)>1)Bolt(new Vector2(i*1.7f,5),Vector2.down*4,false,.16f);}
                    break;
            }
            Ring(p,IonCampaign.StageColors[Sector-1],1.6f,.35f);Play(IonArt.Impact,.7f);
        }
    }
}
