using System.Collections.Generic;

namespace Aloha.Coconut
{
    public interface IPeriodicStoreDatabase
    {
        List<PeriodicStoreProductData> GetProductDatas(ResetPeriod resetPeriod);
        (int, string) GetRVPlacement(ResetPeriod resetPeriod);
        string GetRedDotPath();
        string GetRedDotPath(ResetPeriod resetPeriod);
    }
}
