using System;
using Aloha.Coconut.IAP;
using Cysharp.Threading.Tasks;

namespace Aloha.Coconut
{
    public sealed class DirectProduct : Product
    {
        public int Id { get; }
        public string PrefabKey { get; }

        private readonly IAPProduct _iapProduct;
        private readonly DirectStore _directStore;
        public DateTime EndTime { get; } 

        internal DirectProduct(SaveData saveData, IAPProduct iapProduct, string prefabKey, DirectStore directStore) : 
            base(iapProduct.Price, iapProduct.NameKey, iapProduct.Rewards, null)
        {
            PrefabKey = prefabKey;
            Id = saveData.id;
            EndTime = saveData.endTime;
            _iapProduct = iapProduct;
            _directStore = directStore;
        }

        public override async UniTask<PurchaseResult> Purchase(PlayerAction playerAction)
        {
            var result = await _iapProduct.Purchase(PlayerAction.UNTRACKED);
            if (result.isSuccess)
            {
                _directStore.RemoveProduct(this);
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