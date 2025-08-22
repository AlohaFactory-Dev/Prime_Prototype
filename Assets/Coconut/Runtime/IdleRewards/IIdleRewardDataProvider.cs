using System.Collections.Generic;

namespace Aloha.Coconut.IdleRewards
{
    public interface IIdleRewardDataProvider
    {
        string Id { get; }
        string RedDotPath { get; }

        int IdleHoursMax { get; }
        int RewardGenerationSeconds { get; }

        Property QuickEarningCost { get; }
        int QuickEarningHours { get; }
        int QuickEarningPerDay { get; }
        int RVQuickEarningsPerDay { get; }
        int QuickEarningRVPlacementId { get; }
        string QuickEarningRVPlacementName { get; }

        List<IdleReward> GetIdleRewardsPerGeneration();
    }
}
