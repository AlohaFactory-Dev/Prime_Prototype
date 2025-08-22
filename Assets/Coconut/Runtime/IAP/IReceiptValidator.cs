using Cysharp.Threading.Tasks;
using UnityEngine.Purchasing;

namespace Aloha.Coconut.IAP
{
    public struct ReceiptValidationResult
    {
        public UnityEngine.Purchasing.Product product;
        public bool isSuccess;
        public ReceiptValidationFailureReason failureReason;

        public ReceiptValidationResult(UnityEngine.Purchasing.Product product, bool isSuccess, ReceiptValidationFailureReason failureReason)
        {
            this.product = product;
            this.isSuccess = isSuccess;
            this.failureReason = failureReason;
        }
    }

    public enum ReceiptValidationFailureReason
    {
        None,
        InvalidReceipt,
        NetworkError,
        Unknown,
        ReceiptAlreadyUsed,
    }
    
    public interface IReceiptValidator
    {
        protected internal UniTask<ReceiptValidationResult> Validate(UnityEngine.Purchasing.Product product);
    }
}