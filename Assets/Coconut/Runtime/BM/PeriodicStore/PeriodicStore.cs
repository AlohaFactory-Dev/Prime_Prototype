using System;
using System.Collections.Generic;

namespace Aloha.Coconut
{
    public class PeriodicStore : IDisposable
    {
        public List<LimitedProduct> Products { get; }
        public TimeSpan TimeLeftUntilReset => _periodicResetHandler.GetRemainingTime(_resetPeriod);
        public string RedDotPath { get; }
    
        private readonly SaveData _saveData;
        private readonly ResetPeriod _resetPeriod;
        private readonly PeriodicResetHandler _periodicResetHandler;

        private PeriodicStore(ResetPeriod resetPeriod, SaveData saveData, List<LimitedProduct> products, PeriodicResetHandler periodicResetHandler,
            string redDotPath)
        {
            _saveData = saveData;
            _resetPeriod = resetPeriod;
            _periodicResetHandler = periodicResetHandler;
            RedDotPath = redDotPath;
            Products = products;
            periodicResetHandler.AddResetCallback(resetPeriod, $"prdc_store_{resetPeriod}", OnReset);
        }

        private void OnReset()
        {
            foreach (var periodicStoreProduct in Products)
            {
                periodicStoreProduct.Reset();
            }
        }

        public void Dispose()
        {
            _periodicResetHandler.RemoveResetCallback(_resetPeriod, $"prdc_store_{_resetPeriod}");
        }

        public class SaveData
        {
            public Dictionary<int, LimitedProduct.SaveData> productSaveDatas = new Dictionary<int, LimitedProduct.SaveData>();
        }

        internal class Factory
        {
            private readonly PeriodicResetHandler _periodicResetHandler;
            private readonly LimitedProduct.Factory _limitedProductFactory;
            private readonly IPeriodicStoreDatabase _periodicStoreDatabase;

            public Factory(PeriodicResetHandler periodicResetHandler, IPeriodicStoreDatabase periodicStoreDatabase,
                LimitedProduct.Factory limitedProductFactory)
            {
                _periodicResetHandler = periodicResetHandler;
                _limitedProductFactory = limitedProductFactory;
                _periodicStoreDatabase = periodicStoreDatabase;
            }

            public PeriodicStore Create(ResetPeriod resetPeriod, SaveData saveData)
            {
                List<PeriodicStoreProductData> productDatas = _periodicStoreDatabase.GetProductDatas(resetPeriod);
                List<LimitedProduct> products = new List<LimitedProduct>();
                
                foreach (var productData in productDatas)
                {
                    if (!saveData.productSaveDatas.ContainsKey(productData.productId))
                    {
                        saveData.productSaveDatas.Add(productData.productId, new LimitedProduct.SaveData());
                    }

                    LimitedProduct.SaveData productSaveData = saveData.productSaveDatas[productData.productId];
                    LimitedProduct limitedProduct;
                    if (productData.isFree == 1)
                    {
                        var (rvId, rvName) = _periodicStoreDatabase.GetRVPlacement(productData.resetPeriod);
                        limitedProduct = _limitedProductFactory.CreateRvProduct(productData.iapId, 1, 
                            productData.limit, rvId, rvName, productSaveData);
                        limitedProduct.LinkRedDot($"{_periodicStoreDatabase.GetRedDotPath(productData.resetPeriod)}/{productData.productId}");
                    }
                    else
                    {
                        limitedProduct = _limitedProductFactory.CreateIAPProduct(productData.iapId, productData.limit, productSaveData);                        
                    }

                    products.Add(limitedProduct);
                }
                
                return new PeriodicStore(resetPeriod, saveData, products, _periodicResetHandler, _periodicStoreDatabase.GetRedDotPath(resetPeriod));
            }
        }
    }
}