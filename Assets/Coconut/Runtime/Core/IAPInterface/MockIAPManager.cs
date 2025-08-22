using System;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Aloha.Coconut
{
    // 테스트용으로 결과를 고정시킬 수 있는 MockIAPManager
    public class MockIAPManager : IIAPManager, IPropertyManagerRequirer
    {
        public bool IsInitialized => true;
        public string CurrencyCode { get; set; } = "USD";
        public decimal Price { get; set; } = 0.01m;
        
        public IObservable<IAPResult> OnIAPProcessed => _onIAPProcessed;
        private Subject<IAPResult> _onIAPProcessed = new Subject<IAPResult>();

        public PropertyManager PropertyManager
        {
            get => _propertyManager;
            set => _propertyManager = value;
        }
        
        private PropertyManager _propertyManager;
        private readonly IPackageRewardsManager _packageRewardsManager;
        
        public bool IsSuccess { get; set; } = true;
        public IAPFailureReason IAPFailureReason { get; set; }

        public MockIAPManager(IPackageRewardsManager packageRewardsManager)
        {
            _packageRewardsManager = packageRewardsManager;
        }

        public void Initialize() { }

        public void AddOnInitializedListener(Action<bool> listener)
        {
            listener(true);
        }

        async UniTask<IAPResult> IIAPManager.Purchase(string iapId)
        {
            return FakePurchase(iapId);
        }

        public IAPResult FakePurchase(string iapId)
        {
            IAPResult result;
            if (IsSuccess)
            {
                var rewards = _propertyManager.Obtain(_packageRewardsManager.GetPackageRewards(iapId, true), PlayerAction.TEST);
                result = new IAPResult(iapId, CurrencyCode, Price, IsSuccess, rewards);   
            }
            else
            {
                result = new IAPResult(iapId, CurrencyCode, Price, IsSuccess, null, IAPFailureReason);
            }
            
            _onIAPProcessed.OnNext(result);
            return result;
        }

        public IAPProduct GetProduct(string iapId)
        {
            return new IAPProduct(iapId, "test", 100, _packageRewardsManager.GetPackageRewards(iapId, true), this);
        }

        string IIAPManager.GetLocalizedPriceString(string iapId)
        {
            return $"{CurrencyCode}{Price}";
        }
    }
}