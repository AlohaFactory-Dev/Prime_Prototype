using System;
using AdjustSdk;
using UnityEngine;

namespace Aloha.Sdk
{
    [Serializable]
    public class AdUnitData
    {
        public string AOS_BA;
        public string AOS_IS;
        public string AOS_RV;

        public string IOS_BA;
        public string IOS_IS;
        public string IOS_RV;
    }

    public class AlohaSdkConfigs : ScriptableObject
    {
        public bool showBannerAdsOnInitialized = false;
        public bool autoCUID = true;
        public float offlineCheckInterval = 30f;

        [Header("Ad Unit Ids")]
        public AdUnitData adUnitData;

        [Header("Adjust configurations")]
        public string appToken;
        public AdjustEnvironment adjustEnvironment;

        public string eventId_play_start;
        public string eventId_play_end;
        public string eventId_time_spent;
        public string eventId_tutorial;
        public string eventId_in_app_purchase;

        public string eventId_asset;
        public string eventId_item;

        public string eventId_mobile_purchase;
        public string eventId_in_app_ads;

        public string flamingoAccessKey;

        public void Reset()
        {
            showBannerAdsOnInitialized = false;
            autoCUID = true;
            offlineCheckInterval = 30f;

            adUnitData = new AdUnitData();

            appToken = "";
            adjustEnvironment = AdjustEnvironment.Sandbox;

            eventId_play_start = "";
            eventId_play_end = "";
            eventId_time_spent = "";
            eventId_tutorial = "";
            eventId_in_app_purchase = "";

            eventId_asset = "";
            eventId_item = "";

            eventId_mobile_purchase = "";
            eventId_in_app_ads = "";

            flamingoAccessKey = "";
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Aloha/Select Sdk Configs")]
        public static void SelectSdkSettings()
        {
            var path = "Assets/AlohaSdk/Resources/AlohaSdkConfigs.asset";
            if (System.IO.File.Exists(path))
            {
                UnityEditor.Selection.activeObject =
                    UnityEditor.AssetDatabase.LoadAssetAtPath<AlohaSdkConfigs>(path);
                UnityEditor.EditorGUIUtility.PingObject(UnityEditor.Selection.activeObject);
            }
            else
            {
                Debug.LogError($"Cannot find {path}.");
            }
        }
#endif
    }
}