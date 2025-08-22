using System;
using System.Collections.Generic;
using Aloha.Coconut.IAP;
using UniRx;

namespace Aloha.Coconut.HotDeal
{
    public class HotDealStore : IDisposable
    {
        public IReadOnlyList<HotDealProduct> ActiveProducts => _activeProducts;
        private List<HotDealProduct> _activeProducts = new();
        
        public IObservable<HotDealProduct> OnProductActivated => _onProductActivated;
        private Subject<HotDealProduct> _onProductActivated = new();
        
        public IObservable<HotDealProduct> OnProductRemoved => _onProductRemoved;
        private Subject<HotDealProduct> _onProductRemoved = new();
        
        private readonly IIAPManager _iapManager;
        private readonly SaveData _saveData;
        private readonly List<HotDealProductData> _productDatas;
        private readonly IDisposable _clockSubscription;

        public HotDealStore(IIAPManager iapManager, SaveDataManager saveDataManager)
        {
            _iapManager = iapManager;
            _saveData = saveDataManager.Get<SaveData>("hotDealStore");

            _productDatas = TableManager.Get<HotDealProductData>("hotdeal_products"); 

            foreach (var productSaveData in _saveData.activeProductDatas)
            {
                var productData = _productDatas.Find(data => data.id == productSaveData.id);
                _activeProducts.Add(new HotDealProduct(productSaveData, _iapManager.GetProduct(productData.iapId), this));
            }

            CheckDeactivation();
            _clockSubscription = Clock.OnSecondTick.Subscribe(_ => CheckDeactivation());
        }

        public void Activate(int hotDealProductId)
        {
            var productData = _productDatas.Find(data => data.id == hotDealProductId);
            var newSaveData = new HotDealProduct.SaveData {id = productData.id, endTime = Clock.Now + TimeSpan.FromHours(productData.durationHours)};
            var newProduct = new HotDealProduct(newSaveData, _iapManager.GetProduct(productData.iapId), this);
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

        public void RemoveProduct(HotDealProduct product)
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
            public List<HotDealProduct.SaveData> activeProductDatas = new();
        }
    }
}