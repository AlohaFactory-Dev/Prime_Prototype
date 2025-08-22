using System.Collections.Generic;
using UnityEngine;

namespace Aloha.Coconut
{
    public static class GameConfig
    {
        private struct Entry
        {
            [CSVColumn] public string group;
            [CSVColumn] public string key;
            [CSVColumn] public string value;
        }
        
        private static Dictionary<string, string> _configTable = new Dictionary<string, string>();
        private static Dictionary<string, int> _intCache = new Dictionary<string, int>();
        private static Dictionary<string, float> _floatCache = new Dictionary<string, float>();
        private static Dictionary<string, PropertyTypeGroup> _propertyTypeGroupCache = new Dictionary<string, PropertyTypeGroup>();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Load()
        {
            Clear();
            
            var gameConfigTable = TableManager.Get<Entry>("game_config");
            foreach (var entry in gameConfigTable)
            {
                string key = $"{entry.group}/{entry.key}";
                _configTable.Add(key, entry.value);
            }
        }

        public static void Clear()
        {
            _configTable.Clear();
            _intCache.Clear();
            _floatCache.Clear();
        }

        public static bool HaveKey(string key)
        {
            return _configTable.ContainsKey(key);
        }

        public static bool HaveKey(string group, string key)
        {
            return _configTable.ContainsKey($"{group}/{key}");
        }
        
        public static void Set(string key, string value)
        {
            _configTable[key] = value;
        }
        
        public static void Set(string group, string key, string value)
        {
            Set($"{group}/{key}", value);
        }
        
        public static string GetString(string key)
        {
            if (!HaveKey(key))
            {
                Debug.LogWarning($"GameConfig - key not found: {key}");
                return null;
            }
            
            return _configTable[key];
        }

        public static string GetString(string group, string key)
        {
            return GetString($"{group}/{key}");
        }
        
        public static int GetInt(string key, int defaultValue)
        {
            if (!_intCache.ContainsKey(key))
            {
                var configValue = GetString(key);
                if (configValue == null) return defaultValue;
                _intCache.Add(key, int.Parse(configValue));
            }
            
            return _intCache[key];
        }

        public static int GetInt(string group, string key, int defaultValue = 0)
        {
            return GetInt($"{group}/{key}", defaultValue);
        }
        
        public static float GetFloat(string key, float defaultValue)
        {
            if (!_floatCache.ContainsKey(key))
            {
                var configValue = GetString(key);
                if (configValue == null) return defaultValue;
                _floatCache.Add(key, float.Parse(configValue));
            }

            return _floatCache[key];
        }

        public static float GetFloat(string group, string key, float defaultValue = 0)
        {
            return GetFloat($"{group}/{key}", defaultValue);
        }
        
        public static PropertyTypeGroup GetPropertyTypeGroup(string key)
        {
            if (_propertyTypeGroupCache.ContainsKey(key) == false)
            {
                var configValue = GetString(key);
                if (configValue == null) return 0;

                if (int.TryParse(configValue, out int intValue))
                {
                    _propertyTypeGroupCache.Add(key, (PropertyTypeGroup)GetInt(key, intValue));   
                }
                else
                {
                    _propertyTypeGroupCache.Add(key, PropertyTypeGroup.Parse<PropertyTypeGroup>(configValue));
                }
            }
            
            return _propertyTypeGroupCache[key];
        }
        
        public static PropertyTypeGroup GetPropertyTypeGroup(string group, string key)
        {
            return GetPropertyTypeGroup($"{group}/{key}");
        }
    }
}
