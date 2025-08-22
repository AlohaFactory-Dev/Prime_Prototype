using System.Collections.Generic;

namespace Aloha.Coconut.Achievements
{
    public interface IAchievementDataProvider
    {
        List<AchievementData> GetAchievementDatas();
    }

    internal class DefaultAchievementDataProvider : IAchievementDataProvider
    {
        public List<AchievementData> GetAchievementDatas()
        {
            return TableManager.Get<AchievementData>("achievements");
        }
    }
}