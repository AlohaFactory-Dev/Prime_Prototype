using System;
using System.Collections.Generic;
using Aloha.Coconut.IAP;
using UniRx;

namespace Aloha.Coconut
{
    public class DirectStore : IDisposable
    {
        public IReadOnlyList<DirectProduct> ActiveProducts => _activeProducts;
        private List<DirectProduct> _activeProducts = new();
        
        public IObservable<DirectProduct> OnProductActivated => _onProductActivated;
        private Subject<DirectProduct> _onProductActivated = new();
        
        public IObservable<DirectProduct> OnProductRemoved => _onProductRemoved;
        private Subject<DirectProduct> _onProductRemoved = new();
        
        private readonly IAPManager _iapManager;
        private readonly SaveData _saveData;
        private readonly List<DirectProductData> _productDatas;
        private readonly IDisposable _clockSubscription;

        public DirectStore(IDirectStoreDatabase database, IAPManager iapManager, SaveDataManager saveDataManager)
        {
            _iapManager = iapManager;
            _saveData = saveDataManager.Get<SaveData>("direct_store");

            _productDatas = database.GetProductDatas(); 

            foreach (var productSaveData in _saveData.activeProductDatas)
            {
                var productData = _productDatas.Find(data => data.id == productSaveData.id);
                _activeProducts.Add(new DirectProduct(productSaveData, _iapManager.GetProduct(productData.iapId), productData.prefabKey, this));
            }

            CheckDeactivation();
            _clockSubscription = Clock.OnSecondTick.Subscribe(_ => CheckDeactivation());
        }

        public void Activate(int directProductId)
        {
            var productData = _productDatas.Find(data => data.id == directProductId);
            var newSaveData = new DirectProduct.SaveData
            {
                id = productData.id, 
                endTime = Clock.Now + TimeSpan.FromHours(productData.durationHours)
            };
            var newProduct = new DirectProduct(newSaveData, _iapManager.GetProduct(productData.iapId), productData.prefabKey, this);
            _saveData.activeProductDatas.Add(newSaveData);
            _activeProducts.Add(newProduct);
            _onProductActivated.OnNext(newProduct);
        }

        private void CheckDeactivation()
        {
            int i = 0;
            while (i < _activeProducts.Count)
            {
                if (Clock.Now > _activeProducts[i].EndTime)
                {
                    RemoveProduct(_activeProducts[i]);
                }
                else
                {
                    i++;
                }
            }
        }

        public void RemoveProduct(DirectProduct product)
        {
            _activeProducts.Remove(product);
            _saveData.activeProductDatas.RemoveAll(data => data.id == product.Id);
            _onProductRemoved.OnNext(product);
        }

        public void Dispose()
        {
            _clockSubscription.Dispose();
        }

        private class SaveData
        {
            public List<DirectProduct.SaveData> activeProductDatas = new();
        }
    }
}