using System.Collections.Generic;

namespace Aloha.Coconut
{
    public struct PurchaseResult
    {
        public bool isSuccess;
        public IReadOnlyList<Property> rewards;
        public string errorMessage;
    }
}