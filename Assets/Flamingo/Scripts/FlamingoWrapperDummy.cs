public class FlamingoWrapperDummy : IFlamingoWrapper
{
    public void Setup(string gameObject, string accessKey, string appVersion)
    {
    }

    public void Login(string appUserId)
    {
    }

    public void LogEvent(string eventCode, string eventData, string customEventCode)
    {
    }

    public void StartPlaySession(string playSessionInfo, string customParams)
    {
    }

    public void EndPlaySession(string result, string customParams)
    {
    }

    public void FinishTutorial(string tutorialId, string tutorialName, string customParams)
    {
    }

    public void PurchaseIAP(string store, string isoCurrency, double price, string productId, string transactionId,
        string itemId,
        string itemName, bool isTest, string customParams)
    {
    }

    public void ChangeAssetAmount(string assetId, string assetName, long amountDiff, long resultAmount, string actionId,
        string actionName, string objectId, string objectName, string customParams)
    {
    }

    public void ChangeItemAmount(string itemId, string itemName, long amountDiff, long resultAmount, string actionId,
        string actionName, string objectId, string objectName, string customParams)
    {
    }
}