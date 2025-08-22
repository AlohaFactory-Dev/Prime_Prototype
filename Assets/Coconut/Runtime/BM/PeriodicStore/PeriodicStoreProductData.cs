namespace Aloha.Coconut
{
    public class PeriodicStoreProductData
    {
        [CSVColumn] public ResetPeriod resetPeriod;
        [CSVColumn] public int productId;
        [CSVColumn] public int isFree;
        [CSVColumn] public string iapId;
        [CSVColumn] public int limit;
    }
}