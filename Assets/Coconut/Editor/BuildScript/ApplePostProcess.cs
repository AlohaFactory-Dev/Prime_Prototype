using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
using UnityEngine;
#endif

public class ApplePostProcess
{
#if UNITY_IOS
    [PostProcessBuild(1)]
    public static void OnPostProcessBuild_AddCapabilities(BuildTarget target, string path)
    {
        if (target != BuildTarget.iOS) return;

        var projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
        var project = new PBXProject();
        project.ReadFromString(File.ReadAllText(projectPath));

        var manager = new ProjectCapabilityManager(projectPath, "Entitlements.entitlements", null,
            project.GetUnityMainTargetGuid());
        manager.AddBackgroundModes(BackgroundModesOptions.BackgroundFetch | BackgroundModesOptions.RemoteNotifications);
        //manager.AddSignInWithAppleWithCompatibility(); // AppleAuth 에셋 필요함, 추후 추가
        manager.WriteToFile();
    }

    // iOS resolver - 40에서 Podfile생성, 50에서 pod install, 그 사이에 podfile 수정
    [PostProcessBuild(45)]
    public static void OnPostProcessBuild_Pod(BuildTarget buildTarget, string pathToXcode)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            string projectPath = pathToXcode + "/Unity-iPhone.xcodeproj/project.pbxproj";
            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);

            //Unity Framework
            string target = pbxProject.GetUnityFrameworkTargetGuid();
            pbxProject.AddFrameworkToProject(target, "AuthenticationServices.framework", true);
            pbxProject.SetBuildProperty(target, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");

            pbxProject.WriteToFile(projectPath);

            // Podfile 수정 - use_frameworks!
            var podfilePath = pathToXcode + "/Podfile";

            // pod 'FirebaseAnalyticsOnDeviceConversion' 추가 (구글 ODM(OnDeviceMeasurement) 사용을 위함)
            ModifyPodfileToAddPodString(podfilePath, "pod 'FirebaseAnalyticsOnDeviceConversion'");

            // Info.plist 수정 - ITSAppUsesNonExemptEncryption = false
            string infoPlistPath = pathToXcode + "/Info.plist";
            PlistDocument plistDoc = new PlistDocument();
            plistDoc.ReadFromFile(infoPlistPath);
            if (plistDoc.root != null)
            {
                plistDoc.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
                plistDoc.WriteToFile(infoPlistPath);
            }
            else
            {
                Debug.LogError("ERROR: Can't open " + infoPlistPath);
            }
        }
    }

    private static void ModifyPodfileToAddPodString(string podfilePath, string podString)
    {
        // "pod" 이라는 단어가 있는 마지막 라인 찾기
        string[] lines = File.ReadAllLines(podfilePath);
        int lastLineIndex = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("pod"))
            {
                lastLineIndex = i;
            }
        }

        // 해당 라인까지 복사 후 string 추가
        List<string> newLines = new List<string>();
        for (int i = 0; i <= lastLineIndex; i++)
        {
            newLines.Add(lines[i]);
        }

        newLines.Add($"  {podString}");

        // "pod" 이라는 단어가 있는 마지막 라인 이후 라인들을 추가
        for (int i = lastLineIndex + 1; i < lines.Length; i++)
        {
            newLines.Add(lines[i]);
        }

        // 복사한 라인들을 다시 쓰기
        File.WriteAllLines(podfilePath, newLines);
    }
#endif
}