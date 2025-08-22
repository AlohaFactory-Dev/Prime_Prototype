using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class ImageContainer
{
    private static Dictionary<string, Sprite> _cachedImage = new();

    public static async UniTask InitializeAsync()
    {
        var loadResourcehandle = Addressables.LoadResourceLocationsAsync("Image", typeof(Sprite));
        await loadResourcehandle.Task;

        var opList = new List<AsyncOperationHandle<Sprite>>();
        var keyList = new List<string>();
        foreach (var t in loadResourcehandle.Result)
        {
            var handle = Addressables.LoadAssetAsync<Sprite>(t.PrimaryKey);
            var key = t.PrimaryKey;
            opList.Add(handle);
            keyList.Add(key);
        }


        await Task.WhenAll(opList.Select(op => op.Task));
        for (var i = 0; i < opList.Count; i++)
        {
            _cachedImage[keyList[i]] = opList[i].Result;
        }

        Addressables.Release(loadResourcehandle);
    }

    public static Sprite GetImage(string id)
    {
        if (!_cachedImage.ContainsKey(id))
        {
            Debug.LogError($"Sprite : {id}가 없습니다.");
            return _cachedImage["UI_Icon_Skill_P_CoconutBombChili"];
        }

        return _cachedImage[id];
    }
}