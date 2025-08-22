using System.Collections.Generic;

namespace Aloha.Coconut
{
    // 테스트 코드용 DailyFreeRewardsManager 
    public class MockDailyFreeRewardsManager : IDailyFreeRewardsManager
    {
        private readonly PropertyManager _propertyManager;
        private readonly LimitedProduct.Factory _limitedProductFactory;

        public List<Property> Rewards { get; set; } = new List<Property>();

        public MockDailyFreeRewardsManager(PropertyManager propertyManager, LimitedProduct.Factory limitedProductFactory)
        {
            _propertyManager = propertyManager;
            _limitedProductFactory = limitedProductFactory;
        }
        
        public LimitedProduct GetDailyFreeRewards(string key, string redDotPath)
        {
            return _limitedProductFactory.CreateFreeProduct(Rewards, 1, new LimitedProduct.SaveData());
        }
    }
}
