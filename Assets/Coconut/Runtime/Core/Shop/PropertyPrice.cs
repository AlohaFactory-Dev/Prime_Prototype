using Cysharp.Threading.Tasks;
using Zenject;

namespace Aloha.Coconut
{
    public class PropertyPrice : IPrice
    {
        public Property Property;
        private readonly PropertyManager _propertyManager;

        public PropertyPrice(Property property, PropertyManager propertyManager)
        {
            Property = property;
            _propertyManager = propertyManager;
        }

        public async UniTask<bool> Pay(PlayerAction playerAction)
        {
            if (_propertyManager.GetBalance(Property.type) < Property.amount) return false;
            _propertyManager.Use(Property, playerAction);
            return true;
        }

        public bool IsPayable()
        {
            return _propertyManager.GetBalance(Property.type) >= Property.amount;
        }

        public string GetPriceString()
        {
            return $"<sprite name=\"{Property.type.alias}\"> {Property.amount}";
        }
        
        public class Factory : PlaceholderFactory<Property, PropertyPrice> { }
    }
}