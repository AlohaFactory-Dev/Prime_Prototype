using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ScriptableObjectShelf
{
    private class SOInfo
    {
        public string originalPath;
        public string shelfPath;
    }
    
    private string _tempDirectory = "Library/SOShelf";
    private List<SOInfo> _soInfos = new List<SOInfo>();

    public ScriptableObjectShelf(params string[] soPaths)
    {
        foreach (var soPath in soPaths)
        {
            if (File.Exists(soPath))
            {
                _soInfos.Add(new SOInfo()
                {
                    originalPath = soPath,
                    shelfPath = Path.Combine(_tempDirectory, Path.GetFileName(soPath))
                });   
            }
        }
    }
    
    public void Shelve()
    {
        if (!Directory.Exists(_tempDirectory))
        {
            Directory.CreateDirectory(_tempDirectory);
        }

        foreach (var soInfo in _soInfos)
        {
            File.Copy(soInfo.originalPath, soInfo.shelfPath, true);
            
            var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(soInfo.originalPath);
            var type = so.GetType();
            type.GetMethod("Reset")?.Invoke(so, null);
            EditorUtility.SetDirty(so);
            AssetDatabase.SaveAssetIfDirty(so);
        }
        
        AssetDatabase.Refresh();
    }

    public void Unshelve()
    {
        foreach (var soInfo in _soInfos)
        {
            File.Copy(soInfo.shelfPath, soInfo.originalPath, true);
        }
        
        Directory.Delete(_tempDirectory, true);
        AssetDatabase.Refresh();
    }
}
