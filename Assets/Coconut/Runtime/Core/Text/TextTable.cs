using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aloha.Coconut
{
    [Obsolete("Use TextTableV2 instead")]
    public static class TextTable
    {
        public struct Param
        {
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

        private static bool _isGlobalParamsManagerAdded;
        
        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            _isGlobalParamsManagerAdded = false;
        }

        public static bool Exists(string key, string language)
        {
            return I2.Loc.LocalizationManager.GetTranslation(key, overrideLanguage: language) != null;
        }

        public static string Get(string key, params Param[] @params)
        {
            if (!_isGlobalParamsManagerAdded)
            {
                I2.Loc.LocalizationManager.ParamManagers.Add(new GlobalParamsManager());
                _isGlobalParamsManagerAdded = true;
            }
            
            var result = I2.Loc.LocalizationManager.GetTranslation(key, applyParameters: true);
            if (result == null) return $"[MISSING TEXT]_{key}";

            if (@params.Length > 0)
            {
                var parameterDictionary = new Dictionary<string, object>();
                foreach (Param param in @params)
                {
                    parameterDictionary.Add(param.key, param.value);
                }

                I2.Loc.LocalizationManager.ApplyLocalizationParams(ref result, parameterDictionary);
            }

            return result;
        }
        
        private class GlobalParamsManager : I2.Loc.ILocalizationParamsManager
        {
            public string GetParameterValue(string Param)
            {
                if (Param.StartsWith('$'))
                {
                    return TextTable.Get(Param.Substring(1));
                }
                
                return null;
            }
        }
    }
}