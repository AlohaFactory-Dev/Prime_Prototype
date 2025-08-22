using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Aloha.Coconut
{
    public class SaveDataManager
    {
        private static JsonSerializer _jsonSerializer = new();

        public static void AddJsonConverter(JsonConverter jsonConverter)
        {
            _jsonSerializer.Converters.Add(jsonConverter);
        }

        [RuntimeInitializeOnLoadMethod]
        public static void RuntimeInitializeOnLoad()
        {
            _jsonSerializer = new();
        }
        
        private readonly Dictionary<string, object> _saveDatas = new ();
        private Dictionary<Type, bool> _isLocalSaveType = new();
        
        private FileSaveDataSaver _localSaveDataSaver;
        private JObject _localSaveDataJObject;
        
        private ISaveDataSaver _remoteSaveDataSaver;
        private JObject _remoteSaveDataJObject;

        private bool _isLocked;
        
        public SaveDataManager()
        {
            AddJsonConverter(new PropertyJsonConverter());
            _localSaveDataSaver = new FileSaveDataSaver();

            var loadTask = Task.Run(() => _localSaveDataSaver.LoadAsync());
            loadTask.Wait();
            _localSaveDataJObject = loadTask.Result;
        }

        public async Task LinkAsync(ISaveDataSaver saveDataSaver)
        {
            _remoteSaveDataSaver = saveDataSaver;
            _remoteSaveDataJObject = await _remoteSaveDataSaver.LoadAsync();
        }

        // 서버 구축이 되기 전 사용, 서버에 저장할 데이터를 임시로 로컬에 저장
        public void LinkFileDataSaver(bool useEncryption = true)
        {
            var fileSaveDataSaver = new FileSaveDataSaver(useEncryption, "save_rm.ccn");
            Task.Run(() => LinkAsync(fileSaveDataSaver)).Wait();
        }

        public void Save()
        {
            if (_isLocked) return;
            
            foreach (var saveData in _saveDatas)
            {
                if (IsLocalSave(saveData.Value))
                {
                    _localSaveDataJObject[saveData.Key] = JToken.FromObject(saveData.Value, _jsonSerializer);
                }
                else
                {
                    _remoteSaveDataJObject[saveData.Key] = JToken.FromObject(saveData.Value, _jsonSerializer);   
                }
            }
            
            _localSaveDataSaver.Save(_localSaveDataJObject);
            _remoteSaveDataSaver.Save(_remoteSaveDataJObject);
        }

        private bool IsLocalSave(object saveObject)
        {
            var type = saveObject.GetType();
            if (_isLocalSaveType.TryGetValue(type, out var isLocalSave)) return isLocalSave;

            isLocalSave = type.GetCustomAttributes(typeof(LocalSaveAttribute), true).Length > 0;
            _isLocalSaveType[type] = isLocalSave;
            return isLocalSave;
        }
        
        public T Get<T>(string key)
        {
            if (_saveDatas.TryGetValue(key, out var data)) return (T)data;

            if(_localSaveDataJObject.TryGetValue(key, out var valueLocal))
            {
                _saveDatas[key] = valueLocal.ToObject<T>(_jsonSerializer);
                return (T)_saveDatas[key];
            }
            
            if(_remoteSaveDataJObject.TryGetValue(key, out var valueRemote))
            {
                _saveDatas[key] = valueRemote.ToObject<T>(_jsonSerializer);
                return (T)_saveDatas[key];
            }

            _saveDatas[key] = Activator.CreateInstance<T>();
            return (T)_saveDatas[key];   
        }

        public void Delete(string key)
        {
            if(_saveDatas.ContainsKey(key)) _saveDatas.Remove(key);
            if(_localSaveDataJObject.ContainsKey(key)) _localSaveDataJObject.Remove(key);
            if(_remoteSaveDataJObject.ContainsKey(key)) _remoteSaveDataJObject.Remove(key);
            Save();
        }

        public void DeleteAll()
        {
            _saveDatas.Clear();
            _localSaveDataJObject = new JObject();
            _remoteSaveDataJObject = new JObject();
            Save();
        }
        
        public void Reset()
        {
            _saveDatas.Clear();
            
            _localSaveDataJObject = new JObject();
            _localSaveDataSaver.Delete();
            
            _remoteSaveDataJObject = new JObject();
            _remoteSaveDataSaver.Delete();
        }

        public void Lock(bool isLocked)
        {
            _isLocked = isLocked;
        }

        public static void ClearLocalData()
        {
            (new FileSaveDataSaver()).Delete();
        }
    }
}