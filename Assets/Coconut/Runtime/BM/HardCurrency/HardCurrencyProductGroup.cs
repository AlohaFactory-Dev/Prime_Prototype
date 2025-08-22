using Aloha.Coconut.IAP;
using Cysharp.Threading.Tasks;

namespace Aloha.Coconut
{
    public class HardCurrencyProductGroup
    {
        public IAPProduct Product { get; private set; }
        public bool IsDoublePurchased => _saveData.isDoublePurchased;

        private readonly IAPProduct _baseProduct;
        private readonly IAPProduct _doubleProduct;

        private readonly SaveData _saveData;

        public HardCurrencyProductGroup(IAPProduct baseProduct, IAPProduct doubleProduct, SaveData saveData)
        {
            _baseProduct = baseProduct;
            _doubleProduct = doubleProduct;
            _saveData = saveData;

            Product = _saveData.isDoublePurchased ? _baseProduct : _doubleProduct;
        }

        public async UniTask<PurchaseResult> Purchase()
        {
            var result = await Product.Purchase(PlayerAction.UNTRACKED);
            if (result.isSuccess && Product.IAPId == _doubleProduct.IAPId)
            {
                _saveData.isDoublePurchased = true;
                Product = _baseProduct;
            }

            return result;
        }

        internal void Reset()
        {
            _saveData.isDoublePurchased = false;
            Product = _doubleProduct;
        }

        public class SaveData
        {
            public bool isDoublePurchased;
        }
    }
}