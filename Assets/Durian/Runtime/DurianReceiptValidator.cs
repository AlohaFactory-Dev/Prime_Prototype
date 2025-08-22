using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aloha.Coconut;
using Aloha.Coconut.IAP;
using Alohacorp.Durian.Api;
using Alohacorp.Durian.Model;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Zenject;
using Product = UnityEngine.Purchasing.Product;
using Random = UnityEngine.Random;

namespace Aloha.Durian
{
    public class DurianReceiptValidator : IReceiptValidator
    {
        private class ValidationFailedException : Exception
        {
            public ReceiptValidationFailureReason Reason { get; }
            
            public ValidationFailedException(ReceiptValidationFailureReason reason, string message) : base(message)
            {
                Reason = reason;
            }
        }
        
        public static bool IsFraudSimulationOn { get; set; }
        private static string _lastReceipt;
        
        private readonly LazyInject<SaveDataManager> _saveDataManager;
        private SaveData _saveData;
        private object _lock = new object();

        public DurianReceiptValidator(LazyInject<SaveDataManager> saveDataManager)
        {
            _saveDataManager = saveDataManager;
        }
        
        async UniTask<ReceiptValidationResult> IReceiptValidator.Validate(Product product)
        {
            if (_saveData == null)
            {
                _saveData = _saveDataManager.Value.Get<SaveData>("receipt_validator");
            }
            
            try
            {
                uint hash;
                ////////////////////// Editor ////////////////////
                if (Application.isEditor)
                {
                    string randomReceipt = IsFraudSimulationOn ? _lastReceipt : Random.Range(int.MinValue, int.MaxValue).ToString();
                    hash = ToHash(randomReceipt);
                    CheckAlreadyValidated(hash);
                    
                    await UniTask.Delay(TimeSpan.FromSeconds(1));
                    CheckAlreadyValidated(hash);
                    
                    lock (_lock)
                    {
                        _saveData.validated.Add(hash);
                        _lastReceipt = randomReceipt;   
                    }
                    return new ReceiptValidationResult(product, true, ReceiptValidationFailureReason.None);
                }
                /////////////////////////////////////////////////////
                
                IAPApi iapApi = await DurianApis.IAPApi();
                Task<RootReceiptDto> verificationTask;
                if (Application.platform == RuntimePlatform.Android)
                {
                    string purchaseToken = GetGooglePurchaseToken(product.receipt);
                    hash = ToHash(purchaseToken);
                    CheckAlreadyValidated(hash);
                    verificationTask = iapApi.VerifyGooglePlayPurchaseAsync(new VerifyGooglePlayReceiptReqDto(product.definition.storeSpecificId, purchaseToken));
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    string receiptString = GetIOSReceiptString(product.receipt);
                    hash = ToHash(receiptString);
                    CheckAlreadyValidated(hash);
                    verificationTask = iapApi.VerifyAppStorePurchaseAsync(new VerifyAppStoreReceiptReqDto(receiptString));
                }
                else
                {
                    throw new ValidationFailedException(ReceiptValidationFailureReason.Unknown, "Unsupported platform");
                }
                
                RootReceiptDto rootReceiptDto = await verificationTask;
                CheckAlreadyValidated(hash); // await 중에 validate에 추가되었을 수도 있으므로, 한 번 더 체크

                Debug.Log($"Receipt validation result: {rootReceiptDto.Data.IsValid}");
                if (rootReceiptDto.Data.IsValid)
                {
                    lock (_lock)
                    {
                        _saveData.validated.Add(hash);
                        _lastReceipt = product.receipt;
                    }
                    return new ReceiptValidationResult(product, true, ReceiptValidationFailureReason.None);
                }
                else
                {
                    throw new ValidationFailedException(ReceiptValidationFailureReason.InvalidReceipt, "Invalid Receipt");
                }
            }
            catch (ValidationFailedException e)
            {
                Debug.LogError($"Failed to validate receipt: {e.Message}");
                return new ReceiptValidationResult(product, false, e.Reason);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to validate receipt: {e.Message}");
                return new ReceiptValidationResult(product, false, ReceiptValidationFailureReason.NetworkError);
            }
        }

        private void CheckAlreadyValidated(uint hash)
        {
            lock (_lock)
            {
                if (_saveData.validated.Contains(hash))
                {
                    Debug.LogError("Already validated");
                    throw new ValidationFailedException(ReceiptValidationFailureReason.ReceiptAlreadyUsed, "Already validated");
                }   
            }
        }

        private string GetGooglePurchaseToken(string receipt)
        {
            if (IsFraudSimulationOn) receipt = _lastReceipt;
            
            try
            {
                JObject root = JObject.Parse(receipt);

                // "TransactionID" 필드 읽기
                var transactionID = root["TransactionID"];
                if (transactionID == null || transactionID.Type != JTokenType.String || string.IsNullOrEmpty(transactionID.ToString()))
                {
                    throw new Exception("TransactionID가 유효하지 않습니다.");
                }

                return transactionID.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON 파싱 중 오류 발생: {ex.Message}");
                return null;
            }
        }

        private string GetIOSReceiptString(string receipt)
        {
            if (IsFraudSimulationOn) receipt = _lastReceipt;
            
            try
            {
                JObject root = JObject.Parse(receipt);

                var payload = root["Payload"];
                if (payload == null || payload.Type != JTokenType.String || string.IsNullOrEmpty(payload.ToString()))
                {
                    throw new Exception("Payload가 유효하지 않습니다.");
                }

                return payload.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError($"JSON 파싱 중 오류 발생: {ex.Message}");
                return null;
            }
        }

        private static uint ToHash(string s)
        {
            uint hash = 0;
            for (var i = 0; i < s.Length; i++)
            {
                hash = (hash << 5) - hash + s[i];
            }

            return hash;
        }

        private class SaveData
        {
            public List<uint> validated = new List<uint>();
        }
    }
}
