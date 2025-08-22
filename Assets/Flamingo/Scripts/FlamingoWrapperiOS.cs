using System.Runtime.InteropServices;

#if UNITY_IOS || UNITY_VISIONOS
public class FlamingoWrapperiOS : IFlamingoWrapper
{
    [DllImport("__Internal")]
    private static extern void _ALFlamingoSetup(string gameObject, string accessKey, string appVersion);

    public void Setup(string gameObject, string accessKey, string appVersion)
    {
        _ALFlamingoSetup(gameObject, accessKey, appVersion);
    }

    [DllImport("__Internal")]
    private static extern void _ALFlamingoLogin(string appUserId);

    public void Login(string appUserId)
    {
        _ALFlamingoLogin(appUserId);
    }

    [DllImport("__Internal")]
    private static extern void _ALFlamingoLogEvent(string eventCode, string eventData, string customEventCode);

    public void LogEvent(string eventCode, string eventData, string customEventCode)
    {
        _ALFlamingoLogEvent(eventCode, eventData, customEventCode);
    }

    [DllImport("__Internal")]
    private static extern void _ALFlamingoStartPlaySession(string playSessionInfo, string customParams);

    public void StartPlaySession(string playSessionInfo, string customParams)
    {
        _ALFlamingoStartPlaySession(playSessionInfo, customParams);
    }

    [DllImport("__Internal")]
    private static extern void _ALFlamingoEndPlaySession(string result, string customParams);

    public void EndPlaySession(string result, string customParams)
    {
        _ALFlamingoEndPlaySession(result, customParams);
    }

    [DllImport("__Internal")]
    private static extern void _ALFlamingoFinishTutorial(string tutorialId, string tutorialName, string customParams);

    public void FinishTutorial(string tutorialId, string tutorialName, string customParams)
    {
        _ALFlamingoFinishTutorial(tutorialId, tutorialName, customParams);
    }

    [DllImport("__Internal")]
    private static extern void _ALFlamingoPurchaseIAP(string store, string isoCurrency, double price, string productId, string transactionId,
        string itemId, string itemName, bool isTest, string customParams);

    public void PurchaseIAP(string store, string isoCurrency, double price, string productId, string transactionId,
        string itemId,
        string itemName, bool isTest, string customParams)
    {
        _ALFlamingoPurchaseIAP(store, isoCurrency, price, productId, transactionId, itemId, itemName, isTest, customParams);
    }

    [DllImport("__Internal")]
    private static extern void _ALFlamingoChangeAssetAmount(string assetId, string assetName, long amountDiff, long resultAmount, string actionId,
        string actionName, string objectId, string objectName, string customParams);

    public void ChangeAssetAmount(string assetId, string assetName, long amountDiff, long resultAmount, string actionId,
        string actionName, string objectId, string objectName, string customParams)
    {
        _ALFlamingoChangeAssetAmount(assetId, assetName, amountDiff, resultAmount, actionId, actionName, objectId, objectName, customParams);
    }

    [DllImport("__Internal")]
    private static extern void _ALFlamingoChangeItemAmount(string itemId, string itemName, long amountDiff, long resultAmount, string actionId,
        string actionName, string objectId, string objectName, string customParams);

    public void ChangeItemAmount(string itemId, string itemName, long amountDiff, long resultAmount, string actionId,
        string actionName, string objectId, string objectName, string customParams)
    {
        _ALFlamingoChangeItemAmount(itemId, itemName, amountDiff, resultAmount, actionId, actionName, objectId, objectName, customParams);
    }
}
#endif