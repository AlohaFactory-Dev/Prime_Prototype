using System.Collections.Generic;
using Aloha.Coconut;

namespace Aloha.Durian
{
    public class LeaderboardReward
    {
        public enum ConditionType
        {
            RankRange = 1,
            PercentileRange = 2,
            ScoreRange = 3        
        }
        
        public ConditionType Condition { get; }
        public double MinValue { get; }
        public double MaxValue { get; }
        public List<Property> Rewards { get; }
        
        public LeaderboardReward(ConditionType conditionType, double minValue, double maxValue, List<Property> rewards)
        {
            Condition = conditionType;
            MinValue = minValue;
            MaxValue = maxValue;
            Rewards = rewards;
        }
    }
}
