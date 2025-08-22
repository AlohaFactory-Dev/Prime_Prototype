using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Aloha.Coconut
{
    internal class DailyFreeRewardsManager: IDailyFreeRewardsManager, IDisposable
    {
        private readonly SaveDataManager _saveDataManager;
        private readonly PeriodicResetHandler _periodicResetHandler;
        private readonly LimitedProduct.Factory _limitedProductFactory;

        private readonly SaveData _saveData;
        private List<LimitedProduct> _dailyFreeRewards = new();
        private readonly Property _freeRewards;

        public DailyFreeRewardsManager(SaveDataManager saveDataManager, PeriodicResetHandler periodicResetHandler, 
            LimitedProduct.Factory limitedProductFactory)
        {
            _saveData = saveDataManager.Get<SaveData>("daily_free_rewards");
            _periodicResetHandler = periodicResetHandler;
            _limitedProductFactory = limitedProductFactory;

            _periodicResetHandler.AddResetCallback(ResetPeriod.Daily, "daily_free_rewards", OnDailyReset);
            
            Assert.IsTrue(GameConfig.HaveKey("coconut/daily_free_rewards_type"));
            Assert.IsTrue(GameConfig.HaveKey("coconut/daily_free_rewards_amount"));
            
            _freeRewards = new Property(GameConfig.GetString("coconut/daily_free_rewards_type"), 
                GameConfig.GetInt("coconut/daily_free_rewards_amount", 0));
        }
        
        private void OnDailyReset()
        {
            foreach (var dailyFreeRewards in _dailyFreeRewards)
            {
                dailyFreeRewards.Reset();
            }

            // 세이브데이터는 있지만 생성되지 않은 DailyFreeRewards에 대한 처리
            foreach (var pair in _saveData.dailyFreeRewards)
            {
                pair.Value.count = 0;
            }
        }

        public LimitedProduct GetDailyFreeRewards(string key, string redDotPath)
        {
            if (!_saveData.dailyFreeRewards.TryGetValue(key, out var saveData))
            {
                saveData = new LimitedProduct.SaveData();
                _saveData.dailyFreeRewards[key] = saveData;
            }

            LimitedProduct newRewards = _limitedProductFactory.CreateFreeProduct(_freeRewards, 1, saveData);
            newRewards.LinkRedDot(redDotPath);
            _dailyFreeRewards.Add(newRewards);
            
            return newRewards;
        }

        public void Dispose()
        {
            _periodicResetHandler.RemoveResetCallback(ResetPeriod.Daily, "daily_free_rewards");
        }

        private class SaveData
        {
            public Dictionary<string, LimitedProduct.SaveData> dailyFreeRewards = new();
        }
    }
}
