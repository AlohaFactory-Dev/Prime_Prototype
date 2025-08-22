namespace Aloha.Coconut
{
    public interface IDailyFreeRewardsManager
    {
        public LimitedProduct GetDailyFreeRewards(string key, string redDotPath);
    }
}
