using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Aloha.Coconut.Editor
{
    [InitializeOnLoad]
    public static class ConfigsAddressableSetter
    {
        private const string LABEL = "coconut.configs";
        
        static ConfigsAddressableSetter()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        private static void OnAfterAssemblyReload()
        {
            if (AddressableAssetSettingsDefaultObject.Settings == null)
            {
                Debug.LogWarning("AddressableAssetSettingsDefaultObject.Settings is null");
                return;
            }
            
            var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (!addressableSettings.GetLabels().Contains(LABEL))
            {
                addressableSettings.AddLabel(LABEL);
            }
            
            // Assets/Coconut으로 시작하는 폴더들 아래의 Config들 검색
            var coconutFolders = Directory.GetDirectories("Assets", "Coconut*", SearchOption.TopDirectoryOnly);
            var configs = AssetDatabase.FindAssets("t:CoconutConfig", coconutFolders);
            
            foreach (var configGuid in configs)
            {
                var asset = AssetDatabase.LoadAssetAtPath<CoconutConfig>(AssetDatabase.GUIDToAssetPath(configGuid));
                var entry = addressableSettings.FindAssetEntry(configGuid, true);

                if (entry == null)
                {
                    asset.SetAddressableGroup("CoconutConfigs", asset.name);
                    entry = addressableSettings.FindAssetEntry(configGuid, true);
                }

                if (!entry.labels.Contains(LABEL))
                {
                    entry.SetLabel(LABEL, true);
                    entry.parentGroup.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, entry, true);
                }
            }
        }
    }
}