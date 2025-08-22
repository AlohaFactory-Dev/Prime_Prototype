using UnityEditor;

public class SaveDataRemover
{
    [MenuItem("Coconut/Remove Save Data")]
    public static void RemoveSaveData()
    {
        // remove files *.ccn from persistentDataPath
        string[] files = System.IO.Directory.GetFiles(UnityEngine.Application.persistentDataPath, "*.ccn");
        foreach (string file in files)
        {
            System.IO.File.Delete(file);
        }
    }
}
