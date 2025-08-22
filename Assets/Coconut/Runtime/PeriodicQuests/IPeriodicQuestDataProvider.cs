using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Aloha.Coconut.PeriodicQuests
{
    public interface IPeriodicQuestDataProvider
    {
        List<PeriodicQuestReward> GetRewards();
        List<PeriodicQuestMissionData> GetMissions();
    }

    internal class DefaultPeriodicQuestDataProvider : IPeriodicQuestDataProvider
    {
        private readonly ResetPeriod _resetPeriod;

        public DefaultPeriodicQuestDataProvider(ResetPeriod resetPeriod)
        {
            _resetPeriod = resetPeriod;
        }
        
        public List<PeriodicQuestReward> GetRewards()
        {
            var rewards = _resetPeriod == ResetPeriod.Daily
                ? TableManager.Get<PeriodicQuestReward>("quest_rewards_daily")
                : TableManager.Get<PeriodicQuestReward>("quest_rewards_weekly");
            
            // Assert point not duplicated
            var pointSet = new HashSet<int>();
            foreach (var reward in rewards)
            {
                Assert.IsFalse(pointSet.Contains(reward.requiredPoint), $"Duplicated point in periodic quest rewards: {_resetPeriod}");
                pointSet.Add(reward.requiredPoint);
            }
            
            return rewards;
        }

        public List<PeriodicQuestMissionData> GetMissions()
        {
            var missionDatas = _resetPeriod == ResetPeriod.Daily
                ? TableManager.Get<PeriodicQuestMissionData>("quest_missions_daily")
                : TableManager.Get<PeriodicQuestMissionData>("quest_missions_weekly");
            
            // Assert id not duplicated
            var idSet = new HashSet<int>();
            foreach (var missionData in missionDatas)
            {
                Assert.IsFalse(idSet.Contains(missionData.id), $"Duplicated id in periodic quest missions: {_resetPeriod}");
                idSet.Add(missionData.id);
            }
            
            return missionDatas;
        }
    }
}
