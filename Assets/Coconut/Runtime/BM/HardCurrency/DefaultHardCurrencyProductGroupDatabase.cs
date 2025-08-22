using System.Collections.Generic;

namespace Aloha.Coconut
{
    internal class DefaultHardCurrencyProductGroupDatabase : IHardCurrencyProductGroupDatabase
    {
        private struct HardCurrencyProductGroupTableData
        {
            [CSVColumn] public int groupId;
            [CSVColumn] public string baseProductId;
            [CSVColumn] public string doubleProductId;
        }
        
        public List<HardCurrencyProductGroupData> GetProductGroupDataList()
        {
            return TableManager.Get<HardCurrencyProductGroupTableData>("bm_hard_currency_products")
                .ConvertAll(data => new HardCurrencyProductGroupData(data.groupId, data.baseProductId, data.doubleProductId));
        }
    }
}
