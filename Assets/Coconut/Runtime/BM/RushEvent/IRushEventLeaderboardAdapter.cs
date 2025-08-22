using System.Collections.Generic;
using Aloha.Durian;
using Cysharp.Threading.Tasks;

namespace Aloha.Coconut
{
    public interface IRushEventLeaderboardAdapter
    {
        UniTask UpdateScore(RushEvent rushEvent);
        UniTask<Leaderboard> GetLeaderboard(RushEvent rushEvent);
        UniTask<List<LeaderboardReward>> GetRewardTable(RushEvent rushEvent);
    }
}
