using UnityEngine;

namespace Aloha.Coconut.PeriodicQuests
{
    [CreateAssetMenu(fileName = "PeriodicQuestConfig", menuName = "Coconut/Config/PeriodicQuestConfig")]
    public class PeriodicQuestConfig : CoconutConfig
    {
        public string dailyQuestRedDotPath;
        public string weeklyQuestRedDotPath;
        
        public string GetRedDotPath(ResetPeriod resetPeriod)
        {
            return resetPeriod == ResetPeriod.Daily ? dailyQuestRedDotPath : weeklyQuestRedDotPath;
        }
    }
}
