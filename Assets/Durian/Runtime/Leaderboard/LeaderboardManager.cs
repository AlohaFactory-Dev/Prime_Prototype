using System;
using System.Collections.Generic;
using Aloha.Coconut;
using Alohacorp.Durian.Client;
using Alohacorp.Durian.Model;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Aloha.Durian
{
    public class LeaderboardManager
    {
        public IReadOnlyList<string> LeaderboardNames { get; set; }
        public IReadOnlyDictionary<string, bool> IsLeaderboardLoading => _isLeaderboardLoading;

        private readonly AuthManager _authManager;
        private readonly Dictionary<string, string> _leaderboardIds = new Dictionary<string, string>();
        private readonly Dictionary<string, Leaderboard> _leaderboards = new Dictionary<string, Leaderboard>();
        private readonly Dictionary<string, List<LeaderboardReward>> _rewardTables = new Dictionary<string, List<LeaderboardReward>>();
        private readonly Dictionary<string, bool> _isLeaderboardLoading = new Dictionary<string, bool>();
        
        public bool IsInitialized => _isInitialized;
        private bool _isInitialized;

        public LeaderboardManager(DurianConfig durianConfig, AuthManager authManager)
        {
            _authManager = authManager;

            foreach (DurianConfig.LeaderboardId leaderboardId in durianConfig.leaderboardIds)
            {
                _leaderboardIds.Add(leaderboardId.name, leaderboardId.Id);
            }
            LeaderboardNames = new List<string>(_leaderboardIds.Keys);
            UniTask.WaitUntil(() => !string.IsNullOrEmpty(_authManager.UID)).ContinueWith(() => _isInitialized = true);
        }

        public async UniTask<Leaderboard> GetLeaderboard(string leaderboardName)
        {
            while (!IsInitialized)
            {
                Debug.LogWarning("LeaderboardManager is not initialized. Waiting for UID to be set.");
                await UniTask.Yield();
            }
            
            if (_leaderboards.ContainsKey(leaderboardName) && !_leaderboards[leaderboardName].IsValid)
            {
                _leaderboards.Remove(leaderboardName);
            }

            if (!_leaderboards.ContainsKey(leaderboardName))
            {
                _isLeaderboardLoading[leaderboardName] = true;

                var leaderboardApi = await DurianApis.LeaderboardApi();
                List<LeaderboardPeriodDto> leaderboardPeriodDtos = await RequestHandler.Request(
                    leaderboardApi.GetLeaderboardPeriodsAsync(_leaderboardIds[leaderboardName]),
                    resp =>
                    {
                        var deserialized =
                            JsonConvert.DeserializeObject<List<LeaderboardPeriodDto>>(resp.Data.Content.ToString());
                        return deserialized;
                    });

                var targetLeaderboardPeriodDto = leaderboardPeriodDtos.Find(x =>
                    x.StartAt.Value.ToDateTime() <= Clock.NoDebugNow && Clock.NoDebugNow <= x.EndAt.Value.ToDateTime());

                Leaderboard leaderboard = LeaderboardEntityFactory.CreateLeaderboard(targetLeaderboardPeriodDto, leaderboardName);
                _leaderboards[leaderboardName] = leaderboard;

                leaderboard.SetIsUpdating(true); // 
                try
                {
                    await UniTask.WhenAll(RefreshLeaderboard(leaderboardName), FetchMyRank(leaderboardName));
                }
                finally
                {
                    leaderboard.SetIsUpdating(false); // 
                }

                _isLeaderboardLoading[leaderboardName] = false;
            }

            return _leaderboards[leaderboardName];
        }

        public async UniTask UpdateMyScore(string leaderboardName, int score)
        {
            LeaderboardPeriodRecordDto result = await InternalUpdateScore(leaderboardName, _authManager.UID, score);
            (await GetLeaderboard(leaderboardName)).MyEntry = LeaderboardEntityFactory.CreateEntry(result);
        }

        private async UniTask<LeaderboardPeriodRecordDto> InternalUpdateScore(string leaderboardName, string playerUID, int score)
        {
            Leaderboard leaderboard = await GetLeaderboard(leaderboardName);

            leaderboard.SetIsUpdating(true);
            LeaderboardPeriodRecordUpdateReqDto recordUpdateReqDto = new LeaderboardPeriodRecordUpdateReqDto(playerUID, score);
            LeaderboardPeriodRecordDto result = await RequestHandler.Request((await DurianApis.LeaderboardApi()).UpdateLeaderboardPeriodRecordAsync(leaderboard.LeaderboardId, leaderboard.PeriodId, recordUpdateReqDto),
                rootDto => rootDto.Data);
            leaderboard.SetIsUpdating(false);

            Debug.Log($"UpdateScore: {result.ParticipantId} {result.Score}, Rank {result.Rank}");
            return result;
        }

        public async UniTask UpdateScore(string leaderboardName, string playerUID, int score)
        {
            await InternalUpdateScore(leaderboardName, playerUID, score);
        }

        public async UniTask FetchMyRank(string leaderboardName)
        {
            Leaderboard leaderboard = await GetLeaderboard(leaderboardName);
            leaderboard.SetIsUpdating(true);

            try
            {
                LeaderboardPeriodRecordDto myRecord = await InternalGetRecord(leaderboardName, _authManager.UID);
                leaderboard.MyEntry =
                    myRecord != null ? LeaderboardEntityFactory.CreateEntry(myRecord) : new LeaderboardEntry(_authManager.UID);
            }
            finally
            {
                leaderboard.SetIsUpdating(false);
            }
        }

        private async UniTask<LeaderboardPeriodRecordDto> InternalGetRecord(string leaderboardName, string playerUID)
        {
            Leaderboard leaderboard = await GetLeaderboard(leaderboardName);

            try
            {
                List<LeaderboardPeriodRecordDto> result = await RequestHandler.Request(
                    (await DurianApis.LeaderboardApi()).QueryParticipantLeaderboardAsync(leaderboard.LeaderboardId, leaderboard.PeriodId,
                        playerUID, 0),
                    rootDto =>
                    {
                        var deserialized =
                            JsonConvert.DeserializeObject<List<LeaderboardPeriodRecordDto>>(rootDto.Data.Content
                                .ToString());
                        return deserialized;
                    });

                return result.Find(record => record.ParticipantId == _authManager.UID);
            }
            catch (ApiException apiException)
            {
                if (apiException.ErrorCode == 404)
                {
                    // 아직 등록되지 않은 유저
                    return null;
                }

                throw;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        public async UniTask<int> GetMyRank(string leaderboardName, string playerUID)
        {
            LeaderboardPeriodRecordDto record = await InternalGetRecord(leaderboardName, playerUID);
            if (record != null) return (int)record.Rank;
            return int.MaxValue;
        }

        public async UniTask RefreshLeaderboard(string leaderboardName, int fetchCount = 50)
        {
            Leaderboard leaderboard = await GetLeaderboard(leaderboardName);
            leaderboard.SetIsUpdating(true); // 업데이트 시작
            try
            {
                List<LeaderboardPeriodRecordDto> result = await RequestHandler.Request(
                    (await DurianApis.LeaderboardApi()).QueryLeaderboardPeriodAsync(leaderboard.LeaderboardId, leaderboard.PeriodId, 0,
                        fetchCount),
                    rootDto =>
                    {
                        var deserialized =
                            JsonConvert.DeserializeObject<List<LeaderboardPeriodRecordDto>>(rootDto.Data.Content
                                .ToString());
                        return deserialized;
                    });

                result.Sort((a, b) => a.Rank.Value.CompareTo(b.Rank.Value));
                leaderboard.Entries.Clear();
                foreach (LeaderboardPeriodRecordDto record in result)
                {
                    if (record.Participant.IsBot)
                    {
                        record.Participant.Name = record.ParticipantId;
                    }

                    leaderboard.Entries.Add(LeaderboardEntityFactory.CreateEntry(record));
                    if (record.ParticipantId == _authManager.UID)
                    {
                        leaderboard.MyEntry = LeaderboardEntityFactory.CreateEntry(record);
                    }
                }
            }
            finally
            {
                leaderboard.SetIsUpdating(false);
            }

            leaderboard.ResetLastUpdateAt();
        }

        // 기본적으로 리더보드를 받아오는 GetLeaderboardPeriodsAsync API로는 리워드를 받아올 수 없음
        // 리워드를 받아오기 위해서는 따로 GetLeaderboardAsync의 ResetRule로부터 리워드 테이블을 받아와야 함
        public async UniTask<List<LeaderboardReward>> GetRewardTable(string leaderboardName)
        {
            if (!_rewardTables.ContainsKey(leaderboardName))
            {
                LeaderboardDto leaderboardDto = await RequestHandler.Request((await DurianApis.LeaderboardApi()).GetLeaderboardAsync(_leaderboardIds[leaderboardName]),
                    resp => resp.Data);

                foreach (LeaderboardRewardConditionDto leaderboardRewardConditionDto in leaderboardDto.ResetRule.RewardConditions)
                {
                    if (!_rewardTables.ContainsKey(leaderboardName))
                    {
                        _rewardTables.Add(leaderboardName, new List<LeaderboardReward>());
                    }

                    _rewardTables[leaderboardName].Add(LeaderboardEntityFactory.CreateReward(leaderboardRewardConditionDto));
                }
            }

            return _rewardTables[leaderboardName];
        }

        public void RegisterLeaderboardId(string leaderboardName, string leaderboardId)
        {
            if (_leaderboardIds.ContainsKey(leaderboardName))
            {
                _leaderboardIds.Remove(leaderboardName);
                if (_leaderboards.ContainsKey(leaderboardName)) _leaderboards.Remove(leaderboardName);
                if (_rewardTables.ContainsKey(leaderboardName)) _rewardTables.Remove(leaderboardName);
                if (_isLeaderboardLoading.ContainsKey(leaderboardName)) _isLeaderboardLoading.Remove(leaderboardName);
            }

            _leaderboardIds[leaderboardName] = leaderboardId;
        }

        public bool IdExists(string leaderboardName)
        {
            return _leaderboardIds.ContainsKey(leaderboardName);
        }
    }
}

