using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Aloha.Coconut;
using Aloha.Coconut.Player;
using Alohacorp.Durian.Model;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Object = System.Object;

namespace Aloha.Durian
{
    public class PlayerDataManager : ISaveDataSaver, IServerNicknameSetter
    {
        public string Nickname { get; set; }
        
        private PrivatePlayerDto _privatePlayerDto;
        private JObject _lastSyncedData;
        
        private readonly string _device;
        private string _country;

        private bool IsNotUploaded
        {
            get => PlayerPrefs.GetInt("player_data_not_uploaded", 0) == 1;
            set => PlayerPrefs.SetInt("player_data_not_uploaded", value ? 1 : 0);
        }
        private readonly FileSaveDataSaver _backupSaver;

        public PlayerDataManager()
        {
            _backupSaver = new FileSaveDataSaver(true, "save_rm_bkp.ccn");
            _device = SystemInfo.deviceModel;
            CountryChecker.GetCountry().ContinueWith(result => { _country = result; });
        }

        public async Task<JObject> LoadAsync()
        {
            if (IsNotUploaded)
            {
                return await _backupSaver.LoadAsync();
            }

            var playerApi = await DurianApis.PlayerApi();
            _privatePlayerDto = await RequestHandler.Request(playerApi.GetPlayersAsync(), r => r.Data);
            _lastSyncedData = new JObject();
            foreach (var o in _privatePlayerDto.GameData)
            {
                _lastSyncedData.Add(o.Key, (JObject)o.Value);
            }

            return _lastSyncedData;
        }

        public void Save(JObject jObject)
        {
            RequestUpload(jObject).Forget();
        }

        private bool IsNotChanged(string key, JToken jToken)
        {
            return _lastSyncedData != null && _lastSyncedData.ContainsKey(key) &&
                   JToken.DeepEquals(_lastSyncedData[key], jToken);
        }

        private async UniTaskVoid RequestUpload(JObject jObject)
        {
            var saveData = new Dictionary<string, Object>();
            foreach (var (key, value) in jObject)
            {
                saveData.Add(key, value);
            }

            try
            {
                _backupSaver.Save(jObject);
                IsNotUploaded = true;

                var playerApi = await DurianApis.PlayerApi();
                var req = new PlayerUpdateReqDto(
                    name: Nickname, 
                    gameData: saveData, 
                    device: _device,
                    clientVersion: Application.version,
                    country: _country);
                await RequestHandler.Request(playerApi.EditPlayersWithHttpInfoAsync(req), r => r.Data);
                _lastSyncedData = new JObject();
                foreach (var o in saveData)
                {
                    _lastSyncedData.Add(o.Key, (JObject)o.Value);
                }

                IsNotUploaded = false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception when calling Api: {e}");
                Debug.LogError(e.StackTrace);
            }
        }

        public void Delete()
        {
            var req = new PlayerUpdateReqDto(gameData: new Dictionary<string, Object>());
            DurianApis.PlayerApi().ContinueWith(async playerApi =>
            {
                await RequestHandler.Request(playerApi.EditPlayersWithHttpInfoAsync(req), r => r.Data);
            }).Forget();
        }
        
        private static class CountryChecker
        {
            [Serializable]
            public class IpApiData
            {
                public string country_name;
            }

            public static async UniTask<string> GetCountry()
            {
                string ip = (new WebClient().DownloadString("https://icanhazip.com/")).Trim();
                string uri = $"https://ipapi.co/{ip}/json/";

                UnityWebRequest webRequest = UnityWebRequest.Get(uri);
                await webRequest.SendWebRequest();

                IpApiData ipApiData = JsonUtility.FromJson<IpApiData>(webRequest.downloadHandler.text);
                return ipApiData.country_name;
            }
        }

    }
}