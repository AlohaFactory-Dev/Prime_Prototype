using Firebase.Analytics;

namespace Aloha.Sdk
{
    /// <summary>
    /// Firebase Event를 관리합니다.
    /// </summary>
    internal class AlohaFirebaseEvent
    {
        public void Initialize()
        {
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            if (MaxSdk.IsInitialized())
            {
                StartImpressionTrack();
            }
            else
            {
                MaxSdkCallbacks.OnSdkInitializedEvent += _ => StartImpressionTrack();
            }
        }

        private void StartImpressionTrack()
        {
            // Attach callbacks based on the ad format(s) you are using
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        }
    
        private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo impressionData)
        {
            double revenue = impressionData.Revenue;
        
            var impressionParameters = new[] {
                new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
                new Firebase.Analytics.Parameter("ad_source", impressionData.NetworkName),
                new Firebase.Analytics.Parameter("ad_unit_name", impressionData.AdUnitIdentifier),
                new Firebase.Analytics.Parameter("ad_format", impressionData.AdFormat),
                new Firebase.Analytics.Parameter("value", revenue),
                new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
            };
            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
        
            var impressionParameters2 = new[] {
                new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
                new Firebase.Analytics.Parameter("ad_source", impressionData.NetworkName),
                new Firebase.Analytics.Parameter("ad_unit_name", impressionData.AdUnitIdentifier),
                new Firebase.Analytics.Parameter("ad_format", impressionData.AdFormat),
                new Firebase.Analytics.Parameter("value", revenue),
                new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
            };
            Firebase.Analytics.FirebaseAnalytics.LogEvent("paid_ad_impression", impressionParameters2);
        }

        public void LogEvent(string eventName, params Parameter[] paramArray)
        {
            FirebaseAnalytics.LogEvent(eventName, paramArray);
        }
    }
}