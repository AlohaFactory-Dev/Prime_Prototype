using System;
using Aloha.Coconut.IAP;
using Cysharp.Threading.Tasks;

namespace Aloha.Coconut.HotDeal
{
    public sealed class HotDealProduct : Product
    {
        public int Id { get; }
        
        private readonly IAPProduct _iapProduct;
        private readonly HotDealStore _hotDealStore;
        public DateTime EndTime { get; } 

        internal HotDealProduct(SaveData saveData, IAPProduct iapProduct, HotDealStore hotDealStore) : 
            base(iapProduct.Price, iapProduct.NameKey, iapProduct.Rewards, null)
        {
            Id = saveData.id;
            EndTime = saveData.endTime;
            _iapProduct = iapProduct;
            _hotDealStore = hotDealStore;
        }

        public override async UniTask<PurchaseResult> Purchase(PlayerAction playerAction)
        {
            var result = await _iapProduct.Purchase(PlayerAction.UNTRACKED);
            if (result.isSuccess)
            {
                _hotDealStore.RemoveProduct(this);
            }

            return result;
        }
        
        public class SaveData
        {
            public int id;
            public DateTime endTime;
        }
    }
}