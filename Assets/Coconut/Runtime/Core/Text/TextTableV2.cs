using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Aloha.Coconut
{
    public static class TextTableV2
    {
        public static event Action<SystemLanguage> OnLanguageChanged; 
        
        public static SystemLanguage Language { get; private set; }
        public static List<SystemLanguage> SupportedLanguages => _config.SupportedLanguages;
        
        private static TextTableConfig _config;
        
        private static Dictionary<string, Dictionary<string, Dictionary<SystemLanguage, string>>> _textTable 
            = new Dictionary<string, Dictionary<string, Dictionary<SystemLanguage, string>>>();

        private static bool _isInitialized;
        private static List<string> _keysCache;
        
        #if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        #endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            OnLanguageChanged = null;
            _config = CoconutConfig.Get<TextTableConfig>();
            if (PlayerPrefs.HasKey("coconut.text.language"))
            {
                Language = (SystemLanguage) PlayerPrefs.GetInt("coconut.text.language");
            }
            else if(SupportedLanguages.Contains(Application.systemLanguage))
            {
                Language = Application.systemLanguage;
            }
            else
            {
                Language = SupportedLanguages[0];
            }
            
            RefreshTable();

            _isInitialized = true;
        }

        public static void RefreshTable()
        {
            _textTable.Clear();
            _keysCache = null;

            var tableNames = _config.tableNames;

            foreach (var tableName in tableNames)
            {
                var table = TableManager.Get(tableName);
                foreach (var row in table)
                {
                    var group = row["group"].ToString();
                    var key = row["key"].ToString();
                    var texts = new Dictionary<SystemLanguage, string>();
                    foreach (var language in SupportedLanguages)
                    {
                        texts.Add(language, row[language.ToString()].ToString());
                    }
                    AddTextTableEntry(group, key, texts);
                }
            }
        }

        public static void ChangeLanguage(SystemLanguage language)
        {
            if(!_isInitialized) Initialize();
            
            Assert.IsTrue(_config.SupportedLanguages.Contains(language), $"Unsupported language: {language}");
            PlayerPrefs.SetInt("coconut.text.language", (int) language);
            Language = language;
            OnLanguageChanged?.Invoke(language);
        }

        public static string Get(string key, params Param[] @params)
        {
            return Get(key, Language, @params);
        }
        
        public static string Get(string key, SystemLanguage language, params Param[] @params)
        {
            var dividerIndex = key.IndexOf('/');
            return Get(key.Substring(0, dividerIndex), key.Substring(dividerIndex + 1), language, @params);
        }

        public static string Get(string group, string key, params Param[] @params)
        {
            return Get(group, key, Language, @params);
        }

        public static string Get(string group, string key, SystemLanguage language, params Param[] @params)
        {
            if(!_isInitialized) Initialize();
            
            if (!Exists(group, key))
            {
                return $"[MISSING TEXT]_{group}/{key}";
            }
            
            var text = _textTable[group][key][language];
            var paramOpenIndex = text.IndexOf("{[", StringComparison.InvariantCulture);
            while (paramOpenIndex >= 0)
            {
                var paramCloseIndex = text.IndexOf("]}", paramOpenIndex, StringComparison.InvariantCulture);
                if(paramCloseIndex < 0) break;
                
                var paramKey = text.Substring(paramOpenIndex + 2, paramCloseIndex - paramOpenIndex - 2);
                if (paramKey.StartsWith('$'))
                {
                    var paramValue = Get(paramKey.Substring(1), language);
                    text = text.Substring(0, paramOpenIndex) + paramValue + text.Substring(paramCloseIndex + 2);
                }
                else
                {
                    var param = Array.Find(@params, p => p.key == paramKey);
                    if (param.IsValid)
                    {
                        text = text.Substring(0, paramOpenIndex) + param.value + text.Substring(paramCloseIndex + 2);
                    }
                }
                
                paramOpenIndex = text.IndexOf("{[", paramOpenIndex + 1, StringComparison.InvariantCulture);
            }
            
            return text;
        }

        public static void AddTextTableEntry(string group, string key, Dictionary<SystemLanguage, string> entry)
        {
            if (!_textTable.ContainsKey(group))
            {
                _textTable.Add(group, new Dictionary<string, Dictionary<SystemLanguage, string>>());
            }

            _textTable[group].Add(key, entry);
        }

        public static (TMP_FontAsset, Material) GetFontAndMaterial(FontType fontType)
        {
            return GetFontAndMaterial(fontType, Language);
        }

        public static (TMP_FontAsset, Material) GetFontAndMaterial(FontType fontType, SystemLanguage systemLanguage)
        {
            if(!_isInitialized) Initialize();
            
            return _config.GetFontInfo(systemLanguage, fontType);   
        }

        public static List<string> GetKeys()
        {
            if (_keysCache == null)
            {
                _keysCache = new List<string>();
                foreach (var textTableKey in _textTable.Keys)
                {
                    foreach(var key in _textTable[textTableKey].Keys)
                    {
                        _keysCache.Add($"{textTableKey}/{key}");
                    }
                }
            }
            return _keysCache;
        }

        public static bool Exists(string key)
        {
            var dividerIndex = key.IndexOf('/');
            return Exists(key.Substring(0, dividerIndex), key.Substring(dividerIndex + 1));
        }
        
        public static bool Exists(string group, string key)
        {
            if(!_isInitialized) Initialize();
            
            return _textTable.ContainsKey(group) && _textTable[group].ContainsKey(key);
        }
        
        [Serializable]
        public struct Param
        {
            public bool IsValid => !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value);
            
            public string key;
            public string value;

            public Param(string key, string value)
            {
                this.key = key;
                this.value = value;
            }

            public Param(string key, int value)
            {
                this.key = key;
                this.value = value.ToString();
            }
        }
    }
}
