using System.IO;
using System.Reflection;
using Aloha.Coconut;
using UnityEditor;
using UnityEngine;
using Event = UnityEngine.Event;

public class DebugConfigEditor : EditorWindow
{
    public static DebugConfig DebugConfig => _debugConfigInstance;
    private static DebugConfig _debugConfigInstance;

    [MenuItem("Coconut/Debug Config _F1")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(DebugConfigEditor), false, "Debug Config");
    }

    private void OnGUI()
    {
        LoadGameConfigInstance();

        var isDirty = false;

        //Reflection을 사용해 필드 값들을 자동으로 에디터에 표시
        //TODO:: bool이 아닌 다른 type의 값을 Config하게 되면 해당 type 에디터 추가, 미리부터 하지는 말기
        var filedInfos = typeof(DebugConfig).GetFields();
        foreach (FieldInfo fieldInfo in filedInfos)
        {
            if (fieldInfo.FieldType == typeof(bool))
            {
                var fieldValue = (bool)fieldInfo.GetValue(_debugConfigInstance);
                var prevValue = fieldValue;
                fieldValue = EditorGUILayout.Toggle(fieldInfo.Name, fieldValue);
                if (prevValue != fieldValue)
                {
                    fieldInfo.SetValue(_debugConfigInstance, fieldValue);
                    isDirty = true;
                }
            }
            else if (fieldInfo.FieldType == typeof(int))
            {
                var fieldValue = (int)fieldInfo.GetValue(_debugConfigInstance);
                var prevValue = fieldValue;
                fieldValue = EditorGUILayout.IntField(fieldInfo.Name, fieldValue);
                if (prevValue != fieldValue)
                {
                    fieldInfo.SetValue(_debugConfigInstance, fieldValue);
                    isDirty = true;
                }
            }
            else if (fieldInfo.FieldType == typeof(float))
            {
                var fieldValue = (float)fieldInfo.GetValue(_debugConfigInstance);
                var prevValue = fieldValue;
                fieldValue = EditorGUILayout.FloatField(fieldInfo.Name, fieldValue);
                if (prevValue != fieldValue)
                {
                    fieldInfo.SetValue(_debugConfigInstance, fieldValue);
                    isDirty = true;
                }
            }
        }

        if (isDirty) Save();

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
        {
            Close();
        }
    }

    public static void LoadGameConfigInstance()
    {
        if (_debugConfigInstance == null)
        {
            var path = Path.Combine(Application.streamingAssetsPath, DebugConfig.PATH);
            
            if (File.Exists(path))
            {
                _debugConfigInstance = JsonUtility.FromJson<DebugConfig>(File.ReadAllText(path));
            }
            else
            {
                _debugConfigInstance = new DebugConfig();
            }
        }
    }

    public static void Save()
    {
        var json = JsonUtility.ToJson(_debugConfigInstance);
        var path = Path.Combine(Application.streamingAssetsPath, DebugConfig.PATH);
        
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
    }

    public static void SetDebugFlag(bool isDebug)
    {
        LoadGameConfigInstance();
        _debugConfigInstance.useDebug = isDebug;
    }

    public static void SetTutorialFlag(bool tutorialOn)
    {
        LoadGameConfigInstance();
        _debugConfigInstance.tutorialOn = tutorialOn;
    }
    
    public static void SetDevServerFlag(bool useDevServer)
    {
        LoadGameConfigInstance();
        _debugConfigInstance.useDevServer = useDevServer;
    }
}