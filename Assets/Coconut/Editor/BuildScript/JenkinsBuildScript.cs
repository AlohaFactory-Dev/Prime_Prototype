using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;
using UnityEditor.Build.Reporting;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public static class JenkinsBuildScript
{
    private static string KEYSTORE_FILE_NAME => Environment.GetEnvironmentVariable("KEYSTORE_FILE_NAME");
    private static string KEYSTORE_ALIAS_NAME => Environment.GetEnvironmentVariable("KEYSTORE_ALIAS_NAME");
    private static string PROJECT_CODE => Environment.GetEnvironmentVariable("PROJECT_CODE");

    public static void Build_Android()
    {
        bool clearCache = CheckClearCache();
        BuildAddressable(clearCache);
        SetDebugConfig();
        
        bool isAAB = Environment.GetCommandLineArgs().Any(arg => arg.StartsWith("-aab"));
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        EditorUserBuildSettings.buildAppBundle = isAAB;

        if (File.Exists($"Builds/{KEYSTORE_FILE_NAME}"))
        {
            Debug.Log("Keystore found.");
            var password = System.Environment.GetCommandLineArgs()
                .FirstOrDefault(arg => arg.StartsWith("-keystorePassword"));
            if (password != null)
            {
                PlayerSettings.Android.keystoreName = Path.GetFullPath($"Builds/{KEYSTORE_FILE_NAME}");
                PlayerSettings.Android.keystorePass = password.Split('=')[1];
                PlayerSettings.Android.keyaliasName = KEYSTORE_ALIAS_NAME;
                PlayerSettings.Android.keyaliasPass = password.Split('=')[1];
            }
        }
        else
        {
            Debug.Log("Keystore not found.");
        }

        if (isAAB)
        {
            Build($"Builds/{PROJECT_CODE}.aab", BuildTarget.Android, clearCache);   
        }
        else
        {
            Build($"Builds/{PROJECT_CODE}.apk", BuildTarget.Android, clearCache);
        }
    }

    private static bool CheckClearCache()
    {
        string clearCacheString = Environment.GetCommandLineArgs().FirstOrDefault(arg => arg.StartsWith("-clearCache"));
        return clearCacheString != null && bool.Parse(clearCacheString.Split('=')[1]);
    }
    
    public static void ClearCache()
    {
        AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder.ClearCachedData();
        BuildCache.PurgeCache(false);
        if (Directory.Exists("Library/Bee/")) Directory.Delete("Library/Bee/", true);
        if (Directory.Exists("Library/BuildCache/")) Directory.Delete("Library/ScriptAssemblies/", true);
        if (Directory.Exists("Library/Il2cppBuildCache/")) Directory.Delete("Library/Il2cppBuildCache/", true);
    }

    public static void Build_iOS()
    {
        bool clearCache = CheckClearCache();
        BuildAddressable(clearCache);
        SetDebugConfig();

        Build($"Builds/{PROJECT_CODE}_iOS", BuildTarget.iOS, clearCache);
    }

    private static void BuildAddressable(bool clearCache)
    {
        if (clearCache) AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder.ClearCachedData();
        
        AddressableAssetSettings.BuildPlayerContent(out var result);
        bool success = string.IsNullOrEmpty(result.Error);

        if (!success)
        {
            Debug.LogError("Addressables build error encountered: " + result.Error);
            throw new BuildFailedException($"Build failed");
        }
    }

    private static void SetDebugConfig()
    {
        DebugConfigEditor.LoadGameConfigInstance();
        var debug = System.Environment.GetCommandLineArgs().FirstOrDefault(arg => arg.StartsWith("-isDebug"));
        if (debug != null)
        {
            var isDebug = bool.Parse(debug.Split('=')[1]);
            DebugConfigEditor.SetDebugFlag(isDebug);
            Debug.LogError($"Debug mode set: {isDebug}");
        }

        // var oneStore = System.Environment.GetCommandLineArgs().FirstOrDefault(arg => arg.StartsWith("-isOneStore"));
        // if (oneStore != null)
        // {
        //     var useOneStore = bool.Parse(oneStore.Split('=')[1]);
        //     GameConfigEditor.SetOneStoreFlag(useOneStore);
        //     Debug.LogError($"UseOneStore set: {useOneStore}");
        // }
        
        var useDevServer = Environment.GetCommandLineArgs().FirstOrDefault(arg => arg.StartsWith("-useDevServer"));
        if (useDevServer != null)
        {
            var useDev = bool.Parse(useDevServer.Split('=')[1]);
            DebugConfigEditor.SetDevServerFlag(useDev);
            Debug.LogError($"UseDevServer set: {useDev}");
        }

        DebugConfigEditor.SetTutorialFlag(true);
        DebugConfigEditor.Save();
    }

    private static void Build(string pathName, BuildTarget target, bool clearCache)
    {
        // Set Build Number
        int buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER") != null
            ? int.Parse(Environment.GetEnvironmentVariable("BUILD_NUMBER"))
            : 0;
        if (target == BuildTarget.iOS) PlayerSettings.iOS.buildNumber = buildNumber.ToString();
        else if (target == BuildTarget.Android) PlayerSettings.Android.bundleVersionCode = buildNumber;
        Debug.Log($"Build Number: {buildNumber}");
        
        // Create Version File
        var versionTextPath = "Assets/Resources/version.txt";
        var isDebug = DebugConfigEditor.DebugConfig.useDebug;
        var versionName = $"v{PlayerSettings.bundleVersion}-{DateTime.Now:yyMMdd.HHmm}{(isDebug ? "D" : "R")}";
        if (File.Exists(versionTextPath)) File.Delete(versionTextPath);
        File.WriteAllText(versionTextPath, versionName);
        AssetDatabase.Refresh();

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
        buildPlayerOptions.locationPathName = pathName;
        buildPlayerOptions.target = target;
        buildPlayerOptions.options = clearCache ? BuildOptions.CleanBuildCache : BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        BuildSummary summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
        {
            if (target == BuildTarget.Android)
            {
                var versionText = $"v{PlayerSettings.bundleVersion}_{DateTime.Now:yyMMdd.HHmm}";
                if (DebugConfigEditor.DebugConfig.useDebug)
                {
                    versionText += "_DEBUG";
                }
                
                if (DebugConfigEditor.DebugConfig.useDevServer)
                {
                    versionText += "_DEV";
                }

                // if (GameConfigEditor.GameConfig.oneStore)
                // {
                //     versionText += "_ONESTORE";
                // }

                versionText += $"_{PlayerSettings.Android.bundleVersionCode}";
                File.WriteAllText("Builds/version", versionText);
                File.WriteAllText("Builds/product_name", PlayerSettings.productName);
            }
            else if (target == BuildTarget.iOS)
            {
#if UNITY_IOS
                // Product Name은 PlayerSettings의 Product Name으로 자동 설정되지만 자동 설정 과정에서 한국어 미지원, 공백 생략 등 변경이 생길 수 있으므로 PBXProject에서 새로 읽어와야 함 
                string pbxProjectPath = PBXProject.GetPBXProjectPath(pathName);
                PBXProject pbxProject = new PBXProject();
                pbxProject.ReadFromFile(pbxProjectPath);
                string productName = pbxProject.GetBuildPropertyForAnyConfig(pbxProject.ProjectGuid(), "PRODUCT_NAME_APP");
                File.WriteAllText("Builds/product_name", productName);
#endif
            }

            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build failed");
            throw new BuildFailedException($"Build failed");
        }
    }
}