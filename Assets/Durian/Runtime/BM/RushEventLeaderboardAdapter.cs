using System.Collections.Generic;
using Aloha.Coconut;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Aloha.Durian
{
    public class RushEventLeaderboardAdapter : IRushEventLeaderboardAdapter
    {
        public ReactiveProperty<bool> IsUpdating { get; } = new ReactiveProperty<bool>(false);
        
        private readonly RushEventManager _rushEventManager;
        private readonly LeaderboardManager _leaderboardManager;

        public RushEventLeaderboardAdapter(LeaderboardManager leaderboardManager)
        {
            _leaderboardManager = leaderboardManager;
        }

        public async UniTask UpdateScore(RushEvent rushEvent)
        {
            await _leaderboardManager.UpdateMyScore(rushEvent.MissionGroup.LeaderboardName, rushEvent.MissionGroup.Progress);
            await _leaderboardManager.RefreshLeaderboard(rushEvent.MissionGroup.LeaderboardName);
        }
        
        public UniTask<Leaderboard> GetLeaderboard(RushEvent rushEvent)
        {
            return _leaderboardManager.GetLeaderboard(rushEvent.MissionGroup.LeaderboardName);
        }
        
        public UniTask<List<LeaderboardReward>> GetRewardTable(RushEvent rushEvent)
        {
            return _leaderboardManager.GetRewardTable(rushEvent.MissionGroup.LeaderboardName);
        }
    }
}