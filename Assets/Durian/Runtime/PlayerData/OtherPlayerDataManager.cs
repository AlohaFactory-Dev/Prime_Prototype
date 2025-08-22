using System;
using System.Collections.Generic;
using Alohacorp.Durian.Model;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aloha.Durian
{
    public class OtherPlayerDataManager
    {
        private readonly Dictionary<string, Dictionary<string, JObject>> _playerDataCache = new Dictionary<string, Dictionary<string, JObject>>();

        public async UniTask<T> Get<T>(string playerUID, string dataKey)
        {
            await Fetch(playerUID, dataKey);
            return JsonConvert.DeserializeObject<T>(_playerDataCache[playerUID][dataKey].ToString());
        }

        public async UniTask Fetch(string playerUID, params string[] dataKeys)
        {
            if (!_playerDataCache.ContainsKey(playerUID))
            {
                _playerDataCache.Add(playerUID, new Dictionary<string, JObject>());
            }

            List<QueryGameDataDto> queryList = new List<QueryGameDataDto>();
            foreach (string dataKey in dataKeys)
            {
                if (_playerDataCache[playerUID].ContainsKey(dataKey)) continue;
                queryList.Add(new QueryGameDataDto($"[\"{dataKey}\"]"));
            }

            if (queryList.Count == 0) return;

            var playerApi = await DurianApis.PlayerApi();
            PlayerQueryGameDataReqDto req = new PlayerQueryGameDataReqDto(queryList);
            List<JObject> jObjects = await RequestHandler.Request(playerApi.QueryPlayerGameDataAsync(playerUID, req),
                rootDto =>
                {
                    if (rootDto.Data.Result.Count == 0 || rootDto.Data.Result[0] == null)
                    {
                        throw new Exception("No data found");
                    }

                    List<JObject> result = new List<JObject>();
                    foreach (var data in rootDto.Data.Result)
                    {
                        result.Add((JObject)data);
                    }

                    return result;
                });

            for (int i = 0; i < dataKeys.Length; i++)
            {
                _playerDataCache[playerUID].Add(dataKeys[i], jObjects[i]);
            }
        }

        public void ExpireCache(string playerUID)
        {
            if (_playerDataCache.ContainsKey(playerUID))
            {
                _playerDataCache.Remove(playerUID);
            }
        }

        public void ExpireCache(string playerUID, string dataKey)
        {
            if (_playerDataCache.ContainsKey(playerUID) && _playerDataCache[playerUID].ContainsKey(dataKey))
            {
                _playerDataCache[playerUID].Remove(dataKey);
            }
        }
    }
}
