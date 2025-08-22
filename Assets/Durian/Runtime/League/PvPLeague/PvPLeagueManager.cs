using System;
using System.Collections.Generic;
using Aloha.Coconut;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Aloha.Durian
{
    public class PvPLeagueManager<TPlayerData> : LeagueManager<TPlayerData> where TPlayerData : PlayerPublicData
    {
        public bool IsFetchingOpponents => _isFetchingOpponents;
        private List<PvPOpponentEntry<TPlayerData>> _opponentEntries;

        private readonly IPvPStageController<TPlayerData> _pvpStageController;

        private bool _isFetchingOpponents = false;

        public PvPLeagueManager(ILeagueServer<TPlayerData> leagueServer, SaveDataManager saveDataManager, IMyPublicDataProvider myPublicDataProvider,
            IPvPStageController<TPlayerData> pvpStageController) : base(leagueServer, saveDataManager, myPublicDataProvider)
        {
            _pvpStageController = pvpStageController;
        }

        protected override async UniTask OnLeagueSeasonChanged()
        {
            await GetOpponentEntries();
        }

        public async UniTask<List<PvPOpponentEntry<TPlayerData>>> GetOpponentEntries()
        {
            if (_isFetchingOpponents) await UniTask.WaitWhile(() => _isFetchingOpponents);

            if (_opponentEntries != null) return _opponentEntries;

            _isFetchingOpponents = true;

            List<LeaderboardEntry> entries = new List<LeaderboardEntry>(_leagueDivision.leaderboard.Entries);
            entries.Remove(entries.Find(e => e.UID == _leagueServer.PlayerUID));

            List<LeaderboardEntry> opponentLeaderboardEntries = new List<LeaderboardEntry>();
            for (int i = 0; i < Mathf.Min(5, entries.Count); i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, entries.Count);
                opponentLeaderboardEntries.Add(entries[randomIndex]);
                entries.RemoveAt(randomIndex);
            }

            opponentLeaderboardEntries.Sort((a, b) => b.Score.CompareTo(a.Score));

            List<PvPOpponentEntry<TPlayerData>> opponentEntries = new List<PvPOpponentEntry<TPlayerData>>();
            List<UniTask> playerDataTasks = new List<UniTask>();
            for (int i = 0; i < opponentLeaderboardEntries.Count; i++)
            {
                playerDataTasks.Add(AddPvPOpponentEntry(opponentLeaderboardEntries[i], i, opponentEntries));
            }

            await UniTask.WhenAll(playerDataTasks);
            opponentEntries.Sort((a, b) => b.Rating.CompareTo(a.Rating));

            _opponentEntries = opponentEntries;

            _isFetchingOpponents = false;
            return _opponentEntries;
        }

        public async UniTask<List<PvPOpponentEntry<TPlayerData>>> RefreshOpponentEntries()
        {
            _opponentEntries = null;
            return await GetOpponentEntries();
        }

        private async UniTask AddPvPOpponentEntry(LeaderboardEntry leaderboardEntry, int index, List<PvPOpponentEntry<TPlayerData>> opponentEntries)
        {
            int winScore =
                index == 0 ? 20 :
                index == 1 ? 15 :
                index == 2 ? 10 :
                index == 3 ? 8 :
                5;

            int loseScore =
                index == 0 ? -5 :
                index == 1 ? -8 :
                -10;

            TPlayerData playerData = await _leagueServer.GetPlayerData(leaderboardEntry);
            lock (opponentEntries)
            {
                opponentEntries.Add(new PvPOpponentEntry<TPlayerData>(leaderboardEntry, playerData, playerData.Power, winScore, loseScore));
            }
        }

        public async UniTask PlayPvP(PvPOpponentEntry<TPlayerData> opponentEntry)
        {
            bool isWin = await _pvpStageController.PlayPvP(opponentEntry);

            _isUploadingScore.Value = true;
            int scoreDelta = isWin ? opponentEntry.WinScore : opponentEntry.LoseScore;
            await _leagueServer.UploadScoreDeltas(_leagueDivision,
                new PlayerScoreDelta(_leagueServer.PlayerUID, scoreDelta),
                new PlayerScoreDelta(opponentEntry.UID, -scoreDelta));
            _isUploadingScore.Value = false;

            await RefreshLeaderboard();
        }
    }
}