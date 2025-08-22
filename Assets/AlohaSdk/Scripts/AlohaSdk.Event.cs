using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Aloha.Sdk
{
    public partial class AlohaSdk
    {
        public static class Event
        {
            private static float LastPlayStartTime
            {
                get => PlayerPrefs.GetFloat("alohaSdk.last_play_start_time", 0f);
                set => PlayerPrefs.SetFloat("alohaSdk.last_play_start_time", value);
            }

            private static float LastPlayEndTime
            {
                get => PlayerPrefs.GetFloat("alohaSdk.last_play_end_time", 0f);
                set => PlayerPrefs.SetFloat("alohaSdk.last_play_end_time", value);
            }

            private static float LastTutorialEndTime
            {
                get => PlayerPrefs.GetFloat("alohaSdk.last_tutorial_end_time", 0f);
                set => PlayerPrefs.SetFloat("alohaSdk.last_tutorial_end_time", value);
            }

            private static AlohaAdjust.Param Param(string key, string value)
            {
                return new AlohaAdjust.Param(key, value);
            }

            private static List<AlohaAdjust.Param> CustomParameters(string[] customParamValues)
            {
                if (customParamValues.Length > 10)
                {
                    Debug.LogError("AlohaSdk :: Custom parameter count must be less than 10");
                }

                List<AlohaAdjust.Param> customParameters = new List<AlohaAdjust.Param>();
                for (int i = 0; i < 10; i++)
                {
                    string value = "";

                    if (customParamValues.Length > i)
                        value = customParamValues[i];

                    customParameters.Add(new AlohaAdjust.Param($"custom{i + 1}", value));
                }

                return customParameters;
            }

            public static void SetAccountLevel(int accountLevel)
            {
                Context.AccountLevel = accountLevel;
            }

            public static void LogPlayStart(int stage, params string[] customParamValues)
            {
                CheckIsInitialized();

                Context.Stage = stage;

                List<AlohaAdjust.Param> customParameters = CustomParameters(customParamValues);
                List<AlohaAdjust.Param> parameters = new List<AlohaAdjust.Param>
                {
                    Param("account_lv", Context.AccountLevel.ToString())
                };
                parameters.AddRange(customParameters);

                _alohaAdjust.LogEvent(AlohaAdjust.PredefinedEvent.play_start, parameters.ToArray());

                LastPlayStartTime = Context.PlayTime;

                // Flamingo SDK
                _flamingo.StartPlaySession(parameters.ToJsonString(), "");
            }

            public static void LogPlayEnd(string result = "win", params string[] customParamValues)
            {
                CheckIsInitialized();

                var stageTime = Context.PlayTime - LastPlayStartTime;
                var playTime = Context.PlayTime - LastPlayEndTime;

                List<AlohaAdjust.Param> customParameters = CustomParameters(customParamValues);
                List<AlohaAdjust.Param> parameters = new List<AlohaAdjust.Param>
                {
                    Param("stage_time", stageTime.ToString(CultureInfo.InvariantCulture)),
                    Param("play_time", playTime.ToString(CultureInfo.InvariantCulture)),
                    Param("result", result),
                    Param("account_lv", Context.AccountLevel.ToString())
                };
                parameters.AddRange(customParameters);

                _alohaAdjust.LogEvent(AlohaAdjust.PredefinedEvent.play_end, parameters.ToArray());

                LastPlayEndTime = Context.PlayTime;

                // Flamingo SDK
                _flamingo.EndPlaySession(parameters.ToJsonString(), "");
            }

            public static void LogTutorialEnd(int tutorialId, string tutorialName, params string[] customParamValues)
            {
                CheckIsInitialized();

                var playTime = Context.PlayTime - LastTutorialEndTime;
                Context.TutorialStage = tutorialId;

                List<AlohaAdjust.Param> customParameters = CustomParameters(customParamValues);
                List<AlohaAdjust.Param> parameters = new List<AlohaAdjust.Param>
                {
                    Param("play_time", playTime.ToString(CultureInfo.InvariantCulture)),
                    Param("tutorial_id", Context.TutorialStage.ToString()),
                    Param("tutorial_name", tutorialName),
                    Param("account_lv", Context.AccountLevel.ToString())
                };
                parameters.AddRange(customParameters);

                LastTutorialEndTime = Context.PlayTime;
                _alohaAdjust.LogEvent(AlohaAdjust.PredefinedEvent.tutorial, parameters.ToArray());

                // Flamingo SDK
                _flamingo.FinishTutorial(tutorialId.ToString(), tutorialName, parameters.ToJsonString());
            }

            public static void LogIAP(string isoCurrencyCode, double price, string transactionId, string itemId, string itemName,
                bool isTestPurchase = false, params string[] customParamValues)
            {
                CheckIsInitialized();

                List<AlohaAdjust.Param> customParameters = CustomParameters(customParamValues);
                List<AlohaAdjust.Param> parameters = new List<AlohaAdjust.Param>
                {
                    Param("item_id", itemId),
                    Param("item_name", itemName),
                    Param("is_test_purchase", isTestPurchase ? "1" : "0"),
                    Param("account_lv", Context.AccountLevel.ToString()),
                    Param("store_type", GetStoreName())
                };
                parameters.AddRange(customParameters);

                _alohaAdjust.LogIAPEvent(isoCurrencyCode, price, transactionId, parameters.ToArray());
                _alohaAdjust.LogMobilePurchaseEvent(isoCurrencyCode, price, transactionId, parameters.ToArray());

                // Flamingo SDK
                _flamingo.PurchaseIAP(GetStoreName(), isoCurrencyCode, price, itemId, transactionId, itemId, itemName, isTestPurchase, parameters.ToJsonString());
            }

            public static void LogAsset(int assetId, string assetName, long var, long previousVar, long currentVar,
                int actionId, string actionName, int objectId, string objectName, params string[] customParamValues)
            {
                CheckIsInitialized();

                List<AlohaAdjust.Param> customParameters = CustomParameters(customParamValues);
                List<AlohaAdjust.Param> parameters = new List<AlohaAdjust.Param>
                {
                    Param("asset_id", assetId.ToString()),
                    Param("asset_name", assetName),
                    Param("gubun", var > 0 ? "in" : "out"),
                    Param("var", var.ToString()),
                    Param("pre_var", previousVar.ToString()),
                    Param("curr_var", currentVar.ToString()),
                    Param("action_id", actionId.ToString()),
                    Param("action_name", actionName),
                    Param("object_id", objectId.ToString()),
                    Param("object_name", objectName),
                    Param("account_lv", Context.AccountLevel.ToString())
                };
                parameters.AddRange(customParameters);

                _alohaAdjust.LogEvent(AlohaAdjust.PredefinedEvent.asset, parameters.ToArray());

                // Flamingo SDK
                _flamingo.ChangeAssetAmount(
                    assetId.ToString(), assetName, 
                    var, currentVar, 
                    actionId.ToString(), actionName, 
                    objectId.ToString(), objectName, 
                    parameters.ToJsonString()
                );
            }

            public static void LogItem(string itemId, string itemName, int var, int previousVar, int currentVar,
                int actionId, string actionName, int objectId, string objectName, params string[] customParamValues)
            {
                CheckIsInitialized();

                List<AlohaAdjust.Param> customParameters = CustomParameters(customParamValues);
                List<AlohaAdjust.Param> parameters = new List<AlohaAdjust.Param>
                {
                    Param("item_id", itemId),
                    Param("item_name", itemName),
                    Param("gubun", var > 0 ? "in" : "out"),
                    Param("var", var.ToString()),
                    Param("pre_var", previousVar.ToString()),
                    Param("curr_var", currentVar.ToString()),
                    Param("action_id", actionId.ToString()),
                    Param("action_name", actionName),
                    Param("object_id", objectId.ToString()),
                    Param("object_name", objectName),
                    Param("account_lv", Context.AccountLevel.ToString()),
                };
                parameters.AddRange(customParameters);

                _alohaAdjust.LogEvent(AlohaAdjust.PredefinedEvent.item, parameters.ToArray());

                // Flamingo SDK
                _flamingo.ChangeItemAmount(
                    itemId, itemName, 
                    var, currentVar, 
                    actionId.ToString(), actionName, 
                    objectId.ToString(), objectName, 
                    parameters.ToJsonString()
                );
            }

            internal static void LogTimeSpent(int minutesPassed)
            {
                List<AlohaAdjust.Param> parameters = new List<AlohaAdjust.Param>
                {
                    Param("stage", Context.Stage.ToString(CultureInfo.InvariantCulture)),
                    Param("play_time", (minutesPassed * 60).ToString(CultureInfo.InvariantCulture)),
                    Param("account_lv", Context.AccountLevel.ToString(CultureInfo.InvariantCulture))
                };
                _alohaAdjust.LogEvent(AlohaAdjust.PredefinedEvent.time_spent, parameters.ToArray());

                // Flamingo SDK
                _flamingo.LogEvent(AlohaAdjust.PredefinedEvent.time_spent.ToString(), parameters.ToJsonString(), "");
            }

            public static void Log(string eventId, params string[] customParamValues)
            {
                CheckIsInitialized();

                List<AlohaAdjust.Param> customParameters = CustomParameters(customParamValues);
                _alohaAdjust.LogEvent(eventId, customParameters.ToArray());

                // Flamingo SDK
                _flamingo.LogEvent(eventId, customParameters.ToJsonString(), "");
            }
        }
    }
}
