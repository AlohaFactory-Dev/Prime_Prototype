using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UniRx;
using Zenject;

namespace Aloha.Coconut
{
    public class LimitedProduct
    {
        private struct Component
        {
            public Product product;
            public int limit;
            
            public Component(Product product, int limit)
            {
                this.product = product;
                this.limit = limit;
            }
        }
        
        public int Limit { get; }
        public int LimitExceptFree { get; }
        public bool IsSoldOut => Remaining <= 0;
        public int Remaining => Limit - _saveData.count;
        public Product Product
        {
            get
            {
                foreach (var component in _components)
                {
                    if (_saveData.count < component.limit) return component.product;
                }
                
                return _components.Last().product;
            }
        }
        public string RedDotPath { get; private set; }
        
        public IObservable<Unit> OnPurchased => _onPurchased;
        private readonly Subject<Unit> _onPurchased = new Subject<Unit>();
        
        private readonly List<Component> _components;
        private readonly SaveData _saveData;

        private LimitedProduct(List<Component> components, SaveData saveData)
        {
            _components = components;
            _saveData = saveData;
            
            Limit = components.Sum(c => c.limit);
            LimitExceptFree = components.Where(c => c.product.Price is not FreePrice).Sum(c => c.limit);
        }

        public PurchaseResult Purchase(PlayerAction playerAction)
        {
            return Task.Run(async () => await PurchaseAsync(playerAction)).Result;
        }

        public async UniTask<PurchaseResult> PurchaseAsync(PlayerAction playerAction)
        {
            if (Remaining <= 0) return new PurchaseResult {isSuccess = false, errorMessage = "Limit exceeded"};
            PurchaseResult result = await Product.Purchase(playerAction);
            if (result.isSuccess)
            {
                _saveData.count++;
                UpdateRedDot();
            }
            _onPurchased.OnNext(Unit.Default);
            return result;
        }

        public void LinkRedDot(string path)
        {
            RedDotPath = path;
            UpdateRedDot();
        }

        private void UpdateRedDot()
        {
            if(RedDotPath != null) RedDot.SetNotified(RedDotPath, Remaining > 0 && Product.Price is FreePrice);
        }
        
        public void Reset()
        {
            _saveData.count = 0;
            UpdateRedDot();
        }
        
        public class SaveData
        {
            public int count;
        }

        public class Factory
        {
            private readonly PropertyManager _propertyManager;
            private readonly IPackageRewardsManager _packageRewardsManager;
            private readonly RVPrice.Factory _rvPriceFactory;
            private readonly Product.Factory _productFactory;
            private readonly IIAPManager _iapManager;

            // 사용하려는 LimitedProduct 타입에 따라 모두 필요하지 않을 수 있음, 테스트 용이성을 위해 InjectOptional 처리
            public Factory(PropertyManager propertyManager, [InjectOptional] IPackageRewardsManager packageRewardsManager, 
                [InjectOptional] RVPrice.Factory rvPriceFactory, [InjectOptional] Product.Factory productFactory, 
                [InjectOptional] IIAPManager iapManager)
            {
                _propertyManager = propertyManager;
                _packageRewardsManager = packageRewardsManager;
                _rvPriceFactory = rvPriceFactory;
                _productFactory = productFactory;
                _iapManager = iapManager;
            }
            
            public LimitedProduct Create(Product product, int limit, SaveData saveData)
            {
                return new LimitedProduct(new List<Component> {new Component(product, limit)}, saveData);
            }

            public LimitedProduct Create(IPrice price, List<Property> rewards, int limit, SaveData saveData)
            {
                return Create(_productFactory.Create(price, rewards), limit, saveData);
            }
            
            public LimitedProduct CreateIAPProduct(string packageId, int limit, SaveData saveData)
            {
                return Create(_iapManager.GetProduct(packageId), limit, saveData);
            }

            public LimitedProduct CreateFreeProduct(string packageId, int limit, SaveData saveData)
            {
                return CreateFreeProduct(_packageRewardsManager.GetPackageRewards(packageId, false), limit, saveData);
            }

            public LimitedProduct CreateFreeProduct(Property rewards, int limit, SaveData saveData)
            {
                return CreateFreeProduct(new List<Property> {rewards}, limit, saveData);
            }

            public LimitedProduct CreateFreeProduct(List<Property> rewards, int limit, SaveData saveData)
            {
                Product freeProduct = _productFactory.Create(new FreePrice(), rewards);
                return Create(freeProduct, limit, saveData);
            }

            public LimitedProduct CreateRvProduct(string packageId, int freeLimit, int rvLimit, int placementId,
                string placementName, SaveData saveData)
            {
                List<Property> packageRewards = _packageRewardsManager.GetPackageRewards(packageId, false);
                List<Component> components = new List<Component>();

                if (freeLimit > 0)
                {
                    Product freeProduct = _productFactory.Create(new FreePrice(), packageRewards);
                    components.Add(new Component(freeProduct, freeLimit));
                }
                
                Product rvProduct = _productFactory.Create(_rvPriceFactory.Create(placementId, placementName), packageRewards);
                components.Add(new Component(rvProduct, rvLimit));
                
                return new LimitedProduct(components, saveData);
            }
            
            public LimitedProduct CreatePropertyProduct(string packageId, int limit, Property price, SaveData saveData)
            {
                List<Property> packageRewards = _packageRewardsManager.GetPackageRewards(packageId, false);
                List<Component> components = new List<Component>();

                Product propertyProduct = _productFactory.Create(new PropertyPrice(price, _propertyManager), packageRewards);
                components.Add(new Component(propertyProduct, limit));
                
                return new LimitedProduct(components, saveData);
            }
        }

        public void Dispose()
        {
            _onPurchased.Dispose();
        }
    }
}
