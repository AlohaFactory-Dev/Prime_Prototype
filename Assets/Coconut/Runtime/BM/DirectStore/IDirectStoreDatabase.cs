using System.Collections.Generic;

namespace Aloha.Coconut
{
    public struct DirectProductData
    {
        [CSVColumn] public int id;
        [CSVColumn] public string iapId;
        [CSVColumn] public int durationHours;
        [CSVColumn] public string prefabKey;
    }

    public interface IDirectStoreDatabase
    {
        public List<DirectProductData> GetProductDatas();
    }
}