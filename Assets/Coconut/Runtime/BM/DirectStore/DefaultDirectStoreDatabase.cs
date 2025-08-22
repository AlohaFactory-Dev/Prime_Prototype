using System.Collections.Generic;

namespace Aloha.Coconut
{
    internal class DefaultDirectStoreDatabase : IDirectStoreDatabase
    {
        public List<DirectProductData> GetProductDatas()
        {
            return TableManager.Get<DirectProductData>("bm_direct_store");
        }
    }
}
