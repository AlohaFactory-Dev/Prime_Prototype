using System.IO;
using UnityEngine;

public static class StreamingAssetReader
{
    public static string ReadText(string filePath)
    {
        var path = $"{Application.streamingAssetsPath}/{filePath}";
#if UNITY_ANDROID && !UNITY_EDITOR
        UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(path);
        www.SendWebRequest();
        while (!www.isDone) { }
        return www.downloadHandler.text;
#else
        return File.ReadAllText(path);
#endif
    }
}