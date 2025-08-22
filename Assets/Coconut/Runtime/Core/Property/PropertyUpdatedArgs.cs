using System.Numerics;

namespace Aloha.Coconut
{
    public struct PropertyUpdatedArgs
    {
        public PropertyType type;
        public BigInteger balance;
        public BigInteger variance;

        public PropertyUpdatedArgs(PropertyType type, BigInteger balance, BigInteger variance)
        {
            this.type = type;
            this.balance = balance;
            this.variance = variance;
        }
    }
}