using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using Zenject;

namespace Aloha.Coconut.IAP
{
    public class IAPManager : IDetailedStoreListener, IIAPManager, IPropertyManagerRequirer
    {
        private readonly SaveDataManager _saveDataManager;
        private readonly IReceiptValidator _receiptValidator;
        private readonly IPackageRewardsManager _packageRewardsManager;

        public bool IsInitialized { get; private set; }
        public string CurrencyCode { get; private set; }
        public event Action<bool> OnInitialized;

        public PropertyManager PropertyManager
        {
            private get => _propertyManager;
            set
            {
                _propertyManager = value;
                while (_onValidationCompleteBuffer.Count > 0)
                {
                    OnValidationComplete(_onValidationCompleteBuffer.Dequeue());
                }
            }
        }

        private PropertyManager _propertyManager;
        private Queue<ReceiptValidationResult> _onValidationCompleteBuffer = new Queue<ReceiptValidationResult>();

        public IObservable<IAPResult> OnIAPProcessed => _onIAPProcessed;
        private readonly Subject<IAPResult> _onIAPProcessed = new();

        private IStoreController _storeController;
        private SaveData _saveData;
        private IReadOnlyList<IAPProductInfo> _productInfos;

        public IAPManager(SaveDataManager saveDataManager, IPackageRewardsManager packageRewardsManager,
            [InjectOptional] IReceiptValidator receiptValidator)
        {
            _saveDataManager = saveDataManager;
            _receiptValidator = receiptValidator ?? new FakeReceiptValidator();
            _packageRewardsManager = packageRewardsManager;

            _productInfos = TableManager.Get<IAPProductInfo>("iap_product_infos");
        }

        public void Initialize()
        {
            _saveData = _saveDataManager.Get<SaveData>("iap_manager");

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var productInfo in _productInfos)
            {
                builder.AddProduct(productInfo.productId, productInfo.productType,
                    new IDs()
                    {
                        {productInfo.productIdGoogle, GooglePlay.Name},
                        {productInfo.productIdApple, AppleAppStore.Name}
                    });

                PlayerAction.Add(15000, "iap", GetActionId(productInfo.productId), productInfo.productName);
            }

            UnityPurchasing.Initialize(this, builder);
        }

        public IAPProduct GetProduct(string iapId)
        {
            IAPProductInfo productInfo = new IAPProductInfo();
            foreach (IAPProductInfo info in _productInfos)
            {
                if (info.productId.Equals(iapId))
                {
                    productInfo = info;
                    break;
                }
            }

            return new IAPProduct(iapId, productInfo.nameKey, productInfo.efficiency, GetRewards(iapId), this);
        }

        public List<Property> GetRewards(string packageId)
        {
            if (_saveData.overridenRewards.TryGetValue(packageId, out string overridenReward))
            {
                packageId = overridenReward;
            }

            return _packageRewardsManager.GetPackageRewards(packageId, true);
        }

        private int GetActionId(string iapId)
        {
            return iapId.GetHashCode();
        }

        public void AddOnInitializedListener(Action<bool> listener)
        {
            if (IsInitialized)
            {
                listener(IsInitialized);
            }
            else
            {
                OnInitialized += listener;
            }
        }

        #region IDetailedStoreListener

        void IStoreListener.OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            CurrencyCode = _storeController.products.all[0].metadata.isoCurrencyCode;

            IsInitialized = true;
            OnInitialized?.Invoke(true);
            OnInitialized = null;
        }

        void IStoreListener.OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"IAPManager.OnInitializeFailed: {error}");
        }

        void IStoreListener.OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"IAPManager.OnInitializeFailed: {message}");
        }

        PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            _receiptValidator.Validate(purchaseEvent.purchasedProduct)
                .ContinueWith(OnValidationComplete);
            return PurchaseProcessingResult.Pending;
        }

        void IDetailedStoreListener.OnPurchaseFailed(UnityEngine.Purchasing.Product product,
            PurchaseFailureDescription failureDescription)
        {
            _onIAPProcessed.OnNext(new IAPResult(product.definition.id, product.metadata.isoCurrencyCode,
                product.metadata.localizedPrice, false, null, IAPFailureReason.PaymentFailed));
            Debug.LogError($"IAPManager.OnPurchaseFailed: {failureDescription.productId}_{failureDescription.message}");
        }

        void IStoreListener.OnPurchaseFailed(UnityEngine.Purchasing.Product product,
            PurchaseFailureReason failureReason)
        {
            _onIAPProcessed.OnNext(new IAPResult(product.definition.id, product.metadata.isoCurrencyCode,
                product.metadata.localizedPrice, false, null, IAPFailureReason.PaymentFailed));
            Debug.LogError($"IAPManager.OnPurchaseFailed: {failureReason}");
        }

        #endregion

        async UniTask<IAPResult> IIAPManager.Purchase(string iapId)
        {
#if UNITY_EDITOR
            Observable.Timer(TimeSpan.FromSeconds(1))
                .Subscribe(_ => _storeController.InitiatePurchase(iapId));
#else
            _storeController.InitiatePurchase(iapId);
#endif
            return await _onIAPProcessed.First();
        }

        string IIAPManager.GetLocalizedPriceString(string iapId)
        {
            return _storeController.products.WithID(iapId).metadata.localizedPriceString;
        }

        private void OnValidationComplete(ReceiptValidationResult result)
        {
            if (PropertyManager == null)
            {
                _onValidationCompleteBuffer.Enqueue(result);
                return;
            }

            if (result.isSuccess)
            {
                var productRewards = GetRewards(result.product.definition.id);
                var rewards = PropertyManager.Obtain(productRewards,
                    PlayerAction.Get(GetActionId(result.product.definition.id)));
                _storeController.ConfirmPendingPurchase(result.product);

                _saveData.purchaseCount.TryAdd(result.product.definition.id, 0);
                _saveData.purchaseCount[result.product.definition.id]++;

                BroadcastIAPCompleteEvent(result.product.definition.id);
                _onIAPProcessed.OnNext(new IAPResult(result.product.definition.id,
                    result.product.metadata.isoCurrencyCode, result.product.metadata.localizedPrice, true, rewards));
            }
            else
            {
                if (result.failureReason == ReceiptValidationFailureReason.InvalidReceipt)
                {
                    _storeController.ConfirmPendingPurchase(result.product);
                    _onIAPProcessed.OnNext(new IAPResult(result.product.definition.id,
                        result.product.metadata.isoCurrencyCode, result.product.metadata.localizedPrice, false, null,
                        IAPFailureReason.InvalidReceipt));
                }
                // InvalidReceipt가 아닌 다른 이유로 실패했을 경우, 추후 재처리가 필요하므로 Confirm하지 않음
                else if (result.failureReason == ReceiptValidationFailureReason.NetworkError)
                {
                    _onIAPProcessed.OnNext(new IAPResult(result.product.definition.id,
                        result.product.metadata.isoCurrencyCode, result.product.metadata.localizedPrice, false, null,
                        IAPFailureReason.NetworkError));
                }
                else
                {
                    _onIAPProcessed.OnNext(new IAPResult(result.product.definition.id,
                        result.product.metadata.isoCurrencyCode, result.product.metadata.localizedPrice, false, null,
                        IAPFailureReason.Unknown));
                }
            }
        }

        private void BroadcastIAPCompleteEvent(string iapId)
        {
            var product = _storeController.products.WithID(iapId);
            string transactionId = "";
            if (!Application.isEditor)
            {
#if UNITY_ANDROID
                int first = product.receipt.IndexOf("GPA");
                transactionId = product.receipt.Substring(first, 24);
#elif UNITY_IOS
                transactionId = product.transactionID;
#endif
            }

            string storeId = "";
            IAPProductInfo productInfo = new IAPProductInfo();
            foreach (IAPProductInfo info in _productInfos)
            {
                if (!info.productId.Equals(iapId)) continue;
                
#if UNITY_ANDROID
                storeId = info.productIdGoogle;
#elif UNITY_IOS
                storeId = info.productIdApple;
#endif
                break;
            }

            EventBus.Broadcast(new EVIAPComplete(product.metadata.isoCurrencyCode,
                (double) product.metadata.localizedPrice, product.definition.id, GetProductName(iapId), transactionId,
                storeId, product.metadata.localizedTitle));
        }

        private string GetProductName(string iapId)
        {
            foreach (var productData in _productInfos)
            {
                if (productData.productId.Equals(iapId))
                {
                    return productData.productName;
                }
            }

            return null;
        }

        public int GetPurchaseCount(string iapId)
        {
            return _saveData.purchaseCount.GetValueOrDefault(iapId, 0);
        }

        public void OverrideRewards(string iapId, string overridenPackageId)
        {
            _saveData.overridenRewards[iapId] = overridenPackageId;
        }

        public void ResetOverrideRewards(string iapId)
        {
            _saveData.overridenRewards.Remove(iapId);
        }

        private class SaveData
        {
            public Dictionary<string, int> purchaseCount = new Dictionary<string, int>();
            public Dictionary<string, string> overridenRewards = new Dictionary<string, string>();
        }

        private struct IAPProductInfo
        {
            [CSVColumn] public string productId;
            [CSVColumn] public string productIdGoogle;
            [CSVColumn] public string productIdApple;
            [CSVColumn] public ProductType productType;
            [CSVColumn] public string productName;
            [CSVColumn] public string nameKey;
            [CSVColumn] public int efficiency;
        }
    }
}