using Aloha.Coconut.Launcher;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;

namespace Aloha.Coconut.Editor
{
    [InitializeOnLoad]
    public static class LauncherScenePlayer
    {
        private const string cEditorPrefPreviousScene = "LauncherScenePlayer.PreviousScene";

        private static string PreviousScene
        {
            get => EditorPrefs.GetString(cEditorPrefPreviousScene, EditorSceneManager.GetActiveScene().path);
            set => EditorPrefs.SetString(cEditorPrefPreviousScene, value);
        }

        static class ToolbarStyles
        {
            public static readonly GUIStyle commandButtonStyle;

            static ToolbarStyles()
            {
                commandButtonStyle = new GUIStyle("Command")
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter,
                    imagePosition = ImagePosition.ImageAbove,
                    fontStyle = FontStyle.Bold
                };
            }
        }

        static LauncherScenePlayer()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnLeftToolbarGUI);
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnLeftToolbarGUI()
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("L", "Start from launcher scene"), ToolbarStyles.commandButtonStyle))
            {
                PlayFromPrelaunchScene();
            }
        }

        // click command-0 to go to the prelaunch scene and then play
        [MenuItem("Edit/Play-Unplay, But From Prelaunch Scene %0")]
        public static void PlayFromPrelaunchScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            PreviousScene = SceneManager.GetActiveScene().path;

            if (AssemblyReloadLocker.IsLocked)
            {
                EditorUtility.DisplayDialog("Assembly Reload Locked", "Please unlock Assembly Reload first", "OK");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(CoconutConfig.Get<LauncherConfig>().launcherScenePath);
                EditorApplication.isPlaying = true;
            }
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode &&
                    !string.IsNullOrWhiteSpace(PreviousScene))
                {
                    if (SceneManager.GetActiveScene().path == PreviousScene) return;
                    // User pressed stop -- reload previous scene.
                    try
                    {
                        EditorSceneManager.OpenScene(PreviousScene);
                        PreviousScene = "";
                    }
                    catch
                    {
                        Debug.LogError($"error: scene not found: {PreviousScene}");
                    }
                }
            }
        }
    }
}