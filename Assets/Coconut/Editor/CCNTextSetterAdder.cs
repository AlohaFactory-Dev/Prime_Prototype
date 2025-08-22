using UnityEditor;

namespace Aloha.Coconut.Editor
{
    public static class CCNTextSetterAdder
    {
        [MenuItem("CONTEXT/TMP_Text/Add CCN Text Setter")]
        public static void AddCCNTextSetterToTMPText(MenuCommand command)
        {
            var text = command.context as TMPro.TMP_Text;
            if (text == null) return;

            var ccnTextSetter = text.gameObject.AddComponent<Aloha.Coconut.CCNTextSetter>();
            EditorUtility.SetDirty(ccnTextSetter);
        }
    }
}