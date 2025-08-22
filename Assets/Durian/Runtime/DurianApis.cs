using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aloha.Coconut;
using Alohacorp.Durian.Api;
using Alohacorp.Durian.Client;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Aloha.Durian
{
    // AccessToken의 변경 등 Api 전체에 적용되어야하는 설정을 관리하기 위한 클래스
    internal static class DurianApis
    {
        private static Func<Task<string>> _tokenGetter;
        private static Dictionary<Type, object> _apis = new Dictionary<Type, object>();
        private static ApiClient _apiClient;
        private static DurianConfig _config;

        public static void Initialize()
        {
            _config = CoconutConfig.Get<DurianConfig>();
            ((GlobalConfiguration)GlobalConfiguration.Instance).BasePath = _config.ServerUrl;
            _apiClient = new ApiClient(_config.ServerUrl);
        }

        // Authentication이 되어 TokenGetter가 null이 아닌 값을 리턴하는 게 보장될 때 Set해야 함
        public static void SetTokenGetter(Func<Task<string>> tokenGetter)
        {
            _tokenGetter = tokenGetter;
        }

        private static async UniTask<string> GetTokenAsync()
        {
            if (_tokenGetter == null)
            {
                Debug.LogWarning("Token getter is not set. Waiting for it to be set.");
                while (_tokenGetter == null)
                {
                    await UniTask.Yield();
                }
            }

            return await _tokenGetter.Invoke();
        }

        private static T GetApi<T>(Func<Configuration, T> apiFactory) where T : IApiAccessor
        {
            if (!_apis.TryGetValue(typeof(T), out var api))
            {
                var configuration = new Configuration
                {
                    BasePath = _config.ServerUrl,
                };
                api = apiFactory(configuration);
                _apis[typeof(T)] = api;
            }

            return (T)api;
        }

        public static RootApi RootApi() =>
            GetApi(config => new RootApi(_apiClient, _apiClient, config));

        public static AnnouncementApi AnnouncementApi() =>
            GetApi(config => new AnnouncementApi(_apiClient, _apiClient, config));

        public static AppVersionApi AppVersionApi() =>
            GetApi(config => new AppVersionApi(_apiClient, _apiClient, config));

        private static async UniTask<T> GetApiWithToken<T>(Func<Configuration, T> apiFactory) where T : IApiAccessor
        {
            string token = await GetTokenAsync();
            if (!_apis.TryGetValue(typeof(T), out var api) || ((T)api).Configuration.AccessToken != token)
            {
                var configuration = new Configuration
                {
                    BasePath = _config.ServerUrl,
                    AccessToken = token
                };
                api = apiFactory(configuration);
                _apis[typeof(T)] = api;
            }
            
            return (T)api;
        }

        public static async UniTask<PlayerApi> PlayerApi() =>
            await GetApiWithToken(config => new PlayerApi(_apiClient, _apiClient, config));

        public static async UniTask<SessionApi> SessionApi() =>
            await GetApiWithToken(config => new SessionApi(_apiClient, _apiClient, config));

        public static async UniTask<MailApi> MailApi() =>
            await GetApiWithToken(config => new MailApi(_apiClient, _apiClient, config));

        public static async UniTask<NotificationApi> NotificationApi() =>
            await GetApiWithToken(config => new NotificationApi(_apiClient, _apiClient, config));

        public static async UniTask<LeagueApi> LeagueApi() =>
            await GetApiWithToken(config => new LeagueApi(_apiClient, _apiClient, config));

        public static async UniTask<LeaderboardApi> LeaderboardApi() =>
            await GetApiWithToken(config => new LeaderboardApi(_apiClient, _apiClient, config));
        
        public static async UniTask<CouponApi> CouponApi() =>
            await GetApiWithToken(config => new CouponApi(_apiClient, _apiClient, config));
        
        public static async UniTask<IAPApi> IAPApi() =>
            await GetApiWithToken(config => new IAPApi(_apiClient, _apiClient, config));
    }
}
