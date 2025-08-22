using System;
using System.Collections.Generic;
using Aloha.Coconut;
using Cysharp.Threading.Tasks;
using UniRx;
using Random = UnityEngine.Random;

namespace Aloha.Durian
{
    public class MockLeagueServer<TPlayerData> : ILeagueServer<TPlayerData>, IDisposable where TPlayerData : PlayerPublicData
    {
        public IReadOnlyReactiveProperty<bool> IsInitialized => _isInitialized;
        private ReactiveProperty<bool> _isInitialized = new ReactiveProperty<bool>(false);
        public string PlayerUID { get; private set; }

        private readonly IDisposable _disposable;

        private LeagueDivision _pvpGroup;
        private List<PvPOpponentEntry<TPlayerData>> _opponents;
        private ILeagueBotDataGenerator<TPlayerData> _botDataGenerator;

        public MockLeagueServer(AuthManager authManager, ILeagueBotDataGenerator<TPlayerData> botDataGenerator)
        {
            _botDataGenerator = botDataGenerator;
            _disposable = authManager.IsSignedIn
                .Where(isSignedIn => isSignedIn)
                .First()
                .Subscribe(_ =>
                {
                    PlayerUID = authManager.UID;
                    _isInitialized.Value = true;
                });
        }

        public async UniTask<string> GetCurrentSeasonId()
        {
            return "test_season";
        }

        public async UniTask<bool> IsPlayerJoined(string seasonId)
        {
            if (!IsInitialized.Value) throw new InvalidOperationException("PvPGroup is not initialized.");

            await UniTask.Delay(TimeSpan.FromSeconds(1));
            return _pvpGroup != null && _pvpGroup.endTime > Clock.Now;
        }

        public async UniTask<LeagueDivision> Join(string seasonId)
        {
            if (!IsInitialized.Value) throw new InvalidOperationException("PvPGroup is not initialized.");

            if (await IsPlayerJoined(seasonId)) return _pvpGroup;

            Leaderboard leaderboard = new Leaderboard("test_pvp");
            leaderboard.MyEntry = new LeaderboardEntry(PlayerUID, 0, 30);

            for (int i = 0; i < 29; i++)
            {
                LeaderboardEntry newEntry = new LeaderboardEntry($"test_uid_{i}", 310 - 10 * (i + 1), (i + 1));
                leaderboard.Entries.Add(newEntry);
            }

            leaderboard.Entries.Add(leaderboard.MyEntry);

            GameDate gameDateNow = Clock.GameDateNow;
            _pvpGroup = new LeagueDivision("test_group_id", "test_season_id", "test_league_id", $"test_division_{Random.Range(0, 9999)}",
                gameDateNow.StartDateTime, gameDateNow.AddDay(1).EndDateTime,
                LeagueEnum.Bronze, leaderboard);

            return _pvpGroup;
        }

        public async UniTask<LeagueDivision> GetLeagueDivision(string seasonId)
        {
            if (!IsInitialized.Value) throw new InvalidOperationException("PvPGroup is not initialized.");

            if (!await IsPlayerJoined(seasonId)) throw new InvalidOperationException("Player not joined.");
            return _pvpGroup;
        }

        public async UniTask<Leaderboard> RefreshLeaderboard(LeagueDivision pvpGroup)
        {
            return _pvpGroup.leaderboard;
        }

        public async UniTask UploadScoreDeltas(LeagueDivision pvpGroup, params PlayerScoreDelta[] playerScoreDeltas)
        {
            if (!IsInitialized.Value) throw new InvalidOperationException("PvPGroup is not initialized.");

            await UniTask.Delay(TimeSpan.FromSeconds(1));

            foreach (var playerScoreDelta in playerScoreDeltas)
            {
                _pvpGroup.leaderboard.Entries.Find(entry => entry.UID == playerScoreDelta.uid).Score += playerScoreDelta.scoreDelta;
            }

            _pvpGroup.leaderboard.Entries.Sort((a, b) => b.Score.CompareTo(a.Score));
            for (int i = 0; i < _pvpGroup.leaderboard.Entries.Count; i++)
            {
                _pvpGroup.leaderboard.Entries[i].Rank = i + 1;
            }
        }

        public async UniTask<TPlayerData> GetPlayerData(LeaderboardEntry leaderboardEntry)
        {
            return _botDataGenerator.GetBot(leaderboardEntry.Nickname).GetPlayerData();
        }

        public UniTask<Dictionary<LeagueEnum, List<LeaderboardReward>>> GetDailyRewards(LeagueDivision pvpGroup)
        {
            throw new NotImplementedException();
        }

        public UniTask<Dictionary<LeagueEnum, List<LeaderboardReward>>> GetSeasonRewards(LeagueDivision pvpGroup)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}