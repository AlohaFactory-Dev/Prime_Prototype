using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Aloha.Coconut
{
    // Addressable CSV만 처리 가능
    public static class TableManager
    {
        private static string _rootPath;
        private static Dictionary<string, object> _overridenTables = new Dictionary<string, object>();
        public const int MAGIC_NUMBER = -99999;

        public static bool IsMagicNumber(object value)
        {
            if (value is int intValue)
            {
                return intValue == MAGIC_NUMBER;
            }

            if (value is float floatValue)
            {
                return Mathf.Approximately(floatValue, MAGIC_NUMBER);
            }

            if (value is string stringValue)
            {
                return int.TryParse(stringValue, out var parsedValue) && parsedValue == MAGIC_NUMBER;
            }

            return false;
        }

        [RuntimeInitializeOnLoadMethod]
        public static void RuntimeInitializeOnLoad()
        {
            _rootPath = null;
            _overridenTables.Clear();
        }

        public static List<T> Get<T>(string tableName)
        {
            if (string.IsNullOrEmpty(_rootPath))
            {
                _rootPath = CoconutConfig.Get<TableConfig>().rootFolderAddress;
            }

            if (_overridenTables.TryGetValue(tableName, out var table))
            {
                return (List<T>)table;
            }

            var path = $"{_rootPath}/{tableName}.csv";
            var textAsset = Addressables.LoadAssetAsync<TextAsset>(path).WaitForCompletion();
            if (textAsset == null)
            {
                Debug.LogError($"Failed to load table at address {path}");
            }

            return CSVReader.ReadTextAsset<T>(textAsset);
        }

        public static async UniTask<List<T>> GetAsync<T>(string tableName)
        {
            if (string.IsNullOrEmpty(_rootPath))
            {
                _rootPath = CoconutConfig.Get<TableConfig>().rootFolderAddress;
            }

            if (_overridenTables.TryGetValue(tableName, out var table))
            {
                return (List<T>)table;
            }

            var path = $"{_rootPath}/{tableName}.csv";
            var handle = Addressables.LoadAssetAsync<TextAsset>(path);

            try
            {
                var textAsset = await handle.Task;
                if (textAsset == null)
                {
                    Debug.LogError($"Failed to load table at address {path}");
                    return null;
                }

                // 데이터를 반환하기 전에 캐싱


                return CSVReader.ReadTextAsset<T>(textAsset);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading table {tableName}: {e.Message}");
                return null;
            }
            finally
            {
                Addressables.Release(handle);
            }
        }
#if UNITY_EDITOR
        public static void Set<T>(string tableName, List<T> dataList)
        {
            var path = $"{CoconutConfig.Get<TableConfig>().rootFolderPath}/{tableName}.csv";
            CSVWriter.Write(path, dataList);
        }
#endif

        public static void Override<T>(string tableName, List<T> table)
        {
            _overridenTables[tableName] = table;
        }

        public static List<Dictionary<string, object>> Get(string tableName)
        {
            if (string.IsNullOrEmpty(_rootPath))
            {
                _rootPath = CoconutConfig.Get<TableConfig>().rootFolderAddress;
            }

            if (_overridenTables.TryGetValue(tableName, out var table))
            {
                return (List<Dictionary<string, object>>)table;
            }

            var path = $"{_rootPath}/{tableName}.csv";
            var textAsset = Addressables.LoadAssetAsync<TextAsset>(path).WaitForCompletion();
            if (textAsset == null)
            {
                Debug.LogError($"Failed to load table at address {path}");
                return null;
            }

            return CSVReader.ReadTextAsset(textAsset);
        }
    }
}