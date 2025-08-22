using System;
using System.Collections.Generic;
using Aloha.Coconut.IAP;
using Zenject;

namespace Aloha.Coconut
{
    public class HardCurrencyProductsManager : IDisposable
    {
        public List<HardCurrencyProductGroup> HardCurrencyProductGroups { get; } = new();

        private readonly SaveData _saveData;
        private readonly PeriodicResetHandler _periodicResetHandler;

        public HardCurrencyProductsManager(SaveDataManager saveDataManager, PeriodicResetHandler periodicResetHandler,
            [InjectOptional] IHardCurrencyProductGroupDatabase productGroupDatabase, IIAPManager iapManager)
        {
            _periodicResetHandler = periodicResetHandler;
            _saveData = saveDataManager.Get<SaveData>("hard_currency_products_manager");

            if (productGroupDatabase == null) productGroupDatabase = new DefaultHardCurrencyProductGroupDatabase();
            
            var productGroupDataList = productGroupDatabase.GetProductGroupDataList();
            foreach (var groupData in productGroupDataList)
            {
                var baseProduct = iapManager.GetProduct(groupData.baseProductId);
                var doubleProduct = iapManager.GetProduct(groupData.doubleProductId);

                var saveData = _saveData.productGroupSaveDatas.TryGetValue(groupData.groupId, out var groupSaveData)
                    ? groupSaveData
                    : new HardCurrencyProductGroup.SaveData();

                _saveData.productGroupSaveDatas.TryAdd(groupData.groupId, saveData);
                HardCurrencyProductGroups.Add(new HardCurrencyProductGroup(baseProduct, doubleProduct, saveData));
            }

            periodicResetHandler.AddResetCallback(ResetPeriod.Weekly, "hard_currency_products_manager", Reset);
        }

        private void Reset()
        {
            foreach (var productGroup in HardCurrencyProductGroups)
            {
                productGroup.Reset();
            }
        }

        private class SaveData
        {
            public Dictionary<int, HardCurrencyProductGroup.SaveData> productGroupSaveDatas = new();
        }

        public void Dispose()
        {
            _periodicResetHandler.RemoveResetCallback(ResetPeriod.Weekly, "hard_currency_products_manager");
        }
    }
}