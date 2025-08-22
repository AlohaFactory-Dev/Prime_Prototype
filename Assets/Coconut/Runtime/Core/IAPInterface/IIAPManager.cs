using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Aloha.Coconut
{
    public interface IIAPManager
    {
        bool IsInitialized { get; }
        void Initialize();
        void AddOnInitializedListener(Action<bool> listener);
        string CurrencyCode { get; }
        IObservable<IAPResult> OnIAPProcessed { get; }
        protected internal UniTask<IAPResult> Purchase(string iapId);
        protected internal string GetLocalizedPriceString(string iapId);
        IAPProduct GetProduct(string iapId);
    }
    
    public struct IAPResult
    {
        public string iapId;
        public string currencyCode;
        public decimal price;
        public bool isSuccess;
        public List<Property> rewards;
        public IAPFailureReason failureReason;

        public IAPResult(string iapId, string currencyCode, decimal price, 
            bool isSuccess, List<Property> rewards, IAPFailureReason failureReason = IAPFailureReason.None)
        {
            this.iapId = iapId;
            this.currencyCode = currencyCode;
            this.price = price;
            this.isSuccess = isSuccess;
            this.rewards = rewards;
            this.failureReason = failureReason;
        }
    }

    public enum IAPFailureReason
    {
        None,
        InvalidReceipt,
        PaymentFailed,
        NetworkError,
        Unknown
    }
}
