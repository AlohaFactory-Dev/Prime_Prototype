using System;
using Aloha.Coconut;
using Alohacorp.Durian.Model;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Aloha.Durian
{
    public class AppVersionChecker : ITickable
    {
        private readonly DurianConfig _durianConfig;

        private float _timer;
        private bool _isChecking;

        public AppVersionChecker(DurianConfig durianConfig)
        {
            _durianConfig = durianConfig;
            _timer = float.MinValue;
        }

        internal async UniTask InitialCheck()
        {
            await CheckAppVersion();
        }

        public async UniTask CheckAppVersion()
        {
            _isChecking = true;

            var forceUpdate = (await GetLatestAppVersionAsync()).ForceUpdate;
            if (forceUpdate != null && forceUpdate.IsForceUpdate)
            {
                var forceUpdateVersion = new Version(forceUpdate.VarVersion);
                var myVersion = new Version(Application.version);

                if (myVersion < forceUpdateVersion)
                {
                    await SystemUI.ShowDialogue("New Version", "Please update to the latest version.", "OK");

#if UNITY_EDITOR
                    Application.OpenURL($"https://play.google.com/store/apps/details?id={Application.identifier}");
#elif UNITY_ANDROID
                    Application.OpenURL($"market://details?id={Application.identifier}");
#elif UNITY_IOS
                    Application.OpenURL($"itms-apps://itunes.apple.com/app/{Application.identifier}");
#endif

#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    throw new Exception("Force update required.");
                }
            }

            _timer = 0;
            _isChecking = false;
        }

        public async UniTask<AppVersionLatestRespDto> GetLatestAppVersionAsync()
        {
            var target = Application.isEditor ? "android"
                : Application.platform == RuntimePlatform.Android ? "android" : "ios";
            return await RequestHandler.Request(DurianApis.AppVersionApi().GetLatestAppVersionAsync(target), resp => resp.Data, false);
        }

        public void Tick()
        {
            if (_isChecking) return;

            _timer += Time.deltaTime;
            if (_timer >= _durianConfig.versionCheckInterval)
            {
                _timer = 0;
                CheckAppVersion().Forget();
            }
        }
    }
}