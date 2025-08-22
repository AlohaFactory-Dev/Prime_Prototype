using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Aloha.Coconut
{
    public struct RushEventMissionGroupData
    {
        public int totalRound;
        public List<Property> roundRewards;
        public List<RushEventMissionData> missionDatas;
        public string leaderboardName;
    }
    
    public class RushEventMissionGroup
    {
        public int TotalRound { get; }
        public int CurrentRound => _saveData.currentRound;
        public List<Property> RoundRewards { get; }
        public List<RushEventMission> Missions { get; }
        public bool IsRoundRewardsClaimable => !IsRoundRewardsClaimed && Missions.All(m => m.IsClaimed);
        public bool IsRoundRewardsClaimed => _saveData.isRoundClaimed;
        public string RoundRewardsRedDotPath { get; private set; }
        public string LeaderboardName { get; }
        public int Progress => _saveData.progress;

        public IObservable<int> OnProgressChanged => _onProgressChanged;
        private readonly Subject<int> _onProgressChanged = new Subject<int>();
        
        private readonly SaveData _saveData;
        private readonly PropertyManager _propertyManager;

        private CompositeDisposable _disposables = new CompositeDisposable();

        private RushEventMissionGroup(RushEventMissionGroupData data, SaveData saveData, List<RushEventMission> missions, PropertyManager propertyManager)
        {
            _saveData = saveData;
            _propertyManager = propertyManager;

            Missions = missions;
            TotalRound = data.totalRound;
            RoundRewards = data.roundRewards;
            LeaderboardName = data.leaderboardName;

            // 초기 라운드 설정
            _saveData.currentRound = _saveData.currentRound == 0 ? 1 : _saveData.currentRound;

            Missions.Sort((a, b) => a.Objective.CompareTo(b.Objective));
            
            for(var i = 0; i < Missions.Count; i++)
            {
                int index = i;
                Missions[index].Progress = GetCurrentRoundProgress();
                Missions[index].IsClaimed = _saveData.claimedMissionIdx.Contains(index);
                Missions[index].OnClaimed.Subscribe(_ =>
                {
                    _saveData.claimedMissionIdx.Add(index);
                    Missions[index].IsClaimed = true;
                    Missions[index].UpdateRedDot();
                    UpdateRedDot();
                }).AddTo(_disposables);
            }
        }

        private int GetCurrentRoundProgress()
        {
            return _saveData.progress - (CurrentRound - 1) * Missions[^1].Objective;
        }

        internal void AddProgress(int progress)
        {
            _saveData.progress += progress;
            foreach (var mission in Missions)
            {
                mission.Progress = GetCurrentRoundProgress();
            }

            _onProgressChanged.OnNext(GetCurrentRoundProgress());
        }

        public List<Property> ClaimRoundRewards(PlayerAction playerAction)
        {
            if (!IsRoundRewardsClaimable) return null;
            
            List<Property> result = _propertyManager.Obtain(RoundRewards, playerAction);
            if (CurrentRound == TotalRound) // 마지막 라운드
            {
                _saveData.isRoundClaimed = true;
            }
            else
            {
                _saveData.currentRound++;
                _saveData.isRoundClaimed = false;
                _saveData.claimedMissionIdx.Clear();
                foreach (var mission in Missions)
                {
                    mission.Progress = GetCurrentRoundProgress();
                    mission.IsClaimed = false;
                }
            }

            UpdateRedDot();
            
            return result;
        }
        
        public void LinkRedDot(string path)
        {
            RoundRewardsRedDotPath = $"{path}/RoundRewards";
            
            foreach (var mission in Missions)
            {
                mission.LinkRedDot($"{path}/{mission.Objective}");
            }
            
            UpdateRedDot();
        }
        
        private void UpdateRedDot()
        {
            if (string.IsNullOrEmpty(RoundRewardsRedDotPath)) return;
            RedDot.SetNotified(RoundRewardsRedDotPath, IsRoundRewardsClaimable);
        }

        internal void Dispose()
        {
            _disposables.Dispose();
            _onProgressChanged.Dispose();
        }
        
        public class SaveData
        {
            public int progress;
            public int currentRound = 1;
            public bool isRoundClaimed;
            public List<int> claimedMissionIdx = new List<int>();
        }

        public class Factory
        {
            private readonly PropertyManager _propertyManager;
            private readonly IRushEventDatabase _rushEventDatabase;

            public Factory(PropertyManager propertyManager, IRushEventDatabase rushEventDatabase)
            {
                _propertyManager = propertyManager;
                _rushEventDatabase = rushEventDatabase;
            }
            
            public RushEventMissionGroup Create(RushEventMissionGroupData data, SaveData saveData)
            {
                List<RushEventMission> rushEventMissions = new List<RushEventMission>();
                foreach (var rushEventMissionData in data.missionDatas)
                {
                    var mission = new RushEventMission(rushEventMissionData, _propertyManager);
                    mission.LinkRedDot($"{_rushEventDatabase.GetRedDotPath()}/Mission/{mission.Objective}");
                    rushEventMissions.Add(mission);
                }
                
                return new RushEventMissionGroup(data, saveData, rushEventMissions, _propertyManager);
            }
        }
    }
}
