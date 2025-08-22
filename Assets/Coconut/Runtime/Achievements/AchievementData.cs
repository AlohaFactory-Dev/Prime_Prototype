using System.Collections.Generic;
using System.Numerics;
using Aloha.Coconut.Missions;

namespace Aloha.Coconut.Achievements
{
    public struct AchievementData
    {
        public MissionData MissionData => new MissionData
        {
            id = group * 10000 + order,
            type = type,
            var = var,
            objective = objective,
            rewards = new List<Property>
            {
                new (rewardTypeAlias, rewardAmount)
            }
        };
        
        [CSVColumn] public int group;
        [CSVColumn] public int order; // 9999까지만 가능
        [CSVColumn] public MissionType type;
        [CSVColumn] public int var;
        [CSVColumn] public BigInteger objective;
        [CSVColumn] public string rewardTypeAlias;
        [CSVColumn] public BigInteger rewardAmount;
    }
}
