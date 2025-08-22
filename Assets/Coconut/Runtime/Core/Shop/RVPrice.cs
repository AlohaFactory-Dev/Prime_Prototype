using Cysharp.Threading.Tasks;

namespace Aloha.Coconut
{
    public class RVPrice : IPrice
    {
        private int _placementId;
        private string _placementName;
        private readonly IRVAdapter _rvAdapter;

        private RVPrice(int placementId, string placementName, IRVAdapter rvAdapter)
        {
            _placementId = placementId;
            _placementName = placementName;
            _rvAdapter = rvAdapter;
        }

        public virtual async UniTask<bool> Pay(PlayerAction playerAction)
        {
            return await _rvAdapter.ShowRewardedAdAsync(_placementId, _placementName);
        }

        public bool IsPayable()
        {
            return true;
        }

        public virtual string GetPriceString()
        {
            return "<sprite name=\"Ads\">";
        }

        public class Factory
        {
            private readonly IRVAdapter _rvAdapter;

            public Factory(IRVAdapter rvAdapter)
            {
                _rvAdapter = rvAdapter;
            }
            
            public RVPrice Create(int placementId, string placementName)
            {
                return new RVPrice(placementId, placementName, _rvAdapter);
            }
        }
    }
}