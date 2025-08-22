using System;
using System.Collections.Generic;
using Aloha.Coconut;
using Alohacorp.Durian.Api;
using Alohacorp.Durian.Client;
using Alohacorp.Durian.Model;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UniRx;
using UnityEngine;
using Zenject;

namespace Aloha.Durian
{
    public class DurianLeagueServer<TPlayerData> : ILeagueServer<TPlayerData>, IDisposable where TPlayerData : PlayerPublicData
    {
        public IReadOnlyReactiveProperty<bool> IsInitialized => _isInitialized;
        private ReactiveProperty<bool> _isInitialized = new ReactiveProperty<bool>(false);
        public string PlayerUID { get; private set; }

        private string PlayerPublicDataSavePath { get; }
        private string LeagueGroupId { get; }
        private string LeaderboardName => $"league_{LeagueGroupId}";

        private readonly IDisposable _disposable;

        private readonly OtherPlayerDataManager _otherPlayerDataManager;
        private readonly LeaderboardManager _leaderboardManager;
        private readonly ILeagueBotDataGenerator<TPlayerData> _leagueBotDataGenerator;

        private DateTime _seasonEndTime;

        private string _rewardsCachedSeasonId;
        private Dictionary<LeagueEnum, List<LeaderboardReward>> _dailyRewardsCache;
        private Dictionary<LeagueEnum, List<LeaderboardReward>> _seasonRewardsCache;

        public DurianLeagueServer(DurianConfig durianConfig, AuthManager authManager,
            OtherPlayerDataManager otherPlayerDataManager, LeaderboardManager leaderboardManager,
            ILeagueBotDataGenerator<TPlayerData> leagueBotDataGenerator,
            [Inject(Id = "league_group_id")] string leagueGroupId)
        {
            _otherPlayerDataManager = otherPlayerDataManager;
            _leaderboardManager = leaderboardManager;
            _leagueBotDataGenerator = leagueBotDataGenerator;

            PlayerPublicDataSavePath = durianConfig.publicDataSavePath;
            LeagueGroupId = leagueGroupId;

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
            if (!IsInitialized.Value) throw new InvalidOperationException("LeagueServer is not initialized.");

            var leagueApi = await DurianApis.LeagueApi();
            return await RequestHandler.Request(leagueApi.GetLeagueGroupsAsync(), resp =>
            {
                var results = (JArray)resp.Data.Content;
                foreach (var jToken in results.Children())
                {
                    if (jToken["id"].ToString() == LeagueGroupId)
                    {
                        if (!jToken["currentSeason"].HasValues) return null;
                        return jToken["currentSeason"]["id"].ToString();
                    }
                }

                return null;
            });
        }

        public async UniTask<bool> IsPlayerJoined(string seasonId)
        {
            if (!IsInitialized.Value) throw new InvalidOperationException("LeagueServer is not initialized.");
            if (Clock.NoDebugNow < _seasonEndTime)
            {
                return true;
            }

            try
            {
                var leagueApi = await DurianApis.LeagueApi();
                LeagueDivisionDto leagueDivisionDto = await RequestHandler.Request(leagueApi.GetPlayerLeagueDivisionAsync(LeagueGroupId, seasonId),
                    resp => resp.Data);

                _seasonEndTime = leagueDivisionDto.Leaderboard.InstantPeriod.EndAt == null 
                    ? leagueDivisionDto.Leaderboard.InstantPeriod.EndAt.Value.ToDateTime() 
                    : new DateTime(1970, 1, 1);
                
                return true;
            }
            catch (ApiException e)
            {
                // GetPlayerLeagueDivisionAsync에서 404 리턴되면 Player가 가입되지 않은 것으로 판단
                if (e.ErrorCode == 404)
                {
                    Debug.Log("Player Not joined");
                    return false;
                }

                Debug.Log($"LeagueServer Error: {e.ErrorCode} {e.Message}");
                throw e;
            }
        }

        public async UniTask<LeagueDivision> Join(string seasonId)
        {
            if (!IsInitialized.Value) throw new InvalidOperationException("LeagueServer is not initialized.");

            var leagueApi = await DurianApis.LeagueApi();
            var leagueDivisionDto = await RequestHandler.Request(leagueApi.JoinLeagueSeasonAsync(LeagueGroupId, seasonId),
                resp => resp.Data);
            Debug.Log($"Player joined league division: {leagueDivisionDto.LeagueId}");
            return await CreateLeagueDivision(leagueDivisionDto);
        }

        private async UniTask<LeagueDivision> CreateLeagueDivision(LeagueDivisionDto leagueDivisionDto)
        {
            _leaderboardManager.RegisterLeaderboardId(LeaderboardName, leagueDivisionDto.LeaderboardId);
            Leaderboard leaderboard = await _leaderboardManager.GetLeaderboard(LeaderboardName);

            LeagueEnum league = leagueDivisionDto.League.Tier == null
                ? LeagueEnum.Bronze
                : (LeagueEnum)leagueDivisionDto.League.Tier;

            DateTime startTime = DateTimeOffset.FromUnixTimeMilliseconds(leagueDivisionDto.Leaderboard.InstantPeriod.StartAt.Value).DateTime;
            DateTime endTime = DateTimeOffset.FromUnixTimeMilliseconds(leagueDivisionDto.Leaderboard.InstantPeriod.EndAt.Value).DateTime;

            return new LeagueDivision(LeagueGroupId, leagueDivisionDto.League.LeagueSeasonId, leagueDivisionDto.LeagueId, leagueDivisionDto.Id,
                startTime, endTime, league, leaderboard);
        }

        public async UniTask<LeagueDivision> GetLeagueDivision(string seasonId)
        {
            if (!IsInitialized.Value) throw new InvalidOperationException("LeagueServer is not initialized.");

            var leagueApi = await DurianApis.LeagueApi();
            var leagueDivisionDto = await RequestHandler.Request(leagueApi.GetPlayerLeagueDivisionAsync(LeagueGroupId, seasonId),
                resp => resp.Data);

            return await CreateLeagueDivision(leagueDivisionDto);
        }

        public async UniTask<Leaderboard> RefreshLeaderboard(LeagueDivision leagueDivision)
        {
            if (!IsInitialized.Value) throw new InvalidOperationException("LeagueServer is not initialized.");
            await _leaderboardManager.RefreshLeaderboard(LeaderboardName);
            return leagueDivision.leaderboard;
        }

        public async UniTask UploadScoreDeltas(LeagueDivision leagueDivision, params PlayerScoreDelta[] playerScoreDeltas)
        {
            if (!IsInitialized.Value) throw new InvalidOperationException("LeagueServer is not initialized.");

            var leagueApi = await DurianApis.LeagueApi();
            List<UniTask> uploadTasks = new List<UniTask>();
            foreach (var playerScoreDelta in playerScoreDeltas)
            {
                LeagueDivisionRecordUpdateReqDto playerRecordUpdateReqDto = new LeagueDivisionRecordUpdateReqDto(playerScoreDelta.uid, playerScoreDelta.scoreDelta);
                UniTask uploadPlayerData = RequestHandler.Request(leagueApi.UpdateDivisionRecordAsync(LeagueGroupId,
                    leagueDivision.leagueSeasonId, leagueDivision.leagueId, leagueDivision.divisionId, playerRecordUpdateReqDto),
                resp => resp.Data);
                uploadTasks.Add(uploadPlayerData);
            }

            await UniTask.WhenAll(uploadTasks);
            await _leaderboardManager.RefreshLeaderboard(LeaderboardName);
        }

        public async UniTask<TPlayerData> GetPlayerData(LeaderboardEntry leaderboardEntry)
        {
            if (!leaderboardEntry.IsBot)
            {
                return await _otherPlayerDataManager.Get<TPlayerData>(leaderboardEntry.UID, PlayerPublicDataSavePath);
            }

            return GetBotData(leaderboardEntry);
        }

        private TPlayerData GetBotData(LeaderboardEntry leaderboardEntry)
        {
            return _leagueBotDataGenerator.GetBot(leaderboardEntry.Nickname).GetPlayerData();
        }

        public async UniTask<Dictionary<LeagueEnum, List<LeaderboardReward>>> GetDailyRewards(LeagueDivision leagueDivision)
        {
            if (_rewardsCachedSeasonId != leagueDivision.leagueSeasonId || _dailyRewardsCache == null)
            {
                await CacheRewards(leagueDivision);
            }

            return _dailyRewardsCache;
        }

        public async UniTask<Dictionary<LeagueEnum, List<LeaderboardReward>>> GetSeasonRewards(LeagueDivision leagueDivision)
        {
            if (_rewardsCachedSeasonId != leagueDivision.leagueSeasonId || _seasonRewardsCache == null)
            {
                await CacheRewards(leagueDivision);
            }

            return _seasonRewardsCache;
        }

        private async UniTask CacheRewards(LeagueDivision leagueDivision)
        {
            var leagueApi = await DurianApis.LeagueApi();
            LeagueSeasonDto leagueSeasonDto = await RequestHandler.Request(leagueApi.GetLeagueSeasonAsync(leagueDivision.leagueGroupId, leagueDivision.leagueSeasonId),
                resp => resp.Data);

            _dailyRewardsCache = new Dictionary<LeagueEnum, List<LeaderboardReward>>();
            _seasonRewardsCache = new Dictionary<LeagueEnum, List<LeaderboardReward>>();

            foreach (LeagueDto leagueDto in leagueSeasonDto.Leagues)
            {
                LeagueEnum tier = leagueDto.Tier == null ? LeagueEnum.Bronze : (LeagueEnum)leagueDto.Tier;
                _dailyRewardsCache[tier] = new List<LeaderboardReward>();
                _seasonRewardsCache[tier] = new List<LeaderboardReward>();

                foreach (var rewardCondition in leagueDto.DailyRewardConditions)
                {
                    _dailyRewardsCache[tier].Add(LeaderboardEntityFactory.CreateReward(rewardCondition));
                }

                foreach (var rewardCondition in leagueDto.RewardConditions)
                {
                    _seasonRewardsCache[tier].Add(LeaderboardEntityFactory.CreateReward(rewardCondition));
                }
            }

            _rewardsCachedSeasonId = leagueDivision.leagueSeasonId;
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}