using System.Collections.Generic;
using System.Linq;

namespace Aloha.Coconut
{
    public interface IPackageRewardsManager
    {
        List<Property> GetPackageRewards(string packageId, bool isPaid);
    }
    
    public class PackageRewardsManager : IPackageRewardsManager
    {
        private struct PackageReward
        {
            [CSVColumn] public string packageId;
            [CSVColumn] public string rewardTypeAlias;
            [CSVColumn] public int rewardAmount;
        }
        
        private readonly List<PackageReward> _productRewards;
        
        public PackageRewardsManager()
        {
            _productRewards = TableManager.Get<PackageReward>("iap_rewards");
        }
        
        public List<Property> GetPackageRewards(string packageId, bool isPaid)
        {
            return _productRewards.FindAll(reward => reward.packageId.Equals(packageId))
                .Select(r => new Property(r.rewardTypeAlias, r.rewardAmount, isPaid))
                .ToList();
        }
    }

    public class MockPackageRewardsManager : IPackageRewardsManager
    {
        private Dictionary<string, List<Property>> _packageRewards = new Dictionary<string, List<Property>>();
        
        public void AddPackageRewards(string packageId, List<Property> rewards)
        {
            _packageRewards.Add(packageId, rewards);
        }
        
        public List<Property> GetPackageRewards(string packageId, bool isPaid)
        {
            return _packageRewards.ContainsKey(packageId) ? _packageRewards[packageId] : new List<Property>();
        }
    }
}
