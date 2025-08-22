using Cysharp.Threading.Tasks;

namespace Aloha.Coconut
{
    public class IAPPrice : IPrice
    {
        public string IAPId { get; }
        public int Efficiency { get; }
        private readonly IIAPManager _iapManager;

        internal IAPPrice(string iapId, int efficiency, IIAPManager iapManager)
        {
            IAPId = iapId;
            Efficiency = efficiency;
            _iapManager = iapManager;
        }
        
        public async UniTask<bool> Pay(PlayerAction playerAction)
        {
            // IAP의 경우 Purchase 메서드 하나에서 결제와 획득까지 전부 이뤄짐
            // IAPPrice의 Pay는 가짜로 남겨두고, 실제 결제와 획득은 전부 IAPProduct에서 처리
            return true;
        }

        public bool IsPayable()
        {
            return _iapManager.IsInitialized;
        }

        public string GetPriceString()
        {
            return _iapManager.GetLocalizedPriceString(IAPId);
        }
    }
}