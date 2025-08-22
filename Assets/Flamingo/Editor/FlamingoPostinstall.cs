#if UNITY_EDITOR && (UNITY_IOS || UNITY_VISIONOS)

using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;

public static class XcodeSwiftVersionPostProcess
{
    // set callbackOrder to 999 to ensure this runs as the last post process step
    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            Debug.Log(
                "Installing for iOS. Setting ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES and ENABLE_BITCODE to NO and adding StoreKit");
            ModifyFrameworks(path);
            AddFlamingoKitFramework(path);
            AddStoreKitFramework(path);
        }
    }

    private static void ModifyFrameworks(string path)
    {
        string projPath = PBXProject.GetPBXProjectPath(path);
        var project = new PBXProject();
        project.ReadFromFile(projPath);

        string mainTargetGuid = project.GetUnityMainTargetGuid();

        foreach (var targetGuid in new[] { mainTargetGuid, project.GetUnityFrameworkTargetGuid() })
        {
            project.SetBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
            // Older Unity versions still try to compile with Bitcode, which new versions of PHC
            // are no longer compatible with.
            project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
        }

        project.SetBuildProperty(mainTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
        project.AddBuildProperty(mainTargetGuid, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
        project.AddBuildProperty(mainTargetGuid, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/Frameworks");
        project.WriteToFile(projPath);
    }

    private static void AddFlamingoKitFramework(string path)
    {
        string projPath = PBXProject.GetPBXProjectPath(path);
        var project = new PBXProject();
        project.ReadFromFile(projPath);

        string mainTargetGuid = project.GetUnityMainTargetGuid();
        string frameworkPath = "Frameworks/Flamingo/Plugins/iOS/FlamingoKit.framework";
        string fileGuid = project.AddFile(frameworkPath, frameworkPath, PBXSourceTree.Source);
        project.AddFileToEmbedFrameworks(mainTargetGuid, fileGuid);

        project.WriteToFile(projPath);
    }

    private static void AddStoreKitFramework(string path)
    {
        string projPath = PBXProject.GetPBXProjectPath(path);
        var project = new PBXProject();
        project.ReadFromFile(projPath);

        string mainTargetGuid = project.GetUnityMainTargetGuid();
        project.AddFrameworkToProject(mainTargetGuid, "StoreKit.framework", false);

        project.WriteToFile(projPath);
    }
}
#endif