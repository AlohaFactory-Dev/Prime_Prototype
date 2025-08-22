using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Aloha.Coconut
{
    public sealed class IAPProduct : Product
    {
        public IAPPrice Price => (IAPPrice)base.Price;
        public string IAPId => Price.IAPId;
        public int Efficiency => Price.Efficiency;
        private readonly IIAPManager _iapManager;

        public IAPProduct(string iapId, string nameKey, int efficiency, List<Property> rewards, IIAPManager iapManager) 
            : base(new IAPPrice(iapId, efficiency, iapManager), nameKey, rewards, null)
        {
            _iapManager = iapManager;
        }

        public override async UniTask<PurchaseResult> Purchase(PlayerAction playerAction)
        {
            PurchaseResult result;
            if (Price.IsPayable())
            {
                try
                {
                    var iapResult = await _iapManager.Purchase(((IAPPrice)Price).IAPId);
                    if (iapResult.isSuccess)
                    {
                        result = new PurchaseResult {rewards = iapResult.rewards, isSuccess = true};   
                    }
                    else
                    {
                        result = new PurchaseResult {rewards = null, isSuccess = false, errorMessage = iapResult.failureReason.ToString()};
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Purchase failed: " + e.Message);
                    result = new PurchaseResult {isSuccess = false, errorMessage = e.Message};
                }
            }
            else
            {
                result = new PurchaseResult {isSuccess = false, errorMessage = "IAP is not initialized"};   
            }
    
            return result;
        }
    }
}
