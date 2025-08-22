using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Aloha.Sdk
{
    internal class AlohaAds
    {
        public event Action OnInitialized;
        
        public bool IsInitialized { get; private set; }
        public bool IsBannerLoaded { get; private set; }
        public event Action OnBannerLoaded;
        public Rect BannerLayout { get; private set; }
        
        // 최근 IS 광고 노출 타임 스탬프.
        private int _recentTimeStampIsAds = 0;

        // is 광고를 노출 하려고 시도한 횟수.
        private int _tryCountToShowIS = 0;

        // 배너 소재가 refresh 될 때 마다 호출되기에, 최초 1회만 event log를 날린다.
        private bool _isLoggedEventShowBa = false;

        // Ad unit
        private string[] _bannerAdUnits = new string[1];
        private string[] _interstitialAdUnits = new string[1];
        private string[] _rewardedAdUnits = new string[1];
        
        private int _interstitialLoadRetryAttempt;
        private int _rewardedVideoLoadRetryAttempt;

        public void Initialize(AdUnitData adUnitData)
        {   
#if UNITY_ANDROID
            _bannerAdUnits[0] = adUnitData.AOS_BA;
            _interstitialAdUnits[0] = adUnitData.AOS_IS;
            _rewardedAdUnits[0] = adUnitData.AOS_RV;
#elif UNITY_IOS
            _bannerAdUnits[0] = adUnitData.IOS_BA;
            _interstitialAdUnits[0] = adUnitData.IOS_IS;
            _rewardedAdUnits[0] = adUnitData.IOS_RV;
#endif
            if (AlohaSdk.RemoteConfig.IsFetchCompleted)
            {
                InitializeMaxSdk();
            }
            else
            {
                AlohaSdk.RemoteConfig.OnFetchCompleted += () =>
                {
                    InitializeMaxSdk();
                };
            }
        }

        private void InitializeMaxSdk() 
        {
            MaxSdk.SetTestDeviceAdvertisingIdentifiers(AlohaSdk.RemoteConfig.Predefined.TEST_DEVICES_IDFA);
            MaxSdkCallbacks.OnSdkInitializedEvent += OnMaxSdkInitialized;
            MaxSdk.InitializeSdk();
        }

        private void OnMaxSdkInitialized(MaxSdkBase.SdkConfiguration sdkConfiguration)
        {
#if !UNITY_EDITOR
            //AdSettings.SetDataProcessingOptions(new string[] { });
#endif
#if UNITY_IOS || UNITY_IPHONE
            if (MaxSdkUtils.CompareVersions(UnityEngine.iOS.Device.systemVersion, "14.5") != MaxSdkUtils.VersionComparisonResult.Lesser)
            {
                // Note that App transparency tracking authorization can be checked via `sdkConfiguration.AppTrackingStatus` for Unity Editor and iOS targets
                // 1. Set Facebook ATE flag here, THEN
                if (sdkConfiguration.AppTrackingStatus == MaxSdkBase.AppTrackingStatus.Authorized)
                {
                    AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(true);   
                }
            }
#endif
            try
            {
                AlohaSdk.AddSdkLog("Max Sdk initialized");

                AlohaSdk.AddSdkLog(_bannerAdUnits[0]);
                AlohaSdk.AddSdkLog(_interstitialAdUnits[0]);
                AlohaSdk.AddSdkLog(_rewardedAdUnits[0]);

                SetEventListeners();

                if (AlohaSdk.Context.ShowBannerAdsOnInitialized)
                {
                    InitializeBanner();
                    ShowBanner();
                }
                LoadInterstitial();
                LoadRewardedAd();
            }
            catch (Exception e)
            {
                AlohaSdk.AddSdkLog(e.Message);
                AlohaSdk.AddSdkLog("Max Sdk Failed to initialized");
            }

            IsInitialized = true;
            OnInitialized?.Invoke();
        }
        
        // 이벤트 리스너 등록 
        void SetEventListeners()
        {
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnAdFailedEvent;
            
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;

            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
        }

        public void ShowBanner()
        {
            AlohaSdk.AddSdkLog("Show Ba");
            MaxSdk.ShowBanner(_bannerAdUnits[0]);
        }

        public enum InterstitialCondition
        {
            Success,
            SessionTimeFailed,
            AndConditionFailed,
            OrConditionFailed
        }
        
        public void TryShowInterstitial(int placementId, string placementName)
        {
            InterstitialCondition result = CheckInterstitialCondition();

            switch (result)
            {
                case InterstitialCondition.Success:
                {
                    _tryCountToShowIS += 1;

                    if (_tryCountToShowIS < AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_FIRST_SHOW_AT_PLACEMENT)
                    {
                        AlohaSdk.AddQaLog($"Need more {AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_FIRST_SHOW_AT_PLACEMENT - _tryCountToShowIS} tries (code:1)");
                    }
                
                    ShowInterstitial(_interstitialAdUnits[0], placementId, placementName);
                    break;
                }
                case InterstitialCondition.AndConditionFailed:
                    AlohaSdk.AddQaLog("AND condition But no show");
                    break;
                case InterstitialCondition.OrConditionFailed:
                    AlohaSdk.AddQaLog("OR condition But all is not True");
                    break;
                case InterstitialCondition.SessionTimeFailed:
                    AlohaSdk.AddQaLog($"Need more Play Time to show IS");
                    break;
            }
        }
        
        public InterstitialCondition CheckInterstitialCondition()
        {
            if (CheckSessionShowTime())
            {
                if (AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_AD_INTERVAL_DEPENDENCY.Trim().ToUpper() == "AND")
                {
                    if (CheckTimeInterval() && CheckFrequencyInterval())
                    {
                        return InterstitialCondition.Success;
                    }
                    
                    return InterstitialCondition.AndConditionFailed;
                }
                else
                {
                    if (CheckTimeInterval() || CheckFrequencyInterval())
                    {
                        return InterstitialCondition.Success;
                    }
                    
                    return InterstitialCondition.OrConditionFailed;
                }
            }
            
            return InterstitialCondition.SessionTimeFailed;
        }
        
        private bool CheckSessionShowTime()
        {
            float playTime = AlohaSdk.Context.PlayTime;
            int session = AlohaSdk.Context.Session;
            return session == 1 ? playTime >= AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_FIRST_SHOW_TIME
                : playTime >= AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_SESSION_SHOW_TIME;
        }
        
        private bool CheckTimeInterval()
        {
            int lastSecondToShow = (_recentTimeStampIsAds + AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_AD_TIME_INTERVAL) - GetCurrentTimeStamp();
            return lastSecondToShow <= 0;
        }

        private bool CheckFrequencyInterval()
        {
            return (_tryCountToShowIS - AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_FIRST_SHOW_AT_PLACEMENT) % 
                AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_AD_FREQUENCY_INTERVAL == 0;
        }

        private bool IsMaxInterstitialReady()
        {
            return MaxSdk.IsInterstitialReady(_interstitialAdUnits[0]);
        }

        private void ShowInterstitial(string adUnitId, int placementId, string placementName)
        {
            AlohaSdk.AddSdkLog("Show IS");
            if (IsMaxInterstitialReady())
            {
                if (_interstitialAdUnits[0] == adUnitId)
                {
                    AlohaSdk.Context.LastISPlacementId = placementId;
                    AlohaSdk.Context.LastISPlacementName = placementName;
                    
                    MaxSdk.ShowInterstitial(_interstitialAdUnits[0]);
                }
                else
                {
                    AlohaSdk.AddSdkLog("different Ad Unit Id : " + _interstitialAdUnits[0]);
                }   
            }
            else
            {
                void OnInterstitialLoadedCallback(string _, MaxSdkBase.AdInfo info)
                {
                    MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnInterstitialLoadedCallback;
                    ShowInterstitial(adUnitId, placementId, placementName);
                }
                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedCallback;
                LoadInterstitial();
            }
        }

        public void ShowRV(int placementId, string placementName)
        {
            if (IsRVReady())
            {
                AlohaSdk.AddSdkLog("Show RV");
                
                AlohaSdk.Context.LastRVPlacementId = placementId;
                AlohaSdk.Context.LastRVPlacementName = placementName;
                
                MaxSdk.ShowRewardedAd(_rewardedAdUnits[0]);
            }
        }

        public bool IsRVReady()
        {
            return MaxSdk.IsRewardedAdReady(_rewardedAdUnits[0]);
        }
        
        public async Task<bool> ShowRVAsync(int placementId, string placementName)
        {
            if (IsRVReady())
            {
                AlohaSdk.AddSdkLog("Show RV Async");
                
                AlohaSdk.Context.LastRVPlacementId = placementId;
                AlohaSdk.Context.LastRVPlacementName = placementName;
                
                var taskCompletionSource = new TaskCompletionSource<bool>();

                void OnAdReceivedReward(string _, MaxSdkBase.Reward __, MaxSdkBase.AdInfo ___)
                {
                    taskCompletionSource.TrySetResult(true);
                    MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= OnAdReceivedReward;
                    MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= OnFailed;
                }

                void OnFailed(string _, MaxSdkBase.AdInfo __)
                {
                    taskCompletionSource.TrySetResult(false);
                    MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= OnAdReceivedReward;
                    MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= OnFailed;
                }

                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnAdReceivedReward;
                MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnFailed;  

                MaxSdk.ShowRewardedAd(_rewardedAdUnits[0]);
                return await taskCompletionSource.Task;
            }

            return false;
        }

        #region Banner

        private void InitializeBanner()
        {
            // Banners are automatically sized to 320×50 on phones and 728×90 on tablets
            // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
            MaxSdk.CreateBanner(_bannerAdUnits[0], MaxSdkBase.BannerPosition.BottomCenter);

            // Set background or background color for banners to be fully functional
            MaxSdk.SetBannerBackgroundColor(_bannerAdUnits[0], Color.black);
        }
        
        // 배너 광고 불러오기 성공 시 호출 : OnAdLoadedEvent
        // 배너 소재가 refresh 될 때 마다 호출되기에, 최초 1회만 event log를 날린다. 
        private void OnAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            IsBannerLoaded = true;
            BannerLayout = MaxSdk.GetBannerLayout(_bannerAdUnits[0]);
            OnBannerLoaded?.Invoke();
            
            AlohaSdk.AddSdkLog("OnAdLoadedEvent : " + adUnitId);
        }

        // 배너 광고 불러오기 실패 시 호출 : OnAdFailedEvent
        private void OnAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            AlohaSdk.AddSdkLog("Failed to load banner : " + adUnitId);
            if (!string.IsNullOrEmpty(errorInfo.Message))
                AlohaSdk.AddSdkLog(errorInfo.Message);
        }

        // 배너 광고 중단.
        public void DestroyBanner()
        {
            AlohaSdk.AddQaLog("Destroy Banner");
            MaxSdk.HideBanner(_bannerAdUnits[0]);
        }
        
        #endregion

        #region Interstitial

        private void LoadInterstitial()
        {
            MaxSdk.LoadInterstitial(_interstitialAdUnits[0]);
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad is ready for you to show. MaxSdk.IsInterstitialReady(adUnitId) now returns 'true'
            // Reset retry attempt
            AlohaSdk.AddSdkLog("OnInterstitialLoadedEvent : " + adUnitId);
            _interstitialLoadRetryAttempt = 0;
        }

        private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Interstitial ad failed to load 
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)
            _interstitialLoadRetryAttempt++;
            int retryDelay = (int) Math.Pow(2, Math.Min(6, _interstitialLoadRetryAttempt));
            AlohaSdk.AddSdkLog("load fail interstitial : " + adUnitId);
            
            AlohaSdk.InvokeAfter(LoadInterstitial, (float) retryDelay);
        }

        private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {}

        private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
            LoadInterstitial();
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {}

        private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad is hidden. Pre-load the next ad.
            _recentTimeStampIsAds = GetCurrentTimeStamp();
            AlohaSdk.AddSdkLog("interstitial dismissed : " + adUnitId);
            LoadInterstitial();
        }

        #endregion

        #region Rewarded Video

        /*********************************************************/
        /***************** Rewarded Video 광고 관련 ****************/
        /*********************************************************

        게임에서 사용 시 아래를 호출.
            AlohaManager.ShowAds(AdsType.RewardVideo);

         *********************************************************/
        
        private void LoadRewardedAd()
        {
            MaxSdk.LoadRewardedAd(_rewardedAdUnits[0]);
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.
            AlohaSdk.AddSdkLog("Loaded rewarded video : " + adUnitId);
            // Reset retry attempt
            _rewardedVideoLoadRetryAttempt = 0;
        }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Rewarded ad failed to load 
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).
            AlohaSdk.AddSdkLog("Failed to load rewarded video : " + adUnitId);
            if (!string.IsNullOrEmpty(errorInfo.Message))
                AlohaSdk.AddSdkLog(errorInfo.Message);

            _rewardedVideoLoadRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, _rewardedVideoLoadRetryAttempt));
    
            AlohaSdk.InvokeAfter(LoadRewardedAd, (float) retryDelay);
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {}

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
            AlohaSdk.AddSdkLog("Failed to play rewarded video : " + adUnitId);
            LoadRewardedAd();
            if (!string.IsNullOrEmpty(errorInfo.Message))
                AlohaSdk.AddSdkLog(errorInfo.Message);
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {}

        private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is hidden. Pre-load the next ad
            AlohaSdk.AddSdkLog("Rewarded video dismiss : " + adUnitId);
            LoadRewardedAd();
            if (AlohaSdk.RemoteConfig.Predefined.RV_RESET_INT_QUEUE)
            {
                _tryCountToShowIS = AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_FIRST_SHOW_AT_PLACEMENT;
                _recentTimeStampIsAds = GetCurrentTimeStamp();
            }
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            // The rewarded ad displayed and the user should receive the reward.
            AlohaSdk.Ads.InvokeOnReceivedReward(AlohaSdk.Context.LastRVPlacementId);
        }
        
        #endregion

        //************** 기타 ******************//
        
        // 현재시간 가져오기.
        int GetCurrentTimeStamp()
        {
            TimeSpan time = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            return (int)time.TotalSeconds;
        }

        public void ForceResetInterstitialQueue()
        {
            _tryCountToShowIS = AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_FIRST_SHOW_AT_PLACEMENT + 0;
            _recentTimeStampIsAds = GetCurrentTimeStamp();
        }
    }
}