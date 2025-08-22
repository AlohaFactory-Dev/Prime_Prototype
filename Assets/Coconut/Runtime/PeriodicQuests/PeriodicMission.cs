using Aloha.Coconut.Missions;

namespace Aloha.Coconut.PeriodicQuests
{
    public class PeriodicMission
    {
        public bool IsCompleted => Mission.IsRewardsClaimed;
    
        public Mission Mission { get; }
        public int QuestPoint { get; }

        public PeriodicMission(Mission mission, int questPoint)
        {
            Mission = mission;
            QuestPoint = questPoint;
        }

        internal void Complete(PlayerAction playerAction)
        {
            Mission.Claim(playerAction);
        }
    }
}