using System.Collections.Generic;

namespace Aloha.Coconut
{
    public interface IHardCurrencyProductGroupDatabase
    {
        public List<HardCurrencyProductGroupData> GetProductGroupDataList();
    }

    public struct HardCurrencyProductGroupData
    {
        public int groupId;
        public string baseProductId;
        public string doubleProductId;

        public HardCurrencyProductGroupData(int groupId, string baseProductId, string doubleProductId)
        {
            this.groupId = groupId;
            this.baseProductId = baseProductId;
            this.doubleProductId = doubleProductId;
        }
    }
}