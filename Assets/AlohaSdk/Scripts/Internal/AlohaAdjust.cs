using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdjustSdk;
using UnityEngine;

namespace Aloha.Sdk
{
    internal static class AlohaAdjustExtension
    {
        internal static string ToJsonString(this List<AlohaAdjust.Param> list)
        {
            var paramDict = new Dictionary<string, string>();
            foreach (var p in list)
            {
                paramDict[p.key] = p.value; // Later values override earlier ones for duplicate keys
            }
            var jsonParams = "{" + string.Join(",", paramDict.Select(p => $"\"{p.Key}\":\"{p.Value}\"")) + "}";
            return jsonParams;
        }
    }

    internal class AlohaAdjust
    {
        public enum PredefinedEvent
        {
            play_start, play_end, time_spent, in_app_purchase, mobile_purchase, asset, item, tutorial, in_app_ads
        }

        public struct Param
        {
            public string key;
            public string value;

            public Param(string key, string value)
            {
                this.key = key;
                this.value = value;
            }
        }

        private Dictionary<PredefinedEvent, string> _eventIds;
        private Param _param_cuid;
        private Param _param_ad_frequency_interval;
        private Param _param_ad_interval_dependency;
        private Param _param_ad_time_interval;
        private Param _param_ad_first_show_at_placement;
        private Param _param_ad_first_show_time;
        private Param _param_ad_session_show_time;

        public void Initialize()
        {
            if (AlohaSdk.Context.IsOffline) Adjust.SwitchToOfflineMode();
            AlohaSdk.Context.OnOfflineStatusChanged += (isOffline) =>
            {
                if (isOffline) Adjust.SwitchToOfflineMode();
                else Adjust.SwitchBackToOnlineMode();

                Debug.Log($"Adjust offline mode: {isOffline}");
            };

            _param_cuid = new Param("cuid", AlohaSdk.CUID);
            SetSessionParameter(_param_cuid);
            SetSessionParameter(GetStageParam());

            if (AlohaSdk.RemoteConfig.IsFetchCompleted) InitRemoteConfigParameters();
            else AlohaSdk.RemoteConfig.OnFetchCompleted += InitRemoteConfigParameters;

            _eventIds = new Dictionary<PredefinedEvent, string>
            {
                { PredefinedEvent.play_start, AlohaSdk.SdkConfigs.eventId_play_start },
                { PredefinedEvent.play_end, AlohaSdk.SdkConfigs.eventId_play_end },
                { PredefinedEvent.time_spent, AlohaSdk.SdkConfigs.eventId_time_spent },
                { PredefinedEvent.in_app_purchase, AlohaSdk.SdkConfigs.eventId_in_app_purchase },
                { PredefinedEvent.mobile_purchase, AlohaSdk.SdkConfigs.eventId_mobile_purchase },
                { PredefinedEvent.asset, AlohaSdk.SdkConfigs.eventId_asset },
                { PredefinedEvent.item, AlohaSdk.SdkConfigs.eventId_item },
                { PredefinedEvent.tutorial, AlohaSdk.SdkConfigs.eventId_tutorial },
                { PredefinedEvent.in_app_ads, AlohaSdk.SdkConfigs.eventId_in_app_ads}
            };

            StartAdjustWithConfig();
            SetAdTracking();
        }

        private void StartAdjustWithConfig()
        {
            AdjustConfig adjustConfig =
                new AdjustConfig(AlohaSdk.SdkConfigs.appToken, AlohaSdk.SdkConfigs.adjustEnvironment, false);
            adjustConfig.LogLevel = AdjustLogLevel.Info;
            adjustConfig.IsSendingInBackgroundEnabled = false;
            adjustConfig.IsDeferredDeeplinkOpeningEnabled = true;
            adjustConfig.IsSkanAttributionEnabled = true;
            adjustConfig.IsAdServicesEnabled = true;
            adjustConfig.IsIdfaReadingEnabled = true;
            Adjust.InitSdk(adjustConfig);
            AlohaSdk.AddSdkLog("Adjust SDK initialized");
        }

        private void InitRemoteConfigParameters()
        {
            SetRemoteConfigParameters();
            SetSessionParameter(_param_ad_frequency_interval);
            SetSessionParameter(_param_ad_interval_dependency);
            SetSessionParameter(_param_ad_time_interval);
            SetSessionParameter(_param_ad_first_show_at_placement);
            SetSessionParameter(_param_ad_first_show_time);
            SetSessionParameter(_param_ad_session_show_time);
        }

        private void SetRemoteConfigParameters()
        {
            _param_ad_frequency_interval = new Param("INTERSTITIAL_AD_FREQUENCY_INTERVAL",
                AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_AD_FREQUENCY_INTERVAL.ToString());
            _param_ad_interval_dependency = new Param("INTERSTITIAL_AD_INTERVAL_DEPENDENCY",
                AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_AD_INTERVAL_DEPENDENCY.ToString());
            _param_ad_time_interval = new Param("INTERSTITIAL_AD_TIME_INTERVAL",
                AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_AD_TIME_INTERVAL.ToString());
            _param_ad_first_show_at_placement = new Param("INTERSTITIAL_FIRST_SHOW_AT_PLACEMENT",
                AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_FIRST_SHOW_AT_PLACEMENT.ToString());
            _param_ad_first_show_time = new Param("INTERSTITIAL_FIRST_SHOW_TIME",
                AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_FIRST_SHOW_TIME.ToString());
            _param_ad_session_show_time = new Param("INTERSTITIAL_SESSION_SHOW_TIME",
                AlohaSdk.RemoteConfig.Predefined.INTERSTITIAL_SESSION_SHOW_TIME.ToString());
        }

        private void SetSessionParameter(Param param)
        {
            Adjust.AddGlobalCallbackParameter(param.key, param.value);
        }

        private void SetAdTracking()
        {
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
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent_IS;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent_RV;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent_Banner;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent_RV;
            MaxSdkCallbacks.RewardedInterstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent_RV;
        }

        private void OnAdRevenuePaidEvent_IS(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnAdRevenuePaidEvent(adUnitId, adInfo,
                AlohaSdk.Context.LastISPlacementId.ToString(),
                AlohaSdk.Context.LastISPlacementName);
        }

        private void OnAdRevenuePaidEvent_RV(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnAdRevenuePaidEvent(adUnitId, adInfo,
                AlohaSdk.Context.LastRVPlacementId.ToString(),
                AlohaSdk.Context.LastRVPlacementName);
        }

        private void OnAdRevenuePaidEvent_Banner(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnAdRevenuePaidEvent(adUnitId, adInfo, "1001", "banner_default");
        }

        private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo, string placementId,
            string placementName)
        {
            var adjustAdRevenue = new AdjustAdRevenue("applovin_max_sdk");
            adjustAdRevenue.SetRevenue(adInfo.Revenue, "USD");
            adjustAdRevenue.AdRevenueNetwork = adInfo.NetworkName;
            adjustAdRevenue.AdRevenuePlacement = adInfo.Placement;
            adjustAdRevenue.AdRevenueUnit = adInfo.AdUnitIdentifier;
            AddParam(adjustAdRevenue, _param_cuid);
            AddParam(adjustAdRevenue, _param_ad_frequency_interval);
            AddParam(adjustAdRevenue, _param_ad_interval_dependency);
            AddParam(adjustAdRevenue, _param_ad_time_interval);
            AddParam(adjustAdRevenue, _param_ad_first_show_at_placement);
            AddParam(adjustAdRevenue, _param_ad_first_show_time);
            AddParam(adjustAdRevenue, _param_ad_session_show_time);
            AddParam(adjustAdRevenue, GetStageParam());
            AddParam(adjustAdRevenue, new Param("placement_id", placementId));
            AddParam(adjustAdRevenue, new Param("placement_name", placementName));
            Adjust.TrackAdRevenue(adjustAdRevenue);

            var iaaEvent = new AdjustEvent(_eventIds[PredefinedEvent.in_app_ads]);
            iaaEvent.SetRevenue(adInfo.Revenue, "USD");
            AddParam(iaaEvent, new Param("ad_revenue_network", adInfo.NetworkName));
            AddParam(iaaEvent, new Param("ad_revenue_placement", adInfo.Placement));
            AddParam(iaaEvent, new Param("ad_revenue_unit", adInfo.AdUnitIdentifier));
            AddParam(iaaEvent, _param_cuid);
            AddParam(iaaEvent, _param_ad_frequency_interval);
            AddParam(iaaEvent, _param_ad_interval_dependency);
            AddParam(iaaEvent, _param_ad_time_interval);
            AddParam(iaaEvent, _param_ad_first_show_at_placement);
            AddParam(iaaEvent, _param_ad_first_show_time);
            AddParam(iaaEvent, _param_ad_session_show_time);
            AddParam(iaaEvent, GetStageParam());
            AddParam(iaaEvent, new Param("placement_id", placementId));
            AddParam(iaaEvent, new Param("placement_name", placementName));
            Adjust.TrackEvent(iaaEvent);
        }

        private void AddParam(AdjustAdRevenue ev, Param p)
        {
            ev.AddCallbackParameter(p.key, p.value);
        }

        public void LogEvent(PredefinedEvent predefinedEvent, params Param[] parameters)
        {
            if (!_eventIds.ContainsKey(predefinedEvent)
                || string.IsNullOrEmpty(_eventIds[predefinedEvent]))
            {
                return;
            }
            LogEvent(_eventIds[predefinedEvent], parameters);
        }

        public void LogEvent(string eventId, params Param[] parameters)
        {
            var adjustEvent = new AdjustEvent(eventId);

            AddParam(adjustEvent, _param_cuid);
            AddParam(adjustEvent, GetStageParam());
            AddParam(adjustEvent, _param_ad_frequency_interval);
            AddParam(adjustEvent, _param_ad_interval_dependency);
            AddParam(adjustEvent, _param_ad_time_interval);
            AddParam(adjustEvent, _param_ad_first_show_at_placement);
            AddParam(adjustEvent, _param_ad_first_show_time);
            AddParam(adjustEvent, _param_ad_session_show_time);

            foreach (var p in parameters)
            {
                AddParam(adjustEvent, p);
            }

            Adjust.TrackEvent(adjustEvent);
        }

        private void AddParam(AdjustEvent ev, Param p)
        {
            ev.AddCallbackParameter(p.key, p.value);
        }

        public void LogIAPEvent(string isoCurrencyCode, double price, string transactionId, params Param[] parameters)
        {
            var adjustEvent = new AdjustEvent(_eventIds[PredefinedEvent.in_app_purchase]);
            adjustEvent.SetRevenue(price, isoCurrencyCode);
            adjustEvent.AddCallbackParameter("transaction_id", transactionId);

            AddParam(adjustEvent, _param_cuid);
            AddParam(adjustEvent, GetStageParam());
            AddParam(adjustEvent, _param_ad_frequency_interval);
            AddParam(adjustEvent, _param_ad_interval_dependency);
            AddParam(adjustEvent, _param_ad_time_interval);
            AddParam(adjustEvent, _param_ad_first_show_at_placement);
            AddParam(adjustEvent, _param_ad_first_show_time);
            AddParam(adjustEvent, _param_ad_session_show_time);

            foreach (var p in parameters)
            {
                AddParam(adjustEvent, p);
            }

            Adjust.TrackEvent(adjustEvent);
        }

        public void LogMobilePurchaseEvent(string isoCurrencyCode, double price, string transactionId, Param[] parameters)
        {
            if (!_eventIds.ContainsKey(PredefinedEvent.mobile_purchase)
                || string.IsNullOrEmpty(_eventIds[PredefinedEvent.mobile_purchase]))
            {
                return;
            }

            var adjustEvent = new AdjustEvent(_eventIds[PredefinedEvent.mobile_purchase]);
            adjustEvent.SetRevenue(price, isoCurrencyCode);
            adjustEvent.AddCallbackParameter("transaction_id", transactionId);

            AddParam(adjustEvent, _param_cuid);
            AddParam(adjustEvent, GetStageParam());
            AddParam(adjustEvent, _param_ad_frequency_interval);
            AddParam(adjustEvent, _param_ad_interval_dependency);
            AddParam(adjustEvent, _param_ad_time_interval);
            AddParam(adjustEvent, _param_ad_first_show_at_placement);
            AddParam(adjustEvent, _param_ad_first_show_time);
            AddParam(adjustEvent, _param_ad_session_show_time);

            foreach (var p in parameters)
            {
                AddParam(adjustEvent, p);
            }

            Adjust.TrackEvent(adjustEvent);
        }

        private static Param GetStageParam()
        {
            return new Param("stage", AlohaSdk.Context.Stage.ToString());
        }
    }
}