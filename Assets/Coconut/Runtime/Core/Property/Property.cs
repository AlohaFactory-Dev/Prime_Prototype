using System.Numerics;

namespace Aloha.Coconut
{
#pragma warning disable CS0660, CS0661
    public struct Property
#pragma warning restore CS0660, CS0661
    {
        public bool IsValid => type != null;
        public readonly PropertyType type;
        public BigInteger amount;
        public bool isPaid;
        
        private const string TAG_PAID = "[p]";

        public Property(PropertyType type, BigInteger amount, bool isPaid = false)
        {
            this.type = type;
            this.amount = amount;
            this.isPaid = isPaid;
        }

        public Property(PropertyTypeGroup propertyTypeGroup, int propertyId, BigInteger amount, bool isPaid = false)
        {
            this.type = PropertyType.Get(propertyTypeGroup, propertyId);
            this.amount = amount;
            this.isPaid = isPaid;
        }
        
        public Property(string alias, BigInteger amount)
        {
            isPaid = false;
            if (alias.StartsWith(TAG_PAID))
            {
                int tagLength = TAG_PAID.Length;
                alias = alias.Substring(tagLength);
                isPaid = true;
            }
            
            this.type = PropertyType.Get(alias);
            this.amount = amount;
        }
        
        public Property(string alias, BigInteger amount, bool isPaid = false)
        {
            this.type = PropertyType.Get(alias);
            this.amount = amount;
            this.isPaid = isPaid;
        }
        
        public Property(PropertyTypeAlias alias, BigInteger amount, bool isPaid = false)
        {
            this.type = PropertyType.Get(alias);
            this.amount = amount;
            this.isPaid = isPaid;
        }
        
        public static Property operator +(Property a, Property b)
        {
            if (a.type != b.type)
            {
                throw new System.Exception("PropertyType이 다릅니다.");
            }
            return new Property(a.type, a.amount + b.amount);
        }
        
        public static Property operator -(Property a, Property b)
        {
            if (a.type != b.type)
            {
                throw new System.Exception("PropertyType이 다릅니다.");
            }
            return new Property(a.type, a.amount - b.amount);
        }
        
        public static Property operator *(Property a, BigInteger b)
        {
            return new Property(a.type, a.amount * b);
        }
        
        public static Property operator /(Property a, BigInteger b)
        {
            return new Property(a.type, a.amount / b);
        }
        
        public static bool operator ==(Property a, Property b)
        {
            return a.type == b.type && a.amount == b.amount;
        }
        
        public static bool operator !=(Property a, Property b)
        {
            return !(a == b);
        }
    }
}