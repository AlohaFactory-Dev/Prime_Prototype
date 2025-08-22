using System;
using System.Globalization;
using UnityEngine;

public class Flamingo : MonoBehaviour
{
    [Tooltip("Flamingo Access Key")] public string accessKey;

    private IFlamingoWrapper _flamingo;

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _flamingo = new FlamingoWrapperAndroid();
#elif (UNITY_IOS || UNITY_VISIONOS) && !UNITY_EDITOR
        _flamingo = new FlamingoWrapperiOS();
#else
        _flamingo = new FlamingoWrapperDummy();
#endif
        _flamingo.Setup(gameObject.name, accessKey, Application.version);
    }

    private void _handleFlamingoLog(string log)
    {
        Debug.Log(log);
    }

    public void Login(string appUserId)
    {
        _flamingo.Login(appUserId);
    }

    public void Log(string eventCode, string eventData, string customEventCode)
    {
        _flamingo.LogEvent(eventCode, eventData, customEventCode);
    }

    public void StartPlaySession(string playSessionInfo, string customParams)
    {
        _flamingo.StartPlaySession(playSessionInfo, customParams);
    }

    public void EndPlaySession(string result, string customParams)
    {
        _flamingo.EndPlaySession(result, customParams);
    }

    public void FinishTutorial(string tutorialId, string tutorialName, string customParams)
    {
        _flamingo.FinishTutorial(tutorialId, tutorialName, customParams);
    }

    public void PurchaseIAP(string store, string isoCurrency, double price, string productId, string transactionId,
        string itemId, string itemName, bool isTest, string customParams)
    {
        _flamingo.PurchaseIAP(store, isoCurrency, price, productId, transactionId, itemId, itemName, isTest, customParams);
    }

    public void ChangeAssetAmount(string assetId, string assetName, long amountDiff, long resultAmount, string actionId,
        string actionName, string objectId, string objectName, string customParams)
    {
        _flamingo.ChangeAssetAmount(assetId, assetName, amountDiff, resultAmount, actionId, actionName, objectId, objectName, customParams);
    }

    public void ChangeItemAmount(string itemId, string itemName, long amountDiff, long resultAmount, string actionId,
        string actionName, string objectId, string objectName, string customParams)
    {
        _flamingo.ChangeItemAmount(itemId, itemName, amountDiff, resultAmount, actionId, actionName, objectId, objectName, customParams);
    }
}