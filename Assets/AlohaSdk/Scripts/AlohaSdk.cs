using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Analytics;
using Debug = UnityEngine.Debug;

namespace Aloha.Sdk
{
    public partial class AlohaSdk : MonoBehaviour
    {
        #region static
        public static string CUID { get; private set; }
        public static bool IsInitialized { get; private set; }
        public static event Action OnInitialized;

        internal static AlohaSdkConfigs SdkConfigs { get; private set; }

        private static AlohaSdk _instance;
        private static AlohaFirebaseEvent _alohaFirebaseEvent;
        private static AlohaRemoteConfig _alohaRemoteConfig;
        private static AlohaAds _alohaAds;
        private static AlohaAdjust _alohaAdjust;
        private static AlohaOfflineChecker _offlineChecker;
        private static AlohaTimeSpentLogger _timeSpentLogger;
        // Flamingo SDK
        private static IFlamingoWrapper _flamingo;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitializeOnLoad()
        {
            _instance = null;
            _alohaFirebaseEvent = new AlohaFirebaseEvent();
            _alohaRemoteConfig = new AlohaRemoteConfig();
            _alohaAds = new AlohaAds();
            _alohaAdjust = new AlohaAdjust();
            _offlineChecker = new AlohaOfflineChecker();
            _timeSpentLogger = new AlohaTimeSpentLogger();
#if UNITY_ANDROID && !UNITY_EDITOR
            _flamingo = new FlamingoWrapperAndroid();
#elif (UNITY_IOS || UNITY_VISIONOS) && !UNITY_EDITOR
            _flamingo = new FlamingoWrapperiOS();
#else
            _flamingo = new FlamingoWrapperDummy();
#endif
            
            OnInitialized = null;
            IsInitialized = false;
            CUID = "";
        }

        private static void CheckIsInitialized()
        {
            if (!IsInitialized) throw new AlohaSdkNotInitializedException();
        }

        internal static void AddSdkLog(string logStr)
        {
            Debug.Log($"AlohaSdk Log :: {logStr}");
            if (_instance.showSdkLog && _instance._textLog != null)
                _instance._textLog.text += "\n " + logStr;
        }

        internal static void AddQaLog(string logStr)
        {
            Debug.Log($"AlohaSdk QaLog :: {logStr}");
            if (_instance.showQaLog && _instance._textLog != null)
                _instance._textLog.text += "\n " + logStr;
        }

        internal static void InvokeAfter(Action action, float delay)
        {
            _instance.StartCoroutine(InvokeAfterCoroutine(action, delay));
        }

        private static IEnumerator InvokeAfterCoroutine(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action();
        }

        internal static void StartCoroutineByInstance(IEnumerator coroutine)
        {
            _instance.StartCoroutine(coroutine);
        }

        internal static string GetStoreName()
        {
            string installSource = Application.installerName;
            switch (installSource)
            {
                //google play
                case "com.android.vending":
                    return "google_play";

                //galaxy store
                case "com.sec.android.app.samsungapps":
                    return "galaxy_store";
                
                //one store
                case "com.skt.skaf.A000Z00040":
                    return "one_store";

                //apple
                case "com.apple.AppStore":
                    return "apple_app_store";
            }

            return "unknown";
        }
        
        public class AlohaSdkNotInitializedException : Exception {}
        
        #endregion

        [SerializeField] private bool showSdkLog = true;
        [SerializeField] private bool showQaLog = true;
        [SerializeField] private AlohaPlayTimer playTimer;

        // 로그를 표시할 텍스트. (실제 게임에서는 사용 안함)
        private Text _textLog;

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        async void Start()
        {
            SdkConfigs = Resources.Load<AlohaSdkConfigs>("AlohaSdkConfigs");
            InitLog();

            await _offlineChecker.Run(SdkConfigs);

            if (!await CheckFirebaseDependencies()) return;

            Context.ShowBannerAdsOnInitialized = SdkConfigs.showBannerAdsOnInitialized;
            Context.Session++;

            _alohaRemoteConfig.Initialize();

            if (SdkConfigs.autoCUID)
            {
                if (!PlayerPrefs.HasKey("alohaSdk.cuid"))
                {
                    PlayerPrefs.SetString("alohaSdk.cuid", Guid.NewGuid().ToString());
                }
                SetCUID(PlayerPrefs.GetString("alohaSdk.cuid"));
            }
            else
            {
                while (string.IsNullOrEmpty(CUID)) await Task.Delay(10);
            }
            
            while (!_alohaRemoteConfig.IsFetchCompleted) await Task.Delay(10);

            // Adjust Initialize가 AlohaAds의 MaxSDK Initialize(ATT 동의 팝업)보다 먼저 실행되어야 함
            _alohaAdjust.Initialize();
            _alohaAds.Initialize(SdkConfigs.adUnitData);

            if (RemoteConfig.Predefined.SHOW_PRIVACY_POLICY_POPUP_ON_START)
            {
                await PrivacyPolicy.ShowPopup();   
            }

            _alohaFirebaseEvent.Initialize();
            playTimer.Initialize();
            _timeSpentLogger.Initialize();

            IsInitialized = true;
            InitTutorialLog();

            // 개발사가 지정한 초기화 완료 콜백도 실행
            OnInitialized?.Invoke();
            _flamingo.Setup(gameObject.name, SdkConfigs.flamingoAccessKey, Application.version);
        }

        private async Task<bool> CheckFirebaseDependencies()
        {
            DependencyStatus dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus != DependencyStatus.Available)
            {
                Debug.LogError(String.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                AddSdkLog("Failed to initialize Firebase");
                return false;
            }

            AddSdkLog("Firebase available");
            return true;
        }

        public static void SetCUID(string cuid)
        {
            CUID = cuid;
            FirebaseAnalytics.SetUserProperty("cuid", CUID);
        }
        
        private void InitLog()
        {
            _textLog = transform.Find("LogCanvas").transform.Find("TextLog").GetComponent<Text>();
            transform.Find("LogCanvas").gameObject.SetActive(showSdkLog || showQaLog);
        }

        private void InitTutorialLog()
        {
            if (Context.Session == 1) Event.LogTutorialEnd(0, "init_tutorial");
        }
    }
}
