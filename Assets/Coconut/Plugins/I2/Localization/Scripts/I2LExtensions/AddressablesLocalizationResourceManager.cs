using I2.Loc;
using System;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AddressablesLocalizationResourceManager : IResourceManager_Bundles
{
    public Object LoadFromBundle(string path, Type assetType)
    {
        return Addressables.LoadAssetAsync<Object>(path).WaitForCompletion();
    }
}
