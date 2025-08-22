using System;
using UnityEngine;

namespace Aloha.Sdk
{
    public partial class AlohaSdk
    {
        public static class Context
        {
            public static int Stage
            {
                get => PlayerPrefs.GetInt("alohaSdk.stage", 0); 
                internal set => PlayerPrefs.SetInt("alohaSdk.stage", value);
            }

            public static int TutorialStage
            {
                get => PlayerPrefs.GetInt("alohaSdk.tutorial_stage", 0); 
                internal set => PlayerPrefs.SetInt("alohaSdk.tutorial_stage", value);
            }
            
            public static int Session 
            {
                get => PlayerPrefs.GetInt("alohaSdk.session", 0); 
                internal set => PlayerPrefs.SetInt("alohaSdk.session", value); 
            }
            
            public static bool ShowBannerAdsOnInitialized { get; internal set; } = true;
            public static float PlayTime { get; internal set; }   
            public static float SessionPlayTime { get; internal set; }

            public static int AccountLevel
            {
                get => PlayerPrefs.GetInt("alohaSdk.account_lv", 0);
                internal set => PlayerPrefs.SetInt("alohaSdk.account_lv", value);
            }

            internal static int LastISPlacementId { get; set; }
            internal static string LastISPlacementName { get; set; }
        
            internal static int LastRVPlacementId { get; set; }
            internal static string LastRVPlacementName { get; set; }
            
            public static bool NotificationAgreed { get; internal set; }
            
            public static bool IsOffline
            {
                get => _isOffline;
                internal set
                {
                    var prevValue = _isOffline;
                    _isOffline = value;
                    if (prevValue != _isOffline) OnOfflineStatusChanged?.Invoke(_isOffline);
                }
            }
            private static bool _isOffline;
            public static event Action<bool> OnOfflineStatusChanged; 
        }
    }
}