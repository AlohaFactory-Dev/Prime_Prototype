using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Aloha.Coconut.IdleRewards
{
    public class IdleRewardGenerator : IDisposable
    {
        public int IdleHoursMax => _idleRewardDataProvider.IdleHoursMax;
        public int QuickEarningHours => _idleRewardDataProvider.QuickEarningHours;
        public int RewardGenerateSeconds => _idleRewardDataProvider.RewardGenerationSeconds;
     
        
        public int QuickEarningPerDay => _idleRewardDataProvider.QuickEarningPerDay;
        public int RvQuickEarningsPerDay => _idleRewardDataProvider.RVQuickEarningsPerDay;
        public Property QuickEarningCost => _idleRewardDataProvider.QuickEarningCost;
        
        public int RewardGenerationMax => IdleHoursMax * 3600 / RewardGenerateSeconds; // 순찰 최대시간일 때 Generation 횟수
        public int QuickEarningGeneration => QuickEarningHours * 3600 / RewardGenerateSeconds; // 빠른 순찰에서 Generation 횟수

        public int QuickEarningLeft => _idleRewardDataProvider.QuickEarningPerDay - _saveData.quickEarningCount;
        public int QuickEarningCount => _saveData.quickEarningCount;
        public int RVQuickEarningLeft => _saveData.rvQuickEarningLeft;
        public bool IsRewardHeapMax => _saveData.generationCount >= RewardGenerationMax;
        public TimeSpan IdleTime { get; private set; }

        private string LastGeneratedTimeKey => $"idle_{Id}_last_gen";
        private string LastClaimedTimeKey => $"idle_{Id}_last_claimed";
        private string ResetCallbackKey => $"idle_{Id}";

        private DateTime LastGeneratedTime
        {
            get => _simpleValues.GetDateTime(LastGeneratedTimeKey, Clock.Now);
            set => _simpleValues.SetDateTime(LastGeneratedTimeKey, value);
        }

        private DateTime LastClaimedTime
        {
            get => _simpleValues.GetDateTime(LastClaimedTimeKey, Clock.Now);
            set => _simpleValues.SetDateTime(LastClaimedTimeKey, value);
        }

        private IDisposable _clockSubscription;

        private string Id => _idleRewardDataProvider.Id;
        public string RedDotPath => _idleRewardDataProvider.RedDotPath;
        
        private SaveData _saveData;
        private readonly PropertyManager _propertyManager;
        private readonly PeriodicResetHandler _periodicResetHandler;
        private readonly SimpleValues _simpleValues;
        private readonly IRVAdapter _rvAdapter;
        private readonly IIdleRewardDataProvider _idleRewardDataProvider;

        private IdleRewardGenerator(PropertyManager propertyManager, PeriodicResetHandler periodicResetHandler, SimpleValues simpleValues,
            IRVAdapter rvAdapter, IIdleRewardDataProvider idleRewardDataProvider, SaveData saveData)
        {
            _propertyManager = propertyManager;
            _periodicResetHandler = periodicResetHandler;
            _simpleValues = simpleValues;
            _rvAdapter = rvAdapter;
            _idleRewardDataProvider = idleRewardDataProvider;

            _saveData = saveData;
        }

        public void Run()
        {
            SetRedDot(_saveData.generationCount > 0);
            
            if (!_simpleValues.HaveDateTime(LastGeneratedTimeKey)) LastGeneratedTime = Clock.Now;
            if (!_simpleValues.HaveDateTime(LastClaimedTimeKey)) LastClaimedTime = Clock.Now;

            OnSecondTick();
            _clockSubscription = Clock.OnSecondTick.Subscribe(_ => OnSecondTick());
            _periodicResetHandler.AddResetCallback(ResetPeriod.Daily, ResetCallbackKey, () =>
            {
                _saveData.quickEarningCount = 0;
                _saveData.rvQuickEarningLeft = RvQuickEarningsPerDay;
            });
        }

        private void SetRedDot(bool isOn)
        {
            RedDot.SetNotified(RedDotPath, isOn);
        }

        private void OnSecondTick()
        {
            if (!IsRewardHeapMax)
            {
                while (Clock.Now > LastGeneratedTime.AddSeconds(RewardGenerateSeconds) && !IsRewardHeapMax)
                {
                    _saveData.generationCount++;
                    AddRewardsTo(_saveData.rewardHeap, _idleRewardDataProvider.GetIdleRewardsPerGeneration());
                    SetRedDot(true);
                    LastGeneratedTime = LastGeneratedTime.AddSeconds(RewardGenerateSeconds);
                }
            }

            IdleTime = Clock.Now - LastClaimedTime;
        }

        private void AddRewardsTo(Dictionary<PropertyTypeGroup, Dictionary<int, float>> rewardHeap, List<IdleReward> rewards)
        {
            foreach (var reward in rewards)
            {
                var rewardType = reward.type;
                
                if (!rewardHeap.ContainsKey(rewardType.group))
                {
                    rewardHeap[rewardType.group] = new Dictionary<int, float>();
                }

                if (!rewardHeap[rewardType.group].ContainsKey(rewardType.id))
                {
                    rewardHeap[rewardType.group][rewardType.id] = 0;
                }
                
                rewardHeap[rewardType.group][rewardType.id] += reward.amount;
            }
        }

        public List<Property> GetCurrentRewards()
        {
            return GetPropertyRewards(_saveData.rewardHeap);
        }

        private List<Property> GetPropertyRewards(Dictionary<PropertyTypeGroup, Dictionary<int, float>> rewardHeap)
        {
            var result = new List<Property>();
            foreach (var (group, subGroup) in rewardHeap)
            {
                foreach (var (propertyId, amount) in subGroup)
                {
                    if (amount < 1) continue;
                    result.Add(new Property(group, propertyId, (int)amount));
                }
            }

            return result;
        }

        public List<Property> ClaimRewards(PlayerAction playerAction)
        {
            SetRedDot(false);
            var rewards = GetCurrentRewards();
            foreach (var reward in rewards)
            {
                _saveData.rewardHeap[reward.type.group][reward.type.id] -= (int)reward.amount;
            }

            if (_saveData.generationCount == RewardGenerationMax)
            {
                LastGeneratedTime = Clock.Now;
            }

            LastClaimedTime = LastGeneratedTime;
            _saveData.generationCount = 0;
            IdleTime = Clock.Now - LastClaimedTime;
            
            return _propertyManager.Obtain(rewards, playerAction);
        }

        public List<Property> GetQuickEarningRewards()
        {
            var quickEarningsRewards = new Dictionary<PropertyTypeGroup, Dictionary<int, float>>();
            for (var i = 0; i < QuickEarningGeneration; i++)
            {
                AddRewardsTo(quickEarningsRewards, _idleRewardDataProvider.GetIdleRewardsPerGeneration());
            }

            return GetPropertyRewards(quickEarningsRewards);
        }

        public List<Property> ClaimQuickEarningRewards(PlayerAction playerAction)
        {
            if (QuickEarningLeft <= 0) return new List<Property>();

            _saveData.quickEarningCount++;
            return ClaimQuickEarningRewardsInternal(playerAction);
        }

        private List<Property> ClaimQuickEarningRewardsInternal(PlayerAction playerAction)
        {
            return _propertyManager.Obtain(GetQuickEarningRewards(), playerAction);
        }

        public async UniTask<List<Property>> ClaimQuickEarningRewardsByRV(PlayerAction playerAction)
        {
            if (RVQuickEarningLeft <= 0) return new List<Property>();
            var result = await _rvAdapter.ShowRewardedAdAsync(_idleRewardDataProvider.QuickEarningRVPlacementId, _idleRewardDataProvider.QuickEarningRVPlacementName);
            if (result)
            {
                _saveData.rvQuickEarningLeft--;
                return ClaimQuickEarningRewardsInternal(playerAction);
            }
            else
            {
                return new List<Property>();
            }
        }

        public void Dispose()
        {
            _clockSubscription?.Dispose();
            _periodicResetHandler.RemoveResetCallback(ResetPeriod.Daily, ResetCallbackKey);
        }

        public class SaveData
        {
            public int generationCount;
            public int quickEarningCount;
            public int rvQuickEarningLeft;
            public Dictionary<PropertyTypeGroup, Dictionary<int, float>> rewardHeap = new();
        }

        public class Factory
        {
            private readonly PropertyManager _propertyManager;
            private readonly PeriodicResetHandler _periodicResetHandler;
            private readonly SimpleValues _simpleValues;
            private readonly IRVAdapter _rvAdapter;
            
            public Factory(PropertyManager propertyManager, PeriodicResetHandler periodicResetHandler, SimpleValues simpleValues, IRVAdapter rvAdapter)
            {
                _propertyManager = propertyManager;
                _periodicResetHandler = periodicResetHandler;
                _simpleValues = simpleValues;
                _rvAdapter = rvAdapter;
            }

            public IdleRewardGenerator Create(IIdleRewardDataProvider idleRewardDataProvider, SaveData saveData)
            {
                return new IdleRewardGenerator(_propertyManager, _periodicResetHandler, _simpleValues, _rvAdapter, idleRewardDataProvider, saveData);
            }
        }
    }
}
