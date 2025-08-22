using System.Collections.Generic;
using System.Numerics;

namespace Aloha.Coconut
{
    public class DefaultPropertyHandler: IPropertyHandler
    {
        public List<PropertyTypeGroup> HandlingGroups { get; }
        
        private SaveData _saveData;

        public DefaultPropertyHandler(SaveDataManager saveDataManager)
        {
            HandlingGroups = new List<PropertyTypeGroup> { PropertyTypeGroup.Default };
            _saveData = saveDataManager.Get<SaveData>("default_property_handler");
        }

        public void Obtain(Property property)
        {
            EnsureSaveData(property.type);
            var targetDictionary = property.isPaid ? _saveData.paids : _saveData.balances;
            targetDictionary[property.type.group][property.type.id] += property.amount;
        }

        private void EnsureSaveData(PropertyType propertyType)
        {
            if (!_saveData.balances.ContainsKey(propertyType.group))
            {
                _saveData.balances[propertyType.group] = new Dictionary<int, BigInteger>();
            }

            if (!_saveData.balances[propertyType.group].ContainsKey(propertyType.id))
            {
                _saveData.balances[propertyType.group][propertyType.id] = 0;
            }

            if (!_saveData.paids.ContainsKey(propertyType.group))
            {
                _saveData.paids[propertyType.group] = new Dictionary<int, BigInteger>();
            }

            if (!_saveData.paids[propertyType.group].ContainsKey(propertyType.id))
            {
                _saveData.paids[propertyType.group][propertyType.id] = 0;
            }
        }

        public void Use(Property property)
        {
            // 무료분을 먼저 사용
            if (_saveData.balances[property.type.group][property.type.id] > 0)
            {
                BigInteger freeAmount = BigInteger.Min(property.amount, _saveData.balances[property.type.group][property.type.id]);
                _saveData.balances[property.type.group][property.type.id] -= freeAmount;
                property.amount -= freeAmount;
            }

            // 유료분을 사용
            if (property.amount > 0)
            {
                BigInteger paidAmount = BigInteger.Min(property.amount, _saveData.paids[property.type.group][property.type.id]);
                _saveData.paids[property.type.group][property.type.id] -= paidAmount;
            }
        }

        public void Set(Property property)
        {
            EnsureSaveData(property.type);
            var targetDictionary = property.isPaid ? _saveData.paids : _saveData.balances;
            targetDictionary[property.type.group][property.type.id] = property.amount;
        }

        public BigInteger GetBalance(PropertyType property)
        {
            EnsureSaveData(property);
            return _saveData.balances[property.group][property.id] + _saveData.paids[property.group][property.id];
        }

        private class SaveData
        {
            public Dictionary<PropertyTypeGroup, Dictionary<int, BigInteger>> balances = new();
            public Dictionary<PropertyTypeGroup, Dictionary<int, BigInteger>> paids = new();
        }
    }
}