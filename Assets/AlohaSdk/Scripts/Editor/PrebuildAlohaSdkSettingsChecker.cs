using System.Collections.Generic;
using AdjustSdk;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Aloha.Sdk.Editor
{
    public class PrebuildAlohaSdkSettingsChecker : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; } = 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var alohaSdkSettings = Resources.Load<AlohaSdkConfigs>("AlohaSdkConfigs");
            if (alohaSdkSettings == null)
            {
                throw new BuildFailedException("AlohaSdkSettings이 Resources 폴더에 존재하지 않습니다.");
            }

            var errorLogs = new List<string>();
            if (string.IsNullOrEmpty(alohaSdkSettings.appToken)) errorLogs.Add("Adjust appToken이 입력되지 않았습니다.");
            if (alohaSdkSettings.adjustEnvironment == AdjustEnvironment.Sandbox) errorLogs.Add("Adjust Environment가 Sandbox로 설정되어 있습니다.");
            if (string.IsNullOrEmpty(alohaSdkSettings.eventId_play_start)) errorLogs.Add("Adjust eventId_play_start이 입력되지 않았습니다.");
            if (string.IsNullOrEmpty(alohaSdkSettings.eventId_play_end)) errorLogs.Add("Adjust eventId_play_end이 입력되지 않았습니다.");
            if (string.IsNullOrEmpty(alohaSdkSettings.eventId_time_spent)) errorLogs.Add("Adjust eventId_time_spent이 입력되지 않았습니다.");
            if (string.IsNullOrEmpty(alohaSdkSettings.eventId_tutorial)) errorLogs.Add("Adjust tutorial이 입력되지 않았습니다.");

            if (errorLogs.Count > 0)
            {
                var errorLog = "AlohaSdkSettings에서 다음과 같은 문제가 발견되었습니다:";
                foreach (var log in errorLogs)
                {
                    errorLog += $"\n{log}";
                }

                // if (EditorUtility.DisplayDialog("AlohaSdk Warnings", errorLog, "무시", "빌드 취소") == false)
                // {
                //     throw new BuildFailedException("AlohaSdk가 빌드를 취소했습니다.");
                // }
            }
        }
    }
}