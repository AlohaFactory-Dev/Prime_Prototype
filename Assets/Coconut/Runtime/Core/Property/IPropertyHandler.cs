using System.Collections.Generic;
using System.Numerics;

namespace Aloha.Coconut
{
    public interface IPropertyHandler
    {
        List<PropertyTypeGroup> HandlingGroups { get; }
        void Obtain(Property property);
        void Use(Property property);
        void Set(Property property);
        BigInteger GetBalance(PropertyType property);
    }
}
