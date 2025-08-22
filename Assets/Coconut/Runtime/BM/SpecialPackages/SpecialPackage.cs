using System;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Aloha.Coconut
{
    public class SpecialPackage
    {
        public int Id { get; }
        public LimitedProduct LimitedProduct { get; }
        public bool IsSoldOut => LimitedProduct.IsSoldOut;
        public IObservable<Unit> OnPurchased => LimitedProduct.OnPurchased;
        
        public SpecialPackage(int id, LimitedProduct limitedProduct)
        {
            Id = id;
            LimitedProduct = limitedProduct;
        }

        public UniTask<PurchaseResult> PurchaseAsync(PlayerAction playerAction)
        {
            return LimitedProduct.PurchaseAsync(playerAction);
        }
    }
}
