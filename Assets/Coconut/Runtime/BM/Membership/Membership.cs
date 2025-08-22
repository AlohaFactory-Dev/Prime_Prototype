using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Aloha.Coconut
{
    public class Membership
    {
        public int Id { get; }
        public bool IsActive => _saveData.isActive;
        public GameDate EndDate => _saveData.endDate;
        public bool IsClaimedToday => _saveData.isClaimedToday;
        public IAPProduct Product { get; }
        public string RedDotPath { get; private set; }

        public string NameKey => Product.NameKey;

        public int Efficiency => Product.Efficiency;
        public List<Property> InstantRewards { get; }
        public List<Property> DailyRewards { get; }
        public List<MembershipPrivilege> Privileges { get; }

        private readonly SaveData _saveData;
        private readonly PropertyManager _propertyManager;

        private Membership(MembershipData data, IAPProduct product, SaveData saveData, PropertyManager propertyManager)
        {
            _saveData = saveData;
            _propertyManager = propertyManager;

            Id = data.id;
            Product = product;
            InstantRewards = Product.Rewards;
            DailyRewards = data.dailyRewards;
            Privileges = data.privileges;
        }

        internal void DailyReset()
        {
            _saveData.isClaimedToday = false;

            if (_saveData.isActive && _saveData.endDate < Clock.GameDateNow)
            {
                _saveData.isActive = false;
            }

            UpdateRedDot();
        }

        internal void AddSpan(int day)
        {
            if (!_saveData.isActive)
            {
                _saveData.isActive = true;
            }

            if (_saveData.endDate < Clock.GameDateNow)
            {
                day -= 1;
                _saveData.endDate = Clock.GameDateNow;
            }

            _saveData.endDate = _saveData.endDate.AddDay(day);
            UpdateRedDot();
        }

        public List<Property> ClaimDailyRewards(PlayerAction action)
        {
            var obtainedRewards = _propertyManager.Obtain(DailyRewards, action);
            _saveData.isClaimedToday = true;

            UpdateRedDot();

            return obtainedRewards;
        }

        public void LinkRedDot(string path)
        {
            RedDotPath = path;
            UpdateRedDot();
        }

        private void UpdateRedDot()
        {
            if (string.IsNullOrEmpty(RedDotPath)) return;
            RedDot.SetNotified(RedDotPath, IsActive && !IsClaimedToday);
        }

        public async UniTask<PurchaseResult> Purchase()
        {
            return await Product.Purchase(PlayerAction.UNTRACKED);
        }

        internal class SaveData
        {
            public bool isActive;
            public GameDate endDate;
            public bool isClaimedToday;
        }

        internal class Factory
        {
            private readonly IIAPManager _iapManager;
            private readonly PropertyManager _propertyManager;

            public Factory(IIAPManager iapManager, PropertyManager propertyManager)
            {
                _iapManager = iapManager;
                _propertyManager = propertyManager;
            }

            public Membership Create(MembershipData data, SaveData saveData)
            {
                return new Membership(data, _iapManager.GetProduct(data.iapId), saveData, _propertyManager);
            }
        }
    }
}