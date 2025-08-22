using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLovinMax.ThirdParty.MiniJson;
using UnityEngine;
using Firebase.RemoteConfig;

namespace Aloha.Sdk
{
    /// <summary>
    /// Firebase Remote Config에서 가져온 값은 여기에서 처리 및 가지고 있는다.
    /// </summary>
    internal class AlohaRemoteConfig
    {
        public bool IsFetchCompleted { get; private set; }
        public event Action OnFetchCompleted;
        
        private FirebaseRemoteConfig RemoteConfigInstance => FirebaseRemoteConfig.DefaultInstance;

        public async void Initialize()
        {
            try
            {
                AlohaSdk.AddSdkLog("Start InitializeFirebase Prod");
                await RemoteConfigInstance.SetDefaultsAsync(GetDefaults());
                
                Debug.Log("Fetching data...");
                Task fetchTask = RemoteConfigInstance.FetchAsync(TimeSpan.Zero);
                await fetchTask;

                if (fetchTask.IsCanceled) throw new Exception("Fetch canceled.");
                if (fetchTask.IsFaulted) throw new Exception("Fetch encountered an error.");
                
                if (RemoteConfigInstance.Info.LastFetchStatus == LastFetchStatus.Success)
                {
                    await RemoteConfigInstance.ActivateAsync();
                }
                else
                {
                    throw new Exception($"Fetch failed :: {RemoteConfigInstance.Info.LastFetchFailureReason}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("error prod, use default");
                Debug.LogError(e.Message);
            }
            
            AlohaSdk.AddSdkLog("RemoteConfig configured and ready!");
            OnCompleteFetch();
        }

        private Dictionary<string, object> GetDefaults()
        {
            return new Dictionary<string, object>()
            {
                {"TEST_AD_MODE", false },
                {"INTERSTITIAL_AD_TIME_INTERVAL", 60},
                {"INTERSTITIAL_FIRST_SHOW_AT_PLACEMENT", 3},
                {"INTERSTITIAL_AD_FREQUENCY_INTERVAL", 60},
                {"INTERSTITIAL_AD_INTERVAL_DEPENDENCY", "OR"},
                {"RV_RESET_INT_QUEUE", true},
                {"INTERSTITIAL_FIRST_SHOW_TIME", 60},
                {"INTERSTITIAL_SESSION_SHOW_TIME", 1},
                {"REVIEW_MODE", "0"},
                {"SHOW_PRIVACY_POLICY_POPUP_ON_START", false},
                {"TEST_DEVICES_IDFA", "[]"},
            };
        }

        private void OnCompleteFetch()
        {
            AlohaSdk.RemoteConfig.Predefined.TEST_AD_MODE = GetBooleanValue("TEST_AD_MODE");
            AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_AD_TIME_INTERVAL = (int)GetDoubleValue("INTERSTITIAL_AD_TIME_INTERVAL");
            AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_FIRST_SHOW_AT_PLACEMENT = (int)GetDoubleValue("INTERSTITIAL_FIRST_SHOW_AT_PLACEMENT");
            AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_AD_FREQUENCY_INTERVAL = (int)GetDoubleValue("INTERSTITIAL_AD_FREQUENCY_INTERVAL");
            AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_AD_INTERVAL_DEPENDENCY = GetStringValue("INTERSTITIAL_AD_INTERVAL_DEPENDENCY");
            AlohaSdk.RemoteConfig.Predefined.RV_RESET_INT_QUEUE = GetBooleanValue("RV_RESET_INT_QUEUE");
            AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_FIRST_SHOW_TIME = GetIntValue("INTERSTITIAL_FIRST_SHOW_TIME");
            AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_SESSION_SHOW_TIME = GetIntValue("INTERSTITIAL_SESSION_SHOW_TIME");
            AlohaSdk.RemoteConfig.Predefined.SHOW_PRIVACY_POLICY_POPUP_ON_START = GetBooleanValue("SHOW_PRIVACY_POLICY_POPUP_ON_START");
            AlohaSdk.RemoteConfig.Predefined.REVIEW_MODE = IsReviewMode();

            string testDevicesJson = GetStringValue("TEST_DEVICES_IDFA");
            AlohaSdk.RemoteConfig.Predefined.TEST_DEVICES_IDFA = ((List<object>) Json.Deserialize(testDevicesJson))
                .Select(obj => (string)obj).ToArray();

            AlohaSdk.AddSdkLog("TEST_AD_MODE: " + AlohaSdk.RemoteConfig.Predefined.TEST_AD_MODE);
            if (AlohaSdk.RemoteConfig.Predefined.REVIEW_MODE)
            {
                AlohaSdk.AddQaLog("Review Mode");
            }
            
            IsFetchCompleted = true;
            OnFetchCompleted?.Invoke();
        }

        private bool IsReviewMode()
        {
            var reviewMode = RemoteConfigInstance.GetValue("REVIEW_MODE").StringValue;
            
            if (reviewMode.Trim() == "0")
            {
                return false;
            }
            else
            {
                return reviewMode.Trim() == Application.version.Trim();
            }
        }

        public int GetIntValue(string key)
        {
            return (int)RemoteConfigInstance.GetValue(key).LongValue;
        }
        
        public bool GetBooleanValue(string key)
        {
            return RemoteConfigInstance.GetValue(key).BooleanValue;
        }
        
        public string GetStringValue(string key)
        {
            return RemoteConfigInstance.GetValue(key).StringValue;
        }
        
        public double GetDoubleValue(string key)
        {
            return RemoteConfigInstance.GetValue(key).DoubleValue;
        }
    }
}