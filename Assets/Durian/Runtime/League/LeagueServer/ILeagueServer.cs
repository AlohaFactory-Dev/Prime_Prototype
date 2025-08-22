using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Aloha.Durian
{
    public interface ILeagueServer<TPlayerData> where TPlayerData : PlayerPublicData
    {
        IReadOnlyReactiveProperty<bool> IsInitialized { get; }
        string PlayerUID { get; }
        UniTask<string> GetCurrentSeasonId();
        UniTask<bool> IsPlayerJoined(string seasonId);
        UniTask<LeagueDivision> Join(string seasonId);
        UniTask<LeagueDivision> GetLeagueDivision(string seasonId);
        UniTask<Leaderboard> RefreshLeaderboard(LeagueDivision pvpGroup);
        UniTask UploadScoreDeltas(LeagueDivision leagueDivision, params PlayerScoreDelta[] playerScoreDeltas);
        UniTask<TPlayerData> GetPlayerData(LeaderboardEntry leaderboardEntry);
        UniTask<Dictionary<LeagueEnum, List<LeaderboardReward>>> GetDailyRewards(LeagueDivision pvpGroup);
        UniTask<Dictionary<LeagueEnum, List<LeaderboardReward>>> GetSeasonRewards(LeagueDivision pvpGroup);
    }
}
