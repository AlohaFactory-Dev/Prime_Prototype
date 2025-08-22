using System.Collections.Generic;
using System.Numerics;
using Aloha.Coconut.Missions;

namespace Aloha.Coconut.PeriodicQuests
{
    public struct PeriodicQuestMissionData
    {
        [CSVColumn] public int id;
        [CSVColumn] public MissionType type;
        [CSVColumn] public int var;
        [CSVColumn] public BigInteger objective;
        [CSVColumn] public int point;

        public MissionData GetMissionData()
        {
            return new MissionData
            {
                id = id,
                type = type,
                var = var,
                objective = objective,
                rewards = new List<Property>()
            };
        }
    }
}