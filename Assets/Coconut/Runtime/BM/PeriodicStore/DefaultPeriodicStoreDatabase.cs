using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace Aloha.Coconut
{
    internal class DefaultPeriodicStoreDatabase : IPeriodicStoreDatabase
    {
        public DefaultPeriodicStoreDatabase()
        {
            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/prdc_store_daily_rv_id"));
            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/prdc_store_daily_rv_name"));
            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/prdc_store_weekly_rv_id"));
            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/prdc_store_weekly_rv_name"));
            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/prdc_store_monthly_rv_id"));
            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/prdc_store_monthly_rv_name"));
            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/prdc_store_reddot_path"));
        }
        
        public List<PeriodicStoreProductData> GetProductDatas(ResetPeriod resetPeriod)
        {
            return TableManager.Get<PeriodicStoreProductData>("Tables/bm_prdc_store")
                .Where(p => p.resetPeriod == resetPeriod).ToList();
        }

        public (int, string) GetRVPlacement(ResetPeriod resetPeriod)
        {
            switch (resetPeriod)
            {
                case ResetPeriod.Daily:
                    return (GameConfig.GetInt("coconut_bm/prdc_store_daily_rv_id", 0), GameConfig.GetString("coconut_bm/prdc_store_daily_rv_name"));
                case ResetPeriod.Weekly:
                    return (GameConfig.GetInt("coconut_bm/prdc_store_weekly_rv_id", 0), GameConfig.GetString("coconut_bm/prdc_store_weekly_rv_name"));
                case ResetPeriod.Monthly:
                    return (GameConfig.GetInt("coconut_bm/prdc_store_monthly_rv_id", 0), GameConfig.GetString("coconut_bm/prdc_store_monthly_rv_name"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(resetPeriod), resetPeriod, null);
            }
        }

        public string GetRedDotPath()
        {
            return GameConfig.GetString("coconut_bm/prdc_store_reddot_path");
        }

        public string GetRedDotPath(ResetPeriod resetPeriod)
        {
            return $"{GetRedDotPath()}/{resetPeriod}";
        }
    }
}
