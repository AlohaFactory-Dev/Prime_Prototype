using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public static class AddressableExtensions
{
    // Code from https://forum.unity.com/threads/set-addressable-via-c.671938/#post-6819911
    public static void SetAddressableGroup(this Object obj, string groupName, string address = null)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
 
        if (settings)
        {
            var group = settings.FindGroup(groupName);
            if (!group)
                group = settings.CreateGroup(groupName, false, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
 
            var assetpath = AssetDatabase.GetAssetPath(obj);
            var guid = AssetDatabase.AssetPathToGUID(assetpath);
 
            var e = settings.CreateOrMoveEntry(guid, group, false, false);
            if (address != null)
            {
                e.address = address;
            }
            var entriesAdded = new List<AddressableAssetEntry> {e};
 
            group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false, true);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true, false);
        }
    }

    public static void SetAddressableGroupDefault(this Object obj, string address = null)
    {
        SetAddressableGroup(obj, AddressableAssetSettingsDefaultObject.Settings.DefaultGroup.Name, address);
    }
    
    public static bool IsAddressable(this Object asset)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        foreach (var group in settings.groups)
        {
            foreach (var entry in group.entries)
            {
                if (entry.MainAsset == asset)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsAddressablePath(this Object asset, string path)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        foreach (var group in settings.groups)
        {
            foreach (var entry in group.entries)
            {
                if (entry.MainAsset == asset && entry.address == path)
                {
                    return true;
                }
            }
        }

        return false;
    }
}