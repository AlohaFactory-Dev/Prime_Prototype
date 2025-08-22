using System.Collections.Generic;

namespace Aloha.Coconut
{
    public struct PurchaseRewardsData
    {
        public decimal objective;
        public List<Property> rewards;
    }
    
    public class PurchaseRewards
    {
        public decimal Objective { get; }
        public decimal Progress { get; internal set; }
        public List<Property> Rewards { get; }
        public bool IsClaimable => !IsClaimed && Progress >= Objective;
        public string RedDotPath { get; private set; }

        public bool IsClaimed
        {
            get => _saveData.isClaimed;
            internal set => _saveData.isClaimed = value;
        }

        private readonly SaveData _saveData;
        private readonly PropertyManager _propertyManager;

        private PurchaseRewards(PurchaseRewardsData data, SaveData saveData, PropertyManager propertyManager)
        {
            Objective = data.objective;
            Rewards = data.rewards;
            _saveData = saveData;
            _propertyManager = propertyManager;
        }

        public List<Property> Claim(PlayerAction playerAction)
        {
            var rewards = _propertyManager.Obtain(Rewards, playerAction);
            IsClaimed = true;
            UpdateRedDot();
            return rewards;
        }

        public void LinkRedDot(string path)
        {
            RedDotPath = path;
            UpdateRedDot();
        }

        internal void UpdateRedDot()
        {
            if (string.IsNullOrEmpty(RedDotPath)) return;
            RedDot.SetNotified(RedDotPath, IsClaimable && !IsClaimed);
        }

        internal class SaveData
        {
            public bool isClaimed;
        }

        internal class Factory
        {
            private readonly PropertyManager _propertyManager;

            public Factory(PropertyManager propertyManager)
            {
                _propertyManager = propertyManager;
            }
            
            public PurchaseRewards Create(PurchaseRewardsData data, SaveData saveData)
            {
                return new PurchaseRewards(data, saveData, _propertyManager);
            }
        }
    }
}
