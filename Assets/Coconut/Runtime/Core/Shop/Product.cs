using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Aloha.Coconut
{
    public class Product
    {
        public IPrice Price { get; }
        public string NameKey { get; protected set; }
        public List<Property> Rewards => _rewards;
        
        private readonly List<Property> _rewards;
        private readonly PropertyManager _propertyManager;

        protected Product(IPrice price, string nameKey, List<Property> rewards, PropertyManager propertyManager)
        {
            Price = price;
            NameKey = nameKey;
            
            _propertyManager = propertyManager;
            _rewards = rewards;
        }

        public virtual async UniTask<PurchaseResult> Purchase(PlayerAction playerAction)
        {
            PurchaseResult result;
            if (Price.IsPayable() && await Price.Pay(playerAction))
            {
                try
                {
                    var rewards = _propertyManager.Obtain(_rewards, playerAction);
                    result = new PurchaseResult
                    {
                        isSuccess = true,
                        rewards = rewards
                    };
                }
                catch (Exception e)
                {
                    Debug.LogError("Purchase failed: " + e.Message);
                    result = new PurchaseResult {isSuccess = false, errorMessage = e.Message};
                }       
            }
            else
            {
                result = new PurchaseResult {isSuccess = false, errorMessage = "Payment failed"};
            }
            
            OnPurchaseResult(result);
            return result;
        }
        
        protected virtual void OnPurchaseResult(PurchaseResult result) { }
        
        public class Factory
        {
            private readonly PropertyManager _propertyManager;

            public Factory(PropertyManager propertyManager)
            {
                _propertyManager = propertyManager;
            }

            public Product Create(IPrice price, Property property, string nameKey = null)
            {
                return new Product(price, nameKey, new List<Property> {property}, _propertyManager);
            }
            
            public Product Create(IPrice price, List<Property> properties, string nameKey = null)
            {
                return new Product(price, nameKey, properties, _propertyManager);
            }
        }
    }
}
