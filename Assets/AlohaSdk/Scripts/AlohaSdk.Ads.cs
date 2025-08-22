using System;
using System.Collections;
using System.Threading.Tasks;

namespace Aloha.Sdk
{
    public partial class AlohaSdk
    {
        public static class Ads
        {
            public static bool IsInitialized => _alohaAds.IsInitialized;
            public static event Action OnInitialized
            {
                add => _alohaAds.OnInitialized += value;
                remove => _alohaAds.OnInitialized -= value;
            }
            
            public static event Action<int> OnReceivedReward;
            public static event Action<int> OnRewardedAdFailed;
            
            /// 광고 노출 (Banner)
            public static void ShowBannerAd()
            {
                CheckIsInitialized();
                _alohaAds.ShowBanner();
            }

            /// 배너 광고 중단.
            public static void DestroyBanner()
            {
                CheckIsInitialized();
                _alohaAds.DestroyBanner();
            }

            /// 광고 노출. (Interstitial)
            public static void ShowInterstitialAd(int placementId, string placementName)
            {
                CheckIsInitialized();
                _alohaAds.TryShowInterstitial(placementId, placementName);
            }

            /// 광고 노출 가능 여부. (Interval > 0)
            public static bool CanActivateInterstitialAd()
            {
                return GetInterstitialAdInterval() >= 0;
            }
        
            /// 광고 노출 조건 달성 여부.
            public static bool IsInterstitialAdReady()
            {
                CheckIsInitialized();
                return _alohaAds.CheckInterstitialCondition() == AlohaAds.InterstitialCondition.Success;
            }

            public static int GetInterstitialAdInterval()
            {
                return AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_AD_TIME_INTERVAL;
            }

            /// 광고 노출. (Reward Video)
            public static async void ShowRewardedAd(int placementId, string placementName)
            {
                CheckIsInitialized();
                if (await ShowRewardedAdAsync(placementId, placementName))
                {
                    OnReceivedReward?.Invoke(placementId);
                }
                else
                {
                    OnRewardedAdFailed?.Invoke(placementId);   
                }
            }
            
            public static async void ShowRewardedAd(int placementId, string placementName, Action onReceivedReward, 
                Action onFailed = null)
            {
                CheckIsInitialized();
                if (await ShowRewardedAdAsync(placementId, placementName))
                {
                    onReceivedReward?.Invoke();
                }
                else
                {
                    onFailed?.Invoke();   
                }
            }

            public static bool IsRewardedAdReady()
            {
                CheckIsInitialized();
                return _alohaAds.IsRVReady();
            }

            public static async Task<bool> ShowRewardedAdAsync(int placementId, string placementName)
            {
                CheckIsInitialized();
                return await _alohaAds.ShowRVAsync(placementId, placementName);
            }

            public static void ShowAdNotReadyPopup()
            {
                AlohaSimplePopup.ShowFromResourceTask("AlohaAdNotReadyPopup");
            }

            public static async Task ShowAdNotReadyPopupTask()
            {
                await AlohaSimplePopup.ShowFromResourceTask("AlohaAdNotReadyPopup");
            }

            public static IEnumerator ShowAdNotReadyPopupCoroutine()
            {
                var task = ShowAdNotReadyPopupTask();
                while (!task.IsCompleted) yield return null;
            }

            public static void InvokeOnReceivedReward(int placementId)
            {
                CheckIsInitialized();
                OnReceivedReward?.Invoke(placementId);
            }
        }
    }   
}
