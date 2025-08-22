using System;

namespace Aloha.Sdk
{
    public partial class AlohaSdk
    {
        public static class RemoteConfig
        {
            public static class Predefined
            {
                public static bool TEST_AD_MODE;
            
                public static int INTERSTITIAL_AD_TIME_INTERVAL;
                public static int INTERSTITIAL_AD_FREQUENCY_INTERVAL;
                public static string INTERSTITIAL_AD_INTERVAL_DEPENDENCY;
                public static int INTERSTITIAL_FIRST_SHOW_AT_PLACEMENT;
                public static bool RV_RESET_INT_QUEUE;
                public static bool REVIEW_MODE;

                public static bool SHOW_PRIVACY_POLICY_POPUP_ON_START;
            
                public static int INTERSTITIAL_FIRST_SHOW_TIME = 60;
                public static int INTERSTITIAL_SESSION_SHOW_TIME = 30;
            
                public static string[] TEST_DEVICES_IDFA;
            }

            public static bool IsFetchCompleted => _alohaRemoteConfig.IsFetchCompleted;
            public static event Action OnFetchCompleted
            {
                add => _alohaRemoteConfig.OnFetchCompleted += value;
                remove => _alohaRemoteConfig.OnFetchCompleted -= value;
            }
            
            public static int GetIntValue(string key)
            {
                CheckIsInitialized();
                return _alohaRemoteConfig.GetIntValue(key);
            }
        
            public static bool GetBooleanValue(string key)
            {
                CheckIsInitialized();
                return _alohaRemoteConfig.GetBooleanValue(key);
            }
        
            public static string GetStringValue(string key)
            {
                CheckIsInitialized();
                return _alohaRemoteConfig.GetStringValue(key);
            }
        
            public static double GetDoubleValue(string key)
            {
                CheckIsInitialized();
                return _alohaRemoteConfig.GetDoubleValue(key);
            }
        }
    }   
}
