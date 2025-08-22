using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Aloha.Sdk
{
    internal class AlohaOfflineChecker
    {
        private const int PING_TIMEOUT = 5;
        private float _pingInterval;
        
        // 특정 사이트에 대한 접근이 안 될 때를 대비해서 여러 사이트를 사용
        private string[] _echoServers = new string[]
        {
            "https://www.google.com/",
            "https://www.bing.com/",
            "https://www.yandex.com/",
        };
        
        private int _pingIndex = -1;
        private int _offlineCounter = 0;

        private GameObject _dim;

        public async Task Run(AlohaSdkConfigs configs)
        {
            _pingInterval = configs.offlineCheckInterval;
            
            do
            {
                _pingIndex = (_pingIndex + 1) % _echoServers.Length;
                using (var request = UnityWebRequest.Head(_echoServers[_pingIndex]))
                {
                    request.timeout = PING_TIMEOUT;
                    request.SendWebRequest();
                    while(!request.isDone) await Task.Delay(100);
                    AlohaSdk.Context.IsOffline = request.result != UnityWebRequest.Result.Success;
                }

                if (AlohaSdk.Context.IsOffline)
                {
                    await AlohaSimplePopup.ShowFromResourceTask("AlohaCheckInternetPopup");
                    await Task.Delay(100);
                }
            } while (AlohaSdk.Context.IsOffline);
            
            AlohaSdk.StartCoroutineByInstance(RepeatPingCoroutine());
        }

        private IEnumerator RepeatPingCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_pingInterval);
                yield return PingCheckCoroutine();
            }
        }
        
        private IEnumerator PingCheckCoroutine()
        {
            for (int i = 0; i < 3; ++i)
            {
                _pingIndex = (_pingIndex + 1) % _echoServers.Length;
                using (var request = UnityWebRequest.Head(_echoServers[_pingIndex]))
                {
                    request.timeout = PING_TIMEOUT;
                    yield return request.SendWebRequest();
                    AlohaSdk.Context.IsOffline = request.result != UnityWebRequest.Result.Success;
                }
            
                Debug.Log($"AlohaOfflineChecker :: IsOffline = {AlohaSdk.Context.IsOffline}");
                if (!AlohaSdk.Context.IsOffline) break;
            }

            if (AlohaSdk.Context.IsOffline)
            {
                if (_dim == null) _dim = Object.Instantiate(Resources.Load<GameObject>("AlohaDim"));
                _dim.gameObject.SetActive(true);
            
                _offlineCounter++;
                if (_offlineCounter < 5)
                {
                    yield return AlohaSimplePopup.ShowFromResourceCoroutine("AlohaCheckInternetPopup");
                    AlohaSdk.StartCoroutineByInstance(PingCheckCoroutine());
                }
                else
                {
                    yield return AlohaSimplePopup.ShowFromResourceCoroutine("AlohaRestartPleasePopup");
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }
            }
            else
            {
                _offlineCounter = 0;
                if(_dim != null) _dim.gameObject.SetActive(false);
            }
        }
    }
}