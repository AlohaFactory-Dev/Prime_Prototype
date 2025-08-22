using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

[InitializeOnLoad]
public static class AssemblyReloadLocker
{
    public static bool IsLocked => _isLocked;
    private static bool _isLocked;

    static class ToolbarStyles
    {
        public static readonly GUIStyle commandButtonStyle;

        static ToolbarStyles()
        {
            commandButtonStyle = new GUIStyle("Command")
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageAbove,
                fixedWidth = 160
            };
        }
    }

    static AssemblyReloadLocker()
    {
        _isLocked = false;
        ToolbarExtender.LeftToolbarGUI.Insert(0, OnRightToolbarGUI);
    }

    [MenuItem("Aloha/Toggle AssemblyReload Lock %r")]
    public static void ToggleLock()
    {
        _isLocked = !_isLocked;
        if (_isLocked) EditorApplication.LockReloadAssemblies();
        else EditorApplication.UnlockReloadAssemblies();
    }

    private static void OnRightToolbarGUI()
    {
        var prevIsLocked = _isLocked;
        _isLocked = GUILayout.Toggle(_isLocked,
            new GUIContent("Lock AssemblyReload", "Lock reloading assembly (Ctrl+R)"),
            ToolbarStyles.commandButtonStyle);
        if (_isLocked != prevIsLocked)
        {
            if (_isLocked) EditorApplication.LockReloadAssemblies();
            else EditorApplication.UnlockReloadAssemblies();
        }

        GUILayout.FlexibleSpace();
    }
}