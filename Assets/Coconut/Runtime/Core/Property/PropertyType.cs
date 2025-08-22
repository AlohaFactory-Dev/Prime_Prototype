using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Aloha.Coconut
{
    public class PropertyType
    {
        public readonly PropertyTypeGroup group;
        public readonly int id;
        public readonly string alias;
        public readonly int rarity;
        public readonly string iconPath;
        public readonly string nameKey;
        public readonly string descriptionKey;
        
        public PropertyTypeAlias Alias => Enum.TryParse<PropertyTypeAlias>(alias, out var result) ? result : 0;
        
        private PropertyType(PropertyTypeGroup group, int id, string alias)
        {
            this.group = group;
            this.id = id;
            this.alias = alias;
        }

        private PropertyType(Raw raw)
        {
            group = raw.group;
            id = raw.id;
            alias = raw.alias;
            rarity = raw.rarity;
            iconPath = raw.iconPath;
            nameKey = raw.nameKey;
            descriptionKey = raw.descriptionKey;
        }
        
        public Property ToProperty(BigInteger amount)
        {
            return new Property(this, amount);
        }
        
        private static Dictionary<PropertyTypeGroup, Dictionary<int, PropertyType>> _propertyTypesByGroupAndId = new();
        private static Dictionary<string, PropertyType> _propertyTypesByAlias = new();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Load()
        {
            _propertyTypesByGroupAndId = new Dictionary<PropertyTypeGroup, Dictionary<int, PropertyType>>();
            _propertyTypesByAlias = new Dictionary<string, PropertyType>();

            var propertyTypeRaws = TableManager.Get<Raw>("property_types");
            foreach (var propertyTypeRaw in propertyTypeRaws)
            {
                var propertyType = new PropertyType(propertyTypeRaw);
                if (!_propertyTypesByGroupAndId.ContainsKey(propertyType.group))
                {
                    _propertyTypesByGroupAndId[propertyType.group] = new Dictionary<int, PropertyType>();
                }

                Assert.IsTrue(!_propertyTypesByGroupAndId[propertyType.group].ContainsKey(propertyType.id),
                    $"PropertyType group: {propertyType.group}, id: {propertyType.id}이 중복되었습니다.");
                Assert.IsTrue(!_propertyTypesByAlias.ContainsKey(propertyType.alias),
                    $"PropertyType alias: {propertyType.alias}이 중복되었습니다.");
                
                _propertyTypesByGroupAndId[propertyType.group][propertyType.id] = propertyType;
                _propertyTypesByAlias[propertyType.alias] = propertyType;
            }
            
            Debug.Log($"Coconut.PropertyType: PropertyType {_propertyTypesByAlias.Count}개 로드되었습니다.");
        }

        public static void Clear()
        {
            _propertyTypesByGroupAndId.Clear();
            _propertyTypesByAlias.Clear();
        }

        public static PropertyType Get(string alias)
        {
            return _propertyTypesByAlias[alias];
        }

        public static PropertyType Get(PropertyTypeAlias alias)
        {
            return Get(alias.ToString());
        }
        
        public static PropertyType Get(PropertyTypeGroup group, int id)
        {
            return _propertyTypesByGroupAndId[group][id];
        }

        public static PropertyType AddType(PropertyTypeGroup group, int id, string alias)
        {
            if (!_propertyTypesByGroupAndId.ContainsKey(group))
            {
                _propertyTypesByGroupAndId[group] = new Dictionary<int, PropertyType>();
            }

            Assert.IsFalse(_propertyTypesByGroupAndId[group].ContainsKey(id), $"PropertyType group: {group}, id: {id}이 중복되었습니다.");
            _propertyTypesByGroupAndId[group][id] = new PropertyType(group, id, alias);
            _propertyTypesByAlias[alias] = _propertyTypesByGroupAndId[group][id];
            
            return _propertyTypesByGroupAndId[group][id];
        }
        
        public static void DeleteType(PropertyTypeGroup group, int id)
        {
            var propertyType = _propertyTypesByGroupAndId[group][id];
            _propertyTypesByGroupAndId[group].Remove(id);
            _propertyTypesByAlias.Remove(propertyType.alias);
        }

        public static void DeleteType(string alias)
        {
            var propertyType = _propertyTypesByAlias[alias];
            _propertyTypesByGroupAndId[propertyType.group].Remove(propertyType.id);
            _propertyTypesByAlias.Remove(alias);
        }

        public static List<PropertyType> GetAll()
        {
            var propertyTypes = new List<PropertyType>();
            foreach (var group in _propertyTypesByGroupAndId.Values)
            {
                propertyTypes.AddRange(group.Values);
            }

            return propertyTypes;
        }
        
        public static List<PropertyType> GetAll(PropertyTypeGroup group)
        {
            return _propertyTypesByGroupAndId[group].Values.ToList();
        }
        
        public static implicit operator PropertyType(PropertyTypeAlias propertyTypeAlias)
        {
            return Get(propertyTypeAlias);
        }

        // PropertyType은 사용자가 직접 new()하면 안되지만, CSVReader에게 DefaultConstructor는 public으로 제공되어야 함.
        // PropertyType Constructor를 private 처리하기 위해 별도의 Raw struct를 만들어 사용.
        private struct Raw
        {
            [CSVColumn] public readonly PropertyTypeGroup group;
            [CSVColumn] public readonly int id;
            [CSVColumn] public readonly string alias;
            [CSVColumn] public readonly int rarity;
            [CSVColumn] public readonly string iconPath;
            [CSVColumn] public readonly string nameKey;
            [CSVColumn] public readonly string descriptionKey;
        }
    }
}