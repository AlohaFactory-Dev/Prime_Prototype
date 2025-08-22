using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Aloha.Coconut.IAP
{
    // 테스트용 또는 서버 붙이기 전 사용
    public class FakeReceiptValidator : IReceiptValidator
    {
        public bool IsSuccess { get; set; } = true;
        public ReceiptValidationFailureReason FailureReason { get; set; } = ReceiptValidationFailureReason.None;
        
        async UniTask<ReceiptValidationResult> IReceiptValidator.Validate(UnityEngine.Purchasing.Product product)
        {
            Debug.Log("Fake Receipt Validator: Receipt validation success");
            return new ReceiptValidationResult
            {
                product = product,
                isSuccess = IsSuccess,
                failureReason = FailureReason
            };
        }
    }
}