using System;
using System.Collections.Generic;

namespace Aloha.Coconut
{
    public class SimpleValues
    {
        private SaveData _saveData;

        public SimpleValues(SaveDataManager saveDataManager)
        {
            _saveData = saveDataManager.Get<SaveData>(nameof(SimpleValues));
        }
        
        public int GetInt(string key, int defaultValue = 0)
        {
            if (!_saveData.ints.ContainsKey(key))
                return defaultValue;
            return _saveData.ints[key];
        }
        
        public void SetInt(string key, int value)
        {
            _saveData.ints[key] = value;
        }
        
        public float GetFloat(string key, float defaultValue = 0)
        {
            if (!_saveData.floats.ContainsKey(key))
                return defaultValue;
            return _saveData.floats[key];
        }
        
        public void SetFloat(string key, float value)
        {
            _saveData.floats[key] = value;
        }
        
        public bool GetBool(string key, bool defaultValue = false)
        {
            if (!_saveData.bools.ContainsKey(key))
                return defaultValue;
            return _saveData.bools[key];
        }
        
        public void SetBool(string key, bool value)
        {
            _saveData.bools[key] = value;
        }
        
        public string GetString(string key, string defaultValue = "")
        {
            if (!_saveData.strings.ContainsKey(key))
                return defaultValue;
            return _saveData.strings[key];
        }
        
        public void SetString(string key, string value)
        {
            _saveData.strings[key] = value;
        }

        public DateTime GetDateTime(string key, DateTime defaultValue)
        {
            if (!_saveData.times.ContainsKey(key))
                return defaultValue;
            return DateTime.FromBinary(_saveData.times[key]);
        }
        
        public void SetDateTime(string key, DateTime value)
        {
            _saveData.times[key] = value.ToBinary();
        }
        
        public bool HaveDateTime(string key)
        {
            return _saveData.times.ContainsKey(key);
        }
        
        public void DeleteInt(string key)
        {
            if (_saveData.ints.ContainsKey(key))
                _saveData.ints.Remove(key);
        }
        
        public void DeleteFloat(string key)
        {
            if (_saveData.floats.ContainsKey(key))
                _saveData.floats.Remove(key);
        }
        
        public void DeleteBool(string key)
        {
            if (_saveData.bools.ContainsKey(key))
                _saveData.bools.Remove(key);
        }
        
        public void DeleteString(string key)
        {
            if (_saveData.strings.ContainsKey(key))
                _saveData.strings.Remove(key);
        }
        
        public void DeleteDateTime(string key)
        {
            if (_saveData.times.ContainsKey(key))
                _saveData.times.Remove(key);
        }

        private class SaveData
        {
            public Dictionary<string, int> ints = new ();
            public Dictionary<string, float> floats = new ();
            public Dictionary<string, bool> bools = new ();
            public Dictionary<string, string> strings = new ();
            public Dictionary<string, long> times = new ();
        }
    }
}
