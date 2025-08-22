using UnityEngine;

public class FlamingoWrapperAndroid : IFlamingoWrapper
{
    public void Setup(string gameObject, string accessKey, string appVersion)
    {
        CallMethod("setup", gameObject, accessKey, appVersion);
    }

    public void Login(string appUserId)
    {
        CallMethod("login", appUserId);
    }

    public void LogEvent(string eventCode, string eventData, string customEventCode)
    {
        CallMethod("logEvent", eventCode, eventData, customEventCode);
    }

    public void StartPlaySession(string playSessionInfo, string customParams)
    {
        CallMethod("startPlaySession", playSessionInfo, customParams);
    }

    public void EndPlaySession(string result, string customParams)
    {
        CallMethod("endPlaySession", result, customParams);
    }

    public void FinishTutorial(string tutorialId, string tutorialName, string customParams)
    {
        CallMethod("finishTutorial", tutorialId, tutorialName, customParams);
    }

    public void PurchaseIAP(string store, string isoCurrency, double price, string productId, string transactionId,
        string itemId,
        string itemName, bool isTest, string customParams)
    {
        CallMethod("purchaseIAP", store, isoCurrency, price, productId, transactionId, itemId, itemName, isTest,
            customParams);
    }

    public void ChangeAssetAmount(string assetId, string assetName, long amountDiff, long resultAmount, string actionId,
        string actionName, string objectId, string objectName, string customParams)
    {
        CallMethod("changeAssetAmount", assetId, assetName, amountDiff, resultAmount, actionId, actionName, objectId,
            objectName, customParams);
    }

    public void ChangeItemAmount(string itemId, string itemName, long amountDiff, long resultAmount, string actionId,
        string actionName, string objectId, string objectName, string customParams)
    {
        CallMethod("changeItemAmount", itemId, itemName, amountDiff, resultAmount, actionId, actionName, objectId,
            objectName, customParams);
    }

    private const string FlamingoWrapperClassName = "ai.flmg.unity.FlamingoWrapper";

    private static void CallMethod(string methodName, params object[] args)
    {
        using var flamingo = new AndroidJavaClass(FlamingoWrapperClassName);
        flamingo.CallStatic(methodName, args);
    }
}