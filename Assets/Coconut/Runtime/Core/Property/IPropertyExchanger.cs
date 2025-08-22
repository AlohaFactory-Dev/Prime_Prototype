using System.Collections.Generic;

namespace Aloha.Coconut
{
    public interface IPropertyExchanger
    {
        List<PropertyTypeGroup> HandlingGroups { get; }
        List<Property> Exchange(Property property);
    }
}