using Aloha.Coconut.UI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIViewConfig))]
public class UIViewConfigInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Set Addressable"))
        {
            target.SetAddressableGroup("UIViews", target.name);
        }

        if (GUILayout.Button("Preview"))
        {
            ((UIViewConfig)target).Preview();
        }
        
        if (GUILayout.Button("Clear"))
        {
            ((UIViewConfig)target).ClearPreview();
        }
    }
}
