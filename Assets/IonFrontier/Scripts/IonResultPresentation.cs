using UnityEngine;

namespace IonFrontier
{
    public sealed partial class IonGame
    {
        public int RunHull {get;private set;}
        public int LastCompletedHull {get;private set;}
        public int LastNewHull {get;private set;}=-1;
        public int LastOpenedStage {get;private set;}=-1;
        public int ResultSerial {get;private set;}
        public bool LastRunWon {get;private set;}
        int queuedResultHull=-1;
        void PrepareResultRun(){ApplyQueuedResultHull();RunHull=SelectedHull;LastNewHull=LastOpenedStage=-1;}
        void CommitResultPresentation(bool won,int hullsBefore,int stagesBefore)
        {
            LastCompletedHull=RunHull;LastRunWon=won;LastNewHull=LastOpenedStage=-1;
            if(won)
            {
                int reward=IonCampaign.RewardHull(Sector);
                if(reward>=hullsBefore&&reward<UnlockedHulls)LastNewHull=reward;
                if(UnlockedStages>stagesBefore)LastOpenedStage=UnlockedStages;
            }
            ResultSerial++;
        }
        public void QueueNewResultHull()
        {
            if((State==FlightState.StageClear||State==FlightState.Victory)&&LastRunWon&&LastNewHull>=0&&LastNewHull<UnlockedHulls)queuedResultHull=LastNewHull;
        }
        void ApplyQueuedResultHull()
        {
            if(queuedResultHull>=0&&queuedResultHull<UnlockedHulls){profile.selectedHull=queuedResultHull;IonCampaign.Save(profilePath,profile);}queuedResultHull=-1;
        }
        internal void PlayResultCue(int level,bool reward=false){Play(IonArt.Pickup,reward?1.25f:.95f+level*.09f);}
    }
}
