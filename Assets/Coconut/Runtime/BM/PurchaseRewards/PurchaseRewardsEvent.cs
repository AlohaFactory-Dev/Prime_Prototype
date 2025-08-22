using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace Aloha.Coconut
{
    public struct PurchaseRewardsEventData
    {
        public PurchaseRewardsType type;
        public List<PurchaseRewardsData> purchaseRewardsList;

        public PurchaseRewardsEventData(PurchaseRewardsType type)
        {
            this.type = type;
            purchaseRewardsList = new List<PurchaseRewardsData>();
        }
    }
    
    public class PurchaseRewardsEvent : IDisposable
    {
        public int EventId { get; }
        public GameDate StartDate { get; }
        public GameDate EndDate { get; }
        public PurchaseRewardsType Type { get; }
        public string CurrencyCode => _iapManager.CurrencyCode;
        public string RedDotPath { get; }

        private readonly IIAPManager _iapManager;
        public List<PurchaseRewards> PurchaseRewardsList { get; }

        private readonly IPurchaseRewardsDatabase _database;
        private readonly IDisposable _iapSubscription;
        private readonly SaveData _saveData;

        private PurchaseRewardsEvent(EventSchedule eventSchedule, PurchaseRewardsEventData data, SaveData saveData, 
            PurchaseRewards.Factory purchaseRewardsFactory, IIAPManager iapManager, string redDotPath)
        {
            Assert.IsTrue(iapManager.IsInitialized, "IAPManager is not initialized");
            
            EventId = eventSchedule.Id;
            StartDate = eventSchedule.From;
            EndDate = eventSchedule.To;
            
            Type = data.type;

            _saveData = saveData;
            _iapManager = iapManager;
            RedDotPath = redDotPath;
            if (_saveData.currencyCode.ToUpper() != iapManager.CurrencyCode.ToUpper())
            {
                _saveData.currencyCode = iapManager.CurrencyCode;
                _saveData.progress = 0;
            }
            
            PurchaseRewardsList = new List<PurchaseRewards>();
            for (int i = 0; i < data.purchaseRewardsList.Count; i++)
            {
                PurchaseRewardsData prData = data.purchaseRewardsList[i];
                PurchaseRewards.SaveData rewardsSaveData = null;
                if (saveData.rewardsSaveDatas.Count > i)
                {
                    rewardsSaveData = saveData.rewardsSaveDatas[i];
                }
                else
                {
                    rewardsSaveData = new PurchaseRewards.SaveData();
                    saveData.rewardsSaveDatas.Add(rewardsSaveData);
                }

                PurchaseRewards pr = purchaseRewardsFactory.Create(prData, rewardsSaveData);
                pr.LinkRedDot($"{RedDotPath}/{i}");
                PurchaseRewardsList.Add(pr);
            }
            
            SetProgress(saveData.progress);

            _iapSubscription = _iapManager.OnIAPProcessed
                .Where(iapResult => iapResult.isSuccess)
                .Subscribe(iapResult =>
                {
                    if (Type == PurchaseRewardsType.Amount)
                    {
                        if (_saveData.currencyCode.ToUpper() != iapResult.currencyCode.ToUpper()) // 어떤 이유에서든 화폐 단위가 바뀌었으면, 진행도 초기화
                        {
                            _saveData.currencyCode = iapResult.currencyCode;
                            SetProgress(iapResult.price);
                        }
                        else
                        {
                            SetProgress(_saveData.progress + iapResult.price);
                        }
                    }
                    else if (Type == PurchaseRewardsType.Day)
                    {
                        if (_saveData.lastPurchaseDate < Clock.GameDateNow)
                        {
                            _saveData.lastPurchaseDate = Clock.GameDateNow;
                            SetProgress(_saveData.progress + 1);
                        }
                    }
                });
        }

        private void SetProgress(decimal progress)
        {
            _saveData.progress = progress;
            foreach (PurchaseRewards pr in PurchaseRewardsList)
            {
                pr.Progress = progress;
                pr.UpdateRedDot();
            }
        }

        public void Dispose()
        {
            _iapSubscription?.Dispose();
        }

        internal class SaveData
        {
            public string currencyCode = ""; // amount 전용
            public GameDate lastPurchaseDate; // day 전용
            public decimal progress;
            public List<PurchaseRewards.SaveData> rewardsSaveDatas = new List<PurchaseRewards.SaveData>();
        }

        internal class Factory
        {
            private readonly IIAPManager _iapManager;
            private readonly PurchaseRewards.Factory _purchaseRewardsFactory;
            private readonly IPurchaseRewardsDatabase _purchaseRewardsDatabase;

            public Factory(IIAPManager iapManager, PurchaseRewards.Factory purchaseRewardsFactory, 
                [InjectOptional] IPurchaseRewardsDatabase purchaseRewardsDatabase)
            {
                _iapManager = iapManager;
                _purchaseRewardsFactory = purchaseRewardsFactory;
                _purchaseRewardsDatabase = purchaseRewardsDatabase;
                if (_purchaseRewardsDatabase == null) _purchaseRewardsDatabase = new DefaultPurchaseRewardsDatabase();
            }

            public PurchaseRewardsEvent Create(EventSchedule eventSchedule, SaveData saveData)
            {
                string redDotPath = $"{_purchaseRewardsDatabase.GetRedDotPath()}/{eventSchedule.Id}";
                PurchaseRewardsType eventType = _purchaseRewardsDatabase.GetPurchaseRewardsType(eventSchedule.Var);
                if (eventType == PurchaseRewardsType.Amount)
                {
                    if (!_purchaseRewardsDatabase.IsAmountEventAvailable(eventSchedule.Var, _iapManager.CurrencyCode))
                    {
                        Debug.LogWarning($"Amount event is not available for currency code {_iapManager.CurrencyCode}");
                        return null;   
                    }
                    
                    return new PurchaseRewardsEvent(eventSchedule,
                        _purchaseRewardsDatabase.GetAmountEventData(eventSchedule.Var, _iapManager.CurrencyCode), 
                        saveData, _purchaseRewardsFactory, _iapManager, redDotPath);
                }

                return new PurchaseRewardsEvent(eventSchedule,
                    _purchaseRewardsDatabase.GetDayEventData(eventSchedule.Var), 
                    saveData, _purchaseRewardsFactory, _iapManager, redDotPath);
            }
        }
    }
}
