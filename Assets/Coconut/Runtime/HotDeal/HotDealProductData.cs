namespace Aloha.Coconut.HotDeal
{
    public struct HotDealProductData
    {
        [CSVColumn] public int id;
        [CSVColumn] public string iapId;
        [CSVColumn] public int durationHours;
    }
}