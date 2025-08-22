using JetBrains.Annotations;

public interface IFlamingoWrapper
{
    void Setup(
        string gameObject,
        string accessKey,
        [CanBeNull] string appVersion
    );

    void Login(string appUserId);

    void LogEvent(
        string eventCode,
        [CanBeNull] string eventData,
        [CanBeNull] string customEventCode
    );

    void StartPlaySession(
        string playSessionInfo,
        [CanBeNull] string customParams
    );

    void EndPlaySession(
        string result,
        [CanBeNull] string customParams
    );

    void FinishTutorial(
        string tutorialId,
        [CanBeNull] string tutorialName,
        [CanBeNull] string customParams
    );

    void PurchaseIAP(
        string store,
        string isoCurrency,
        double price,
        [CanBeNull] string productId,
        [CanBeNull] string transactionId,
        [CanBeNull] string itemId,
        [CanBeNull] string itemName,
        bool isTest,
        [CanBeNull] string customParams
    );

    void ChangeAssetAmount(
        string assetId,
        [CanBeNull] string assetName,
        long amountDiff,
        long resultAmount,
        [CanBeNull] string actionId,
        [CanBeNull] string actionName,
        [CanBeNull] string objectId,
        [CanBeNull] string objectName,
        [CanBeNull] string customParams
    );

    void ChangeItemAmount(
        string itemId,
        [CanBeNull] string itemName,
        long amountDiff,
        long resultAmount,
        [CanBeNull] string actionId,
        [CanBeNull] string actionName,
        [CanBeNull] string objectId,
        [CanBeNull] string objectName,
        [CanBeNull] string customParams
    );
}