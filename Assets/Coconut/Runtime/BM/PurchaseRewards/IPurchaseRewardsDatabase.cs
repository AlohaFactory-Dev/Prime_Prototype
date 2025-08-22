namespace Aloha.Coconut
{
    public interface IPurchaseRewardsDatabase
    {
        PurchaseRewardsType GetPurchaseRewardsType(int groupId);
        bool IsAmountEventAvailable(int groupId, string isoCurrencyCode);
        PurchaseRewardsEventData GetAmountEventData(int groupId, string isoCurrencyCode);
        PurchaseRewardsEventData GetDayEventData(int groupId);
        string GetRedDotPath();
    }
}
