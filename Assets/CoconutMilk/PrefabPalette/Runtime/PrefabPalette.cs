using System;
using System.Collections.Generic;
using Aloha.Coconut;
using Sirenix.OdinInspector;
using UnityEngine;
using Event = UnityEngine.Event;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor.SceneManagement;

namespace Aloha.CoconutMilk
{
    [CreateAssetMenu(fileName = "PrefabPalette", menuName = "Coconut/Config/PrefabPalette")]
    public class PrefabPalette : CoconutConfig
    {
        public List<Group> prefabGroups;
    
        [Header("UIComponents")]
        public PrefabButton coconutCanvas;
        public PrefabButton button;
        public PrefabButton text;
        public PrefabButton dim;
        public PrefabButton safeArea;
        public PrefabButton popupWindow;
        public PrefabButton barGauge;

        [Serializable]
        public struct Group
        {
            [HideLabel] public string name;
            public List<PrefabButton> prefabs;
        }

        [Serializable]
        public struct PrefabButton
        {
            [HideLabel, InlineButton("Instantiate")]
            public GameObject prefab;

            public void Instantiate()
            {
#if UNITY_EDITOR
                // generate under selected transform
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                instance.name = prefab.name;
                if (Selection.activeTransform == null && PrefabStageUtility.GetCurrentPrefabStage() != null)
                {
                    instance.transform.SetParent(PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot.transform, false);
                }
                else
                {
                    instance.transform.SetParent(Selection.activeTransform, false);   
                }
                EditorGUIUtility.PingObject(instance);
                Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);
#endif
            }
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/Open PrefabPalette _F3", false, -1)]
        public static void ShowWindow()
        {
            var window =
                OdinEditorWindow.InspectObject(Get<PrefabPalette>());
            window.position = GUIHelper.GetEditorWindowRect().AlignRight(500, true);

            // add close on escape
            window.OnBeginGUI += () =>
            {
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
                {
                    window.Close();
                    Event.current.Use();
                }
            };
        }
#endif
    }
}
#endif