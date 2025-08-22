using System;
using System.Collections.Generic;
using Aloha.Coconut.IAP;
using UnityEngine;
using Zenject;

namespace Aloha.Coconut
{
    public class PeriodicStoreManager : IDisposable
    {
        public string RedDotPath => _database.GetRedDotPath();
        
        private readonly IIAPManager _iapManager;
        private readonly IPeriodicStoreDatabase _database;
        private Dictionary<ResetPeriod, PeriodicStore> _periodicStores = new Dictionary<ResetPeriod, PeriodicStore>();
        private SaveData _saveData;
    
        public PeriodicStoreManager(SaveDataManager saveDataManager, IIAPManager iapManager, IPeriodicStoreDatabase database)
        {
            _saveData = saveDataManager.Get<SaveData>("periodic_store_manager");
            _iapManager = iapManager;
            _database = database;
        }

        [Inject]
        internal void Inject(PeriodicStore.Factory periodicStoreFactory)
        {
            _iapManager.AddOnInitializedListener(result =>
            {
                if (!result)
                {
                    Debug.Log("PeriodicStoreManager :: IAPManager 초기화 실패");
                    return;
                }
                
                foreach (ResetPeriod period in Enum.GetValues(typeof(ResetPeriod)))
                {
                    if (!_saveData.storeSaveDatas.ContainsKey(period))
                    {
                        _saveData.storeSaveDatas.Add(period, new PeriodicStore.SaveData());
                    }
            
                    _periodicStores[period] = periodicStoreFactory.Create(period, _saveData.storeSaveDatas[period]);
                } 
            });
        }
    
        public PeriodicStore GetStore(ResetPeriod period)
        {
            return _periodicStores[period];
        }

        public void Dispose()
        {
            foreach (var periodicStore in _periodicStores.Values)
            {
                periodicStore.Dispose();
            }
        }

        private class SaveData
        {
            public Dictionary<ResetPeriod, PeriodicStore.SaveData> storeSaveDatas = new Dictionary<ResetPeriod, PeriodicStore.SaveData>();
        }
    }
}