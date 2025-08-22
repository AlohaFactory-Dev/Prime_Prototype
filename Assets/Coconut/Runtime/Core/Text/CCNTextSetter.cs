using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Aloha.Coconut
{
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class CCNTextSetter : MonoBehaviour
    {
        [ValueDropdown("@TextTableV2.GetKeys()"), SerializeField]
        private string key;

        [SerializeField] private FontType fontType;

        [SerializeField] private List<TextTableV2.Param> parameters = new List<TextTableV2.Param>();

        [NonSerialized] private TMP_Text _text;
        private bool _onSetText;

        void OnEnable()
        {
            SetTMP();
            TextTableV2.OnLanguageChanged += UpdateFontAndText;
        }

        private void SetTMP()
        {
            if (!_onSetText)
            {
                _onSetText = true;
                _text = GetComponent<TMP_Text>();
            }
        }

        private void UpdateFontAndText(SystemLanguage _)
        {
            UpdateFont();
            UpdateText();
        }

        public void SetText(string group, string key)
        {
            this.key = $"{group}/{key}";
            UpdateText();
        }

        public void SetFontType(FontType fontType)
        {
            this.fontType = fontType;
            UpdateFont();
        }

        private void UpdateText()
        {
            if (!string.IsNullOrEmpty(key))
            {
                SetTMP();
                _text.text = TextTableV2.Get(key, parameters.ToArray());
            }
        }

        private void UpdateFont()
        {
            (_text.font, _text.fontSharedMaterial) = TextTableV2.GetFontAndMaterial(fontType);
        }

        public void SetParam(string key, string value)
        {
            var paramIndex = parameters.FindIndex(p => p.key == key);
            if (paramIndex > 0)
            {
                parameters[paramIndex] = new TextTableV2.Param(key, value);
            }
            else
            {
                parameters.Add(new TextTableV2.Param(key, value));
            }

            UpdateText();
        }

        private void OnDisable()
        {
            TextTableV2.OnLanguageChanged -= UpdateFontAndText;
        }

        private void OnValidate()
        {
            if (_text != null)
            {
                UpdateText();
                UpdateFont();
            }
        }

#if UNITY_EDITOR
        [BoxGroup("Editor")]
        [PropertyOrder(0)]
        [ShowInInspector, NonSerialized, ValueDropdown("SupportedLanguages")]
        private SystemLanguage _testLanguage = SystemLanguage.English;

        [BoxGroup("Editor")]
        [PropertyOrder(1)]
        [Button]
        public void SelectLanguage()
        {
            TextTableV2.ChangeLanguage(_testLanguage);
        }

        private IEnumerable<SystemLanguage> SupportedLanguages()
        {
            return TextTableV2.SupportedLanguages;
        }

        [BoxGroup("Editor")]
        [PropertyOrder(2)]
        [Button]
        public void RefreshTable()
        {
            TextTableV2.RefreshTable();
            UnityEditor.EditorUtility.DisplayDialog("Refresh Table", "Text Table Refreshed", "OK");
            UpdateText();
        }

        [BoxGroup("Editor")]
        [PropertyOrder(3)]
        [Button]
        public void SelectConfig()
        {
            UnityEditor.Selection.activeObject = CoconutConfig.Get<TextTableConfig>();
        }
#endif
    }
}