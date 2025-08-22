using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Aloha.Coconut
{
    internal class DefaultPurchaseRewardsDatabase : IPurchaseRewardsDatabase
    {
        private struct PurchaseRewardsTableData
        {
            [CSVColumn] public int groupId;
            [CSVColumn] public PurchaseRewardsType type;
            [CSVColumn] public string currencyCode;
            [CSVColumn] public decimal objective;
            [CSVColumn] public string pAlias0;
            [CSVColumn] public int pAmount0;
            [CSVColumn] public string pAlias1;
            [CSVColumn] public int pAmount1;
            [CSVColumn] public string pAlias2;
            [CSVColumn] public int pAmount2;
            [CSVColumn] public string pAlias3;
            [CSVColumn] public int pAmount3;

            public PurchaseRewardsData ToRewardsData()
            {
                List<Property> rewards = new List<Property>();
                if (!string.IsNullOrEmpty(pAlias0)) rewards.Add(new Property(pAlias0, pAmount0));
                if (!string.IsNullOrEmpty(pAlias1)) rewards.Add(new Property(pAlias1, pAmount1));
                if (!string.IsNullOrEmpty(pAlias2)) rewards.Add(new Property(pAlias2, pAmount2));
                if (!string.IsNullOrEmpty(pAlias3)) rewards.Add(new Property(pAlias3, pAmount3));
                
                return new PurchaseRewardsData
                {
                    objective = objective,
                    rewards = rewards
                };
            }
        }

        private readonly Dictionary<int, PurchaseRewardsEventData> _dayEventDatas = new ();

        private readonly Dictionary<int, Dictionary<string, PurchaseRewardsEventData>> _amountEventDatas = new ();

        private readonly string _redDotPath;

        public DefaultPurchaseRewardsDatabase()
        {
            List<PurchaseRewardsTableData> tableDatas = TableManager.Get<PurchaseRewardsTableData>("purchase_rewards");
            foreach (var tableData in tableDatas)
            {
                if (tableData.type == PurchaseRewardsType.Amount)
                {
                    if (!_amountEventDatas.ContainsKey(tableData.groupId))
                    {
                        _amountEventDatas.Add(tableData.groupId, new Dictionary<string, PurchaseRewardsEventData>());
                    }
                    
                    if (!_amountEventDatas[tableData.groupId].ContainsKey(tableData.currencyCode))
                    {
                        _amountEventDatas[tableData.groupId].Add(tableData.currencyCode, new PurchaseRewardsEventData(PurchaseRewardsType.Amount));
                    }
                    
                    _amountEventDatas[tableData.groupId][tableData.currencyCode].purchaseRewardsList.Add(tableData.ToRewardsData());
                }
                else if (tableData.type == PurchaseRewardsType.Day)
                {
                    if (!_dayEventDatas.ContainsKey(tableData.groupId))
                    {
                        _dayEventDatas.Add(tableData.groupId, new PurchaseRewardsEventData(PurchaseRewardsType.Day));
                    }
                    
                    _dayEventDatas[tableData.groupId].purchaseRewardsList.Add(tableData.ToRewardsData());
                }
            }

            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/purchase_rewards_red_dot_path"));
            _redDotPath = GameConfig.GetString("coconut_bm/purchase_rewards_red_dot_path");
        }

        public PurchaseRewardsType GetPurchaseRewardsType(int groupId)
        {
            if (_amountEventDatas.ContainsKey(groupId)) return PurchaseRewardsType.Amount;
            if (_dayEventDatas.ContainsKey(groupId)) return PurchaseRewardsType.Day;
            throw new KeyNotFoundException($"No purchase rewards event data found for groupId: {groupId}");
        }

        public bool IsAmountEventAvailable(int groupId, string isoCurrencyCode)
        {
            return _amountEventDatas.ContainsKey(groupId) && _amountEventDatas[groupId].ContainsKey(isoCurrencyCode);
        }

        public PurchaseRewardsEventData GetAmountEventData(int groupId, string isoCurrencyCode)
        {
            if (_amountEventDatas.ContainsKey(groupId))
            {
                if (_amountEventDatas[groupId].ContainsKey(isoCurrencyCode)) return _amountEventDatas[groupId][isoCurrencyCode];
            }
            
            throw new KeyNotFoundException($"No purchase rewards event data found for groupId: {groupId}, currencyCode: {isoCurrencyCode}");
        }

        public PurchaseRewardsEventData GetDayEventData(int groupId)
        {
            if (_dayEventDatas.ContainsKey(groupId)) return _dayEventDatas[groupId];
            
            throw new KeyNotFoundException($"No purchase rewards event data found for groupId: {groupId}");
        }
        
        public string GetRedDotPath()
        {
            return _redDotPath;
        }
    }
}
