using System;
using System.Collections.Generic;
using Aloha.Coconut.Missions;
using Zenject;

namespace Aloha.Coconut.PeriodicQuests
{
    public class PeriodicQuest: IDisposable
    {
        public static void InstallTo(DiContainer container, ResetPeriod resetPeriod)
        {
            container.Bind<PeriodicQuest>().WithId(resetPeriod).FromSubContainerResolve()
                .ByMethod(c =>
                {
                    c.Bind<ResetPeriod>().FromInstance(resetPeriod).AsSingle();
                    c.BindInterfacesTo<DefaultPeriodicQuestDataProvider>().AsSingle().NonLazy();
                    c.BindInterfacesAndSelfTo<PeriodicQuest>().AsSingle().NonLazy();
                }).AsCached().NonLazy();
        }
        
        public IReadOnlyList<PeriodicQuestReward> Rewards => _rewards;
        public int PointMax => _rewards[^1].requiredPoint;
        public int CurrentPoint => _saveData.currentPoint;
        public IReadOnlyList<PeriodicMission> PeriodicMissions => _periodicMissions;
        public bool IsQuestOver => CurrentPoint >= PointMax;
        
        private List<PeriodicMission> _periodicMissions = new ();
        private List<PeriodicQuestReward> _rewards = new ();
        
        private readonly PeriodicResetHandler _periodicResetHandler;
        private readonly MissionFactory _missionFactory;
        private readonly PropertyManager _propertyManager;
        private readonly ResetPeriod _resetPeriod;
        private readonly SaveData _saveData;
        
        private IPeriodicQuestDataProvider _dataProvider;

        public string Key => $"periodic_quest_{_resetPeriod}";
        public string RedDotPath { get; }

        public PeriodicQuest(SaveDataManager saveDataManager, PeriodicResetHandler periodicResetHandler,
            MissionFactory missionFactory, PropertyManager propertyManager, ResetPeriod resetPeriod,
            IPeriodicQuestDataProvider periodicQuestDataProvider)
        {
            _periodicResetHandler = periodicResetHandler;
            _missionFactory = missionFactory;
            _propertyManager = propertyManager;
            _dataProvider = periodicQuestDataProvider;
            _resetPeriod = resetPeriod;
            RedDotPath = CoconutConfig.Get<PeriodicQuestConfig>().GetRedDotPath(resetPeriod);
            
            _saveData = saveDataManager.Get<SaveData>(Key);
            if (_saveData.missionSaves == null) _saveData.missionSaves = new Dictionary<int, Mission.SaveData>();
            if (_saveData.claimedRewards == null) _saveData.claimedRewards = new List<int>();
            
            _rewards = _dataProvider.GetRewards();
            _rewards.Sort((a, b) => a.requiredPoint.CompareTo(b.requiredPoint));
            UpdateRewardStates();
            
            _periodicResetHandler.AddResetCallback(_resetPeriod, Key, ResetQuest);
            
            // 이번 주기에서 한번도 생성된 적이 없으면 AddResetCallback 후 _periodicMissions가 채워진 상태,
            // _periodicMissions가 채워지지 않았다면 이전에 생성되었다는 뜻으로, 로드 필요
            if (_periodicMissions.Count == 0) LoadMissions();
        }

        private void UpdateRewardStates()
        {
            for(var i = 0; i < _rewards.Count; i++)
            {
                if (_rewards[i].requiredPoint > CurrentPoint) _rewards[i].RewardState = PeriodicQuestReward.State.Locked;
                else if (_saveData.claimedRewards.Contains(i)) _rewards[i].RewardState = PeriodicQuestReward.State.Claimed;
                else _rewards[i].RewardState = PeriodicQuestReward.State.Claimable;
            }
        }

        private void ResetQuest()
        {
            _saveData.claimedRewards.Clear();
            _saveData.currentPoint = 0;
            UpdateRewardStates();
            
            _saveData.missionSaves.Clear();
            
            foreach (var periodicMission in _periodicMissions)
            {
                periodicMission.Mission.Dispose();
            }
            _periodicMissions.Clear();

            foreach (var periodicQuestMissionData in _dataProvider.GetMissions())
            {
                var missionSaveData = new Mission.SaveData();
                _saveData.missionSaves[periodicQuestMissionData.id] = missionSaveData;
                var mission = _missionFactory.Create(periodicQuestMissionData.GetMissionData(), missionSaveData);
                var periodicMission = new PeriodicMission(mission, periodicQuestMissionData.point);
                periodicMission.Mission.Start();
                _periodicMissions.Add(periodicMission);
            }

            UpdateRedDots();
        }

        private void UpdateRedDots()
        {
            if (IsQuestOver)
            {
                foreach (var periodicMission in _periodicMissions)
                {
                    RedDot.SetNotified(GetMissionRedDotPath(periodicMission), false);
                }

                for (var i = 0; i < _rewards.Count; i++)
                {
                    RedDot.SetNotified(GetRewardRedDotPath(i), false);
                }

                return;
            }

            foreach (var periodicMission in _periodicMissions)
            {
                RedDot.SetNotified(GetMissionRedDotPath(periodicMission), periodicMission.Mission.IsCompleted && !periodicMission.Mission.IsRewardsClaimed);
            }

            for (var i = 0; i < _rewards.Count; i++)
            {
                RedDot.SetNotified(GetRewardRedDotPath(i), _rewards[i].RewardState == PeriodicQuestReward.State.Claimable);
            }
        }

        public string GetMissionRedDotPath(PeriodicMission mission)
        {
            return $"{RedDotPath}/mission_{mission.Mission.Id}";
        }
        
        public string GetRewardRedDotPath(int index)
        {
            return $"{RedDotPath}/reward_{index}";
        }

        private void LoadMissions()
        {
            foreach (var periodicQuestMissionData in _dataProvider.GetMissions())
            {
                if (!_saveData.missionSaves.ContainsKey(periodicQuestMissionData.id))
                {
                    _saveData.missionSaves[periodicQuestMissionData.id] = new Mission.SaveData();
                }
                
                var missionSaveData = _saveData.missionSaves[periodicQuestMissionData.id];
                var mission = _missionFactory.Create(periodicQuestMissionData.GetMissionData(), missionSaveData);
                var periodicMission = new PeriodicMission(mission, periodicQuestMissionData.point);
                periodicMission.Mission.Start();
                _periodicMissions.Add(periodicMission);
            }
            
            UpdateRedDots();
        }

        public void Complete(PeriodicMission periodicMission)
        {
            periodicMission.Complete(PlayerAction.UNTRACKED);
            _saveData.currentPoint += periodicMission.QuestPoint;
            UpdateRewardStates();
            UpdateRedDots();
        }

        public List<Property> ClaimRewards(PlayerAction playerAction)
        {
            var result = new List<Property>();
            
            for(var i = 0; i < _rewards.Count; i++)
            {
                if(_rewards[i].requiredPoint <= CurrentPoint && !_saveData.claimedRewards.Contains(i))
                {
                    result.Add(_rewards[i].Reward);
                    _propertyManager.Obtain(_rewards[i].Reward, playerAction);
                    _saveData.claimedRewards.Add(i);
                }
            }
            UpdateRewardStates();
            UpdateRedDots();

            return result;
        }

        public void Dispose()
        {
            _periodicResetHandler.RemoveResetCallback(_resetPeriod, Key);
        }

        private class SaveData
        {
            public int currentPoint;
            public List<int> claimedRewards = new();
            public Dictionary<int, Mission.SaveData> missionSaves = new();
        }
    }
}