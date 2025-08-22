using System.IO;
using System.Linq;
using UnityEditor;

public static class CoconutExportScripts
{
    [MenuItem("Coconut/Export AlohaDependencyManager", priority = 20)]
    public static void ExportAlohaDependencyManager()
    {
        if (Directory.Exists("Builds") == false)
        {
            Directory.CreateDirectory("Builds");   
        }
        
        string path = EditorUtility.SaveFilePanel("Save Package", "Builds", "AlohaDependencyManager", "zip");
        
        if (string.IsNullOrEmpty(path) == false)
        {
            if(File.Exists(path)) File.Delete(path);
            System.IO.Compression.ZipFile.CreateFromDirectory("Packages/AlohaDependencyManager", path);
        }
        
        EditorUtility.RevealInFinder(path);
    }
    
    [MenuItem("Coconut/Export Coconut", priority = 21)]
    public static void ExportCoconut()
    {
        string[] directories = new string[]
        {
            "Assets/Coconut"
        };

        var configFiles = AssetDatabase.FindAssets("t:ScriptableObject", new string[] { "Assets/Coconut/Configs" });
        var shelf = new ScriptableObjectShelf(configFiles.Select(AssetDatabase.GUIDToAssetPath).ToArray());
        shelf.Shelve();
        
        if (Directory.Exists("Builds") == false)
        {
            Directory.CreateDirectory("Builds");   
        }

        var version = GetDateVersion();
        File.WriteAllText("Assets/Coconut/version.txt", version);
        string path = EditorUtility.SaveFilePanel("Save Package", "Builds", $"Coconut_v{version}", "unitypackage");
        
        if (string.IsNullOrEmpty(path) == false)
        {
            AssetDatabase.ExportPackage(directories, path, ExportPackageOptions.Recurse);
            EditorUtility.RevealInFinder(path);   
        }
        
        shelf.Unshelve();
    }

    private static string GetDateVersion()
    {
        return System.DateTime.Now.ToString("yyyy.MM.dd");
    }
    
    [MenuItem("Coconut/Export CoconutMilk", priority = 22)]
    public static void ExportCoconutMilk()
    {
        string[] directories = new string[]
        {
            "Assets/CoconutMilk"
        };

        var configFiles = AssetDatabase.FindAssets("t:ScriptableObject", new string[] { "Assets/CoconutMilk/Configs" });
        var shelf = new ScriptableObjectShelf(configFiles.Select(AssetDatabase.GUIDToAssetPath).ToArray());
        shelf.Shelve();
        
        if (Directory.Exists("Builds") == false)
        {
            Directory.CreateDirectory("Builds");   
        }

        var version = GetDateVersion();
        File.WriteAllText("Assets/CoconutMilk/version.txt", version);
        string path = EditorUtility.SaveFilePanel("Save Package", "Builds", $"CoconutMilk_v{version}", "unitypackage");
        
        if (string.IsNullOrEmpty(path) == false)
        {
            AssetDatabase.ExportPackage(directories, path, ExportPackageOptions.Recurse);
            EditorUtility.RevealInFinder(path);   
        }
        
        shelf.Unshelve();
    }
    
    [MenuItem("Coconut/Export Durian", priority = 23)]
    public static void ExportDurian()
    {
        string[] directories = new string[]
        {
            "Assets/Durian"
        };

        var configFiles = AssetDatabase.FindAssets("t:ScriptableObject", new string[] { "Assets/Durian" });
        var shelf = new ScriptableObjectShelf(configFiles.Select(AssetDatabase.GUIDToAssetPath).ToArray());
        shelf.Shelve();
        
        if (Directory.Exists("Builds") == false)
        {
            Directory.CreateDirectory("Builds");   
        }

        var version = GetDateVersion();
        File.WriteAllText("Assets/Durian/version.txt", version);
        string path = EditorUtility.SaveFilePanel("Save Package", "Builds", $"Durian_v{version}", "unitypackage");
        
        if (string.IsNullOrEmpty(path) == false)
        {
            AssetDatabase.ExportPackage(directories, path, ExportPackageOptions.Recurse);
            EditorUtility.RevealInFinder(path);   
        }
        
        shelf.Unshelve();
    }
}
