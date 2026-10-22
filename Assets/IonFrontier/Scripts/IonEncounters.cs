using System;
using UnityEngine;

namespace IonFrontier
{
    public sealed partial class IonGame
    {
        sealed class EncounterMember
        {
            public Enemy enemy; public int serial,role; public Vector2 station;
            public bool child;
        }
        readonly EncounterMember[] encounterMembers=new EncounterMember[10];
        readonly SpriteRenderer[] encounterLinks=new SpriteRenderer[6];
        System.Random encounterRandom;
        int encounterSession,encounterSeed,encounterType=-1,lastEncounterType=-1;
        float encounterClock,encounterRecovery,encounterHealClock,encounterSpawnClock;
        bool encounterRunning,encounterAlarmTriggered,encounterAlarmCalled,encounterPullWarned,encounterBeamFired,encounterMineDropped;
        int encounterPullCycle=-1,encounterMineCycle=-1;
        Vector2 encounterAnchor,encounterLockedAim;
        public static readonly string[] EncounterNames={"MŨI KHIÊN / XẠ THỦ","MÓC KÉO / LÀN LASER","TÀU MẸ / XƯỞNG SỬA","HAI CÁNH GỌNG KÌM","TÀU BÁO ĐỘNG","MÌN / NGƯ LÔI"};
        public bool EncounterActive=>encounterRunning;
        public int EncounterTypeIndex=>encounterType;
        public int EncounterSeed=>encounterSeed;
        public int EncounterMembersAlive{get{int n=0;foreach(var m in encounterMembers)if(EncounterMemberAlive(m))n++;return n;}}
        public int EncounterChildCount{get{int n=0;foreach(var m in encounterMembers)if(m!=null&&m.child&&EncounterMemberAlive(m))n++;return n;}}
        public int EncounterShieldLinks{get{if(encounterType!=0||EncounterRole(0)==null)return 0;int n=0;foreach(var m in encounterMembers)if(m!=null&&m.role!=0&&EncounterMemberAlive(m)&&(m.enemy.position-EncounterRole(0).position).sqrMagnitude<13)n++;return n;}}
        public int EncounterHealPulses {get;private set;}
        public int EncounterPullEvents {get;private set;}
        public int EncounterLaserEvents {get;private set;}
        public int EncounterFireEvents {get;private set;}
        public int EncounterAlarmCalls {get;private set;}
        public int EncounterAlarmRewards {get;private set;}
        public int EncounterMineEvents {get;private set;}
        public int EncounterTorpedoes {get;private set;}
        public int EncounterChildrenSpawned {get;private set;}
        public ulong EncounterKindsSeen {get;private set;}
        public string EncounterHudText=>!encounterRunning?"":encounterType==4&&!encounterAlarmTriggered&&EncounterRole(0)!=null?
            "BÁO ĐỘNG / HẠ TÀU TÍN HIỆU TRONG "+Mathf.CeilToInt(Mathf.Max(0,7.6f-encounterClock))+"s":
            encounterType==1&&encounterPullWarned&&!encounterBeamFired?"MÓC KÉO ĐANG KHÓA / RỜI LÀN ĐỎ":EncounterNames[encounterType];

        bool EncounterMemberAlive(EncounterMember m)=>m!=null&&m.enemy!=null&&m.enemy.serial==m.serial&&m.enemy.active;
        Enemy EncounterRole(int role){foreach(var m in encounterMembers)if(m!=null&&m.role==role&&!m.child&&EncounterMemberAlive(m))return m.enemy;return null;}
        EncounterMember FindEncounterMember(Enemy e){foreach(var m in encounterMembers)if(m!=null&&m.enemy==e&&m.serial==e.serial)return m;return null;}
        bool EncounterOwns(Enemy e)=>encounterRunning&&e!=null&&e.active&&FindEncounterMember(e)!=null;
        void EnsureEncounterVisuals()
        {
            for(int i=0;i<encounterLinks.Length;i++)if(encounterLinks[i]==null)
            {encounterLinks[i]=IonArt.Sprite("Coordinated fleet link "+i,IonArt.Pixel,Vector2.zero,Vector2.one,Color.clear,8,transform);encounterLinks[i].gameObject.SetActive(false);}
        }
        void HideEncounterLinks(){foreach(var line in encounterLinks)if(line!=null)line.gameObject.SetActive(false);}
        void ResetEncounters()
        {
            encounterRunning=false;encounterType=lastEncounterType=-1;encounterClock=encounterRecovery=0;
            encounterSeed=unchecked(8101+Sector*101+(++encounterSession)*977);encounterRandom=new System.Random(encounterSeed);
            for(int i=0;i<encounterMembers.Length;i++)encounterMembers[i]=null;
            EncounterHealPulses=EncounterPullEvents=EncounterLaserEvents=EncounterFireEvents=EncounterAlarmCalls=EncounterAlarmRewards=EncounterMineEvents=EncounterTorpedoes=EncounterChildrenSpawned=0;
            EncounterKindsSeen=0;HideEncounterLinks();
        }
        void ClearEncountersForBoss()
        {
            foreach(var m in encounterMembers)if(EncounterMemberAlive(m)&&!m.enemy.missionMarked)RetireEncounterEnemy(m.enemy);
            encounterRunning=false;encounterRecovery=0;for(int i=0;i<encounterMembers.Length;i++)encounterMembers[i]=null;HideEncounterLinks();
        }
        void RetireEncounterEnemy(Enemy e)
        {CancelSourceBeams(e);e.active=false;e.renderer.gameObject.SetActive(false);e.engine.gameObject.SetActive(false);e.halo.gameObject.SetActive(false);}
        int[] EncounterChoices()
        {
            switch(Sector)
            {
                case 5:return new[]{0,5};case 6:return new[]{2,0};case 8:return new[]{4,3};case 9:return new[]{0,2};case 10:return new[]{5};
                case 11:return route==1?new[]{0,5,1}:new[]{4,3,1};case 12:return new[]{3};case 13:return new[]{2,0,3};default:return null;
            }
        }
        bool TrySpawnEncounter()
        {
            if(Sector<5||Sector==4)return false;
            if(missionBossStarted||RoutePending||State!=FlightState.Playing)return true;
            if(Sector==7)
            {
                if(MissionMarkedAlive>0){TryAttachPriorityWings();return true;}
                return false;
            }
            var choices=EncounterChoices();if(choices==null)return false;
            // Returning true while deferred consumes the ordinary spawn slot without doubling density.
            if(encounterRunning||encounterRecovery>0||ActiveEnemies>12)return true;
            if(choices.Length==1&&lastEncounterType==choices[0]){lastEncounterType=-1;return false;}
            if(encounterRandom==null)encounterRandom=new System.Random(8101+Sector*101);
            int pick=choices[encounterRandom.Next(choices.Length)];
            if(pick==lastEncounterType&&choices.Length>1){int at=Array.IndexOf(choices,pick);pick=choices[(at+1)%choices.Length];}
            int[] variants=EncounterVariants(pick);int free=0;foreach(var e in enemies)if(!e.active&&!e.dying)free++;
            if(free<variants.Length||ActiveEnemies+variants.Length>18)return true;
            BeginEncounter(pick,variants,null);return true;
        }
        int[] EncounterVariants(int type)
        {
            switch(type)
            {
                case 0:return new[]{Sector<=3?5:11,4,4};case 1:return new[]{14,16};case 2:return new[]{15,10,7,7};
                case 3:return new[]{7,7,9,9};case 4:return new[]{16,7,7};default:return new[]{13,8};
            }
        }
        void ConfigureOpeningEncounter()
        {
            if(!(Sector==2&&Wave==3||Sector==3&&(Wave==2||Wave==3)))return;
            ClearEncountersForBoss();int type=Sector==3&&Wave==3?3:0;int[] variants=EncounterVariants(type);var reuse=new Enemy[variants.Length];int n=0;
            foreach(var e in enemies)if(e.active&&e.kind<8&&n<reuse.Length)reuse[n++]=e;
            if(n==reuse.Length)BeginEncounter(type,variants,reuse);
        }
        void TryAttachPriorityWings()
        {
            if(Sector!=7||encounterRunning||encounterRecovery>0||missionBossStarted)return;
            var reuse=new Enemy[MissionMarkedAlive];int n=0;foreach(var e in enemies)if(e.active&&e.missionMarked)reuse[n++]=e;
            if(n==0)return;var variants=new int[n];for(int i=0;i<n;i++)variants[i]=reuse[i].variant;BeginEncounter(3,variants,reuse,true);
        }
        void BeginEncounter(int type,int[] variants,Enemy[] reuse,bool preservePriority=false)
        {
            EnsureEncounterVisuals();HideEncounterLinks();encounterType=lastEncounterType=type;encounterRunning=true;encounterClock=0;
            encounterHealClock=3;encounterSpawnClock=5;encounterAlarmTriggered=encounterAlarmCalled=false;encounterPullWarned=encounterBeamFired=encounterMineDropped=false;encounterPullCycle=encounterMineCycle=-1;
            encounterAnchor=new Vector2(Sector==5||Sector==11&&route==1?Mathf.Clamp(ConvoyPosition.x,-3,3):0,3.5f);
            for(int i=0;i<encounterMembers.Length;i++)encounterMembers[i]=null;
            for(int i=0;i<variants.Length;i++)
            {
                Enemy e=reuse!=null?reuse[i]:null;
                if(e==null){int slot=FreeEnemy();if(slot<0)break;e=SpawnEnemy(slot,1,encounterAnchor,IonHostileFleet.Health[variants[i]]+Mathf.Min(2,Sector/5));}
                if(!preservePriority){SetEnemyVariant(e,variants[i]);e.health=e.maxHealth=IonHostileFleet.Health[variants[i]]+Mathf.Min(2,Sector/5);}
                Vector2 station=EncounterStation(type,i);e.target=station;e.entry=type==3?new Vector2(i%2==0?-HalfWidth-2:HalfWidth+2,station.y+2):station+Vector2.up*8;
                e.position=e.entry;e.age=0;e.motionTime=0;e.fire=2.2f+i*.85f;e.renderer.transform.position=e.position;
                encounterMembers[i]=new EncounterMember{enemy=e,serial=e.serial,role=i,station=station};
            }
            EncounterKindsSeen|=1UL<<type;hud.Toast("ĐỘI HÌNH / "+EncounterNames[type]);
        }
        Vector2 EncounterStation(int type,int role)
        {
            Vector2 c=encounterAnchor;
            if(type==0)return c+(role==0?new Vector2(0,-1.25f):new Vector2(role==1?-1.5f:1.5f,.25f));
            if(type==1)return c+new Vector2(role==0?-3:3,-.3f);
            if(type==2)return c+(role==0?new Vector2(0,0):role==1?new Vector2(2.3f,.3f):new Vector2(role==2?-3:3,-1.5f));
            if(type==3)return c+new Vector2((role%2==0?-1:1)*(role<2?5:2.5f),role<2?-1.3f:.7f);
            if(type==4)return c+new Vector2(role==0?0:role==1?-2.5f:2.5f,role==0?.5f:-1);
            return c+new Vector2(role==0?-3:3,0);
        }
        void TickEncounters(float dt)
        {
            if(State!=FlightState.Playing||TestFreezeEnemies)return;
            encounterRecovery=Mathf.Max(0,encounterRecovery-dt);
            if(Sector==7&&!encounterRunning&&MissionMarkedAlive>0)TryAttachPriorityWings();
            if(!encounterRunning)return;
            if(missionBossStarted){ClearEncountersForBoss();return;}
            HideEncounterLinks();
            if(EncounterMembersAlive==0){encounterRunning=false;encounterRecovery=2.5f;missionSpawnClock=Mathf.Min(missionSpawnClock,2.5f);return;}
            Enemy lead=EncounterRole(0);float step=dt*(lead!=null?EnemySlow(lead.position):1);encounterClock+=step;
            if(encounterClock<1.6f)return;
            if(encounterType==0&&lead!=null)
            {
                int line=0;foreach(var m in encounterMembers)if(m!=null&&m.role!=0&&EncounterMemberAlive(m)&&(m.enemy.position-lead.position).sqrMagnitude<13)
                {DrawLine(encounterLinks[line++],lead.position,m.enemy.position,.045f,new Color(.2f,.65f,1,.65f));if(line==2)break;}
            }
            if(encounterType==2)
            {
                var mechanic=EncounterRole(1);encounterHealClock-=step;encounterSpawnClock-=step;
                if(lead!=null&&mechanic!=null)
                {
                    DrawLine(encounterLinks[0],mechanic.position,lead.position,.055f,new Color(.25f,1,.5f,.7f));
                    if(encounterHealClock<=0){encounterHealClock=3;lead.health=Mathf.Min(lead.maxHealth,lead.health+2);EncounterHealPulses++;Ring(lead.position,new Color(.25f,1,.5f),1.3f,.35f);}
                }
                if(lead!=null&&encounterSpawnClock<=0){encounterSpawnClock=5;SpawnEncounterChildren(2);}
            }
            if(encounterType==1)TickTractorEncounter(step);
            if(encounterType==4&&lead!=null&&!encounterAlarmTriggered)
            {
                lead.halo.sprite=IonArt.Ring;lead.halo.transform.localScale=Vector3.one*(1.5f+Mathf.Sin(encounterClock*12)*.15f);lead.halo.color=new Color(1,.3f,.15f,.6f);
                if(encounterClock>=7.6f){encounterAlarmTriggered=encounterAlarmCalled=true;EncounterAlarmCalls++;SpawnEncounterChildren(2);hud.Toast("TÍN HIỆU ĐÃ PHÁT / VIỆN BINH ĐANG TỚI!");}
            }
            if(encounterType==5)TickMineEncounter();
        }
        void TickTractorEncounter(float dt)
        {
            var tractor=EncounterRole(0);var prism=EncounterRole(1);float local=Mathf.Max(0,encounterClock-2);int cycle=(int)(local/7);float phase=local%7;
            if(cycle!=encounterPullCycle){encounterPullCycle=cycle;encounterPullWarned=encounterBeamFired=false;}
            if(tractor!=null&&prism!=null)
            {
                if(!encounterPullWarned){encounterPullWarned=true;encounterLockedAim=playerPosition;EncounterPullEvents++;hud.Toast("MÓC KÉO KHÓA LÀN / RỜI VẠCH TRƯỚC KHI PHÁO BẮN!");}
                if(phase<.75f)DrawLine(encounterLinks[0],tractor.position,playerPosition,.035f,new Color(.25f,1,.6f,.35f));
                else if(phase<1.75f)
                {
                    if((playerPosition-tractor.position).sqrMagnitude<64)playerPosition=ClampPlayer(Vector2.MoveTowards(playerPosition,tractor.position,.6f*dt));
                    DrawLine(encounterLinks[0],tractor.position,playerPosition,.065f,new Color(.25f,1,.6f,.65f));
                }
                // Pull ends at1.75s. Beam begins warning at2.15s and fires only at3.30s.
                if(phase>=2.15f&&!encounterBeamFired)
                {encounterBeamFired=true;int casts=BeamCasts;SpawnBeam(new Vector2(Mathf.Clamp(encounterLockedAim.x,-HalfWidth+1,HalfWidth-1),5),prism);if(BeamCasts>casts)EncounterLaserEvents++;}
            }
        }
        void TickMineEncounter()
        {
            float local=Mathf.Max(0,encounterClock-2);int cycle=(int)(local/6);float phase=local%6;
            if(cycle!=encounterMineCycle){encounterMineCycle=cycle;encounterMineDropped=false;encounterBeamFired=false;}
            var layer=EncounterRole(0);var torpedo=EncounterRole(1);
            if(layer!=null&&!encounterMineDropped&&WorldAlive(WorldKind.Mine)<12)
            {
                encounterMineDropped=true;int created=0,allowed=Mathf.Min(3,12-WorldAlive(WorldKind.Mine));
                float dropY=Mathf.Max(layer.position.y-.7f,playerPosition.y+1.8f);
                for(int i=0;i<allowed;i++)if(SpawnWorld(WorldKind.Mine,new Vector2(-5+i*2,dropY),3,.45f,Vector2.down*1.15f)!=null)created++;
                if(created>0){EncounterMineEvents++;Ring(layer.position,IonArt.Orange,2,.4f);}
            }
            if(torpedo!=null&&phase>=1.3f&&!encounterBeamFired)
            {encounterBeamFired=true;FireEncounterTorpedoes(torpedo);}
        }
        void FireEncounterTorpedoes(Enemy e)
        {
            int before=EncounterTorpedoes;
            for(int side=-1;side<=1;side+=2){var b=Bolt(e.position+new Vector2(side*.4f,-.5f),Vector2.down*2.2f,false,.18f);if(b!=null){b.acceleration=.7f;b.damage=24;EncounterTorpedoes++;b.renderer.sprite=IonArt.BoltIcons[2,0];b.renderer.transform.localScale=new Vector3(.25f,.6f,1);}}
            if(EncounterTorpedoes>before){EncounterFireEvents++;Play(IonArt.Impact,.8f);}
        }
        void SpawnEncounterChildren(int requested)
        {
            var leader=EncounterRole(0);if(leader==null)return;
            for(int n=0;n<requested&&EncounterChildCount<4&&ActiveEnemies<18;n++)
            {
                int position=-1;for(int i=6;i<encounterMembers.Length;i++)if(!EncounterMemberAlive(encounterMembers[i])){position=i;break;}int slot=FreeEnemy();if(position<0||slot<0)break;
                var e=SpawnEnemy(slot,1,encounterAnchor+new Vector2((position-7.5f)*1.8f,-1.7f),3);SetEnemyVariant(e,7);e.entry=e.position=leader.position;e.age=0;e.fire=2+position*.15f;
                encounterMembers[position]=new EncounterMember{enemy=e,serial=e.serial,role=position,child=true,station=e.target};
                EncounterChildrenSpawned++;
            }
        }
        bool UpdateEncounterEnemy(Enemy e,float dt)
        {
            if(!EncounterOwns(e))return false;var m=FindEncounterMember(e);float step=dt*EnemySlow(e.position);e.age+=step;e.motionTime+=step;e.flash=Mathf.Max(0,e.flash-dt);
            bool retreat=encounterType==4&&m.role==0&&encounterAlarmCalled;
            Vector2 station=m.station+Vector2.right*Mathf.Sin(e.motionTime*.6f+m.role)*.35f;
            if(encounterType==3)station+=new Vector2(Mathf.Sin(e.motionTime*.9f+m.role)*.75f,Mathf.Sin(e.motionTime*.5f)*.2f);
            Vector2 old=e.position;e.position=retreat?Vector2.MoveTowards(e.position,new Vector2(e.position.x,11),step*7):Vector2.Lerp(e.entry,station,Mathf.SmoothStep(0,1,Mathf.Clamp01(e.age/1.5f)));
            if(retreat&&e.position.y>9.2f){RetireEncounterEnemy(e);return true;}
            e.renderer.transform.position=e.position;e.renderer.transform.rotation=Quaternion.Euler(0,0,Mathf.Clamp((e.position.x-old.x)/Mathf.Max(.001f,dt)*-2,-16,16));
            e.renderer.color=e.flash>0?new Color(1,.55f,.4f):EnemyTint(e);e.engine.transform.position=e.position+Vector2.up*.43f;e.engine.transform.localScale=new Vector3(.4f,.62f+Mathf.Sin(e.age*20)*.07f,1);
            e.halo.transform.position=e.position;e.halo.transform.localScale=Vector3.one*1.8f;e.halo.color=new Color(1,.2f,.05f,.08f);
            if(e.missionMarked){e.halo.sprite=IonArt.Ring;e.halo.color=new Color(1,.2f,.1f,.7f);e.halo.transform.localScale=Vector3.one*2.15f;}
            else if(encounterType==4&&m.role==0){e.halo.sprite=IonArt.Ring;e.halo.color=new Color(1,.3f,.1f,.5f);}
            if(e.age>=1.7f&&!TutorialActive&&!retreat)
            {
                e.fire-=step;if(e.fire<=0){FireEncounterMember(m);e.fire=4.2f+(m.role%2)*1.0f;}
                if((e.position-playerPosition).sqrMagnitude<.75f&&invincible<=0){HurtPlayer(20,false,"VA CHẠM TÀU");DamageEnemy(e,99);}
            }
            return true;
        }
        void FireEncounterMember(EncounterMember m)
        {
            Enemy e=m.enemy;Vector2 p=e.position+Vector2.down*.45f;Vector2 aim=(playerPosition-p).normalized;
            if(MissionConvoyActive&&m.role%2==0)aim=(ConvoyPosition-p).normalized;
            if(encounterType==1&&!m.child)
            {
                if(m.role==0){int before=ActiveBullets;Fan(p,aim,2,14,3.8f);if(ActiveBullets>before)EncounterFireEvents++;}
                else if(EncounterRole(0)==null){int casts=BeamCasts;SpawnBeam(new Vector2(e.position.x,5),e);if(BeamCasts>casts){EncounterLaserEvents++;EncounterFireEvents++;}}
                return;
            }
            if(encounterType==5&&!m.child)return;
            int bulletsBefore=ActiveBullets;
            if(encounterType==0)Fan(p,m.role==0?Vector2.down:aim,m.role==0?2:1,14,m.role==0?3.6f:6,.14f);
            else if(encounterType==3&&!m.child&&e.variant==9)
            {for(int i=0;i<6;i++){float a=i*Mathf.PI/3+e.age*.15f;Bolt(p,new Vector2(Mathf.Sin(a),Mathf.Cos(a))*2.9f,false,.12f);}}
            else if(encounterType==2&&m.role==1)Fan(p,aim,2,18,3.3f);
            else Fan(p,aim,m.role==0?3:2,16,4.3f,.13f);
            if(ActiveBullets>bulletsBefore){EncounterFireEvents++;Play(IonArt.Impact,.95f);}
        }
        float EncounterDamage(Enemy e,float damage)
        {
            if(encounterRunning&&encounterType==0&&EncounterOwns(e))
            {
                var shield=EncounterRole(0);if(shield!=null&&shield!=e&&(shield.position-e.position).sqrMagnitude<13){Ring(e.position,IonArt.Cyan,.8f,.12f);return damage*.65f;}
            }
            return damage;
        }
        void OnEncounterKilled(Enemy e)
        {
            if(!encounterRunning)return;var m=FindEncounterMember(e);if(m==null)return;
            if(encounterType==4&&m.role==0&&!encounterAlarmTriggered)
            {
                encounterAlarmTriggered=true;EncounterAlarmRewards++;bool full=Ammo>=IonCampaign.MaxAmmo;if(full)Score+=300;else Ammo=Mathf.Min(IonCampaign.MaxAmmo,Ammo+20);
                hud.Toast("ĐÃ CẮT BÁO ĐỘNG / "+(full?"ĐIỂM +300":"ĐẠN +20"));
            }
            if(encounterType==0||encounterType==2)HideEncounterLinks();
        }
    }
}
