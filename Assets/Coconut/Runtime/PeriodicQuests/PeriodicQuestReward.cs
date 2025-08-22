using System.Numerics;

namespace Aloha.Coconut.PeriodicQuests
{
    public class PeriodicQuestReward
    {
        public enum State
        {
            Locked,
            Claimable,
            Claimed
        }
        
        [CSVColumn] public int requiredPoint;
        public Property Reward => new (rewardTypeGroup, rewardTypeId, rewardAmount);

        [CSVColumn] public PropertyTypeGroup rewardTypeGroup;
        [CSVColumn] public int rewardTypeId;
        [CSVColumn] public BigInteger rewardAmount;

        public State RewardState { get; internal set; }
    }
}