using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Aloha.Coconut
{
    [CreateAssetMenu(fileName = "TextTableConfig", menuName = "Coconut/Config/TextTableConfig")]
    public class TextTableConfig : CoconutConfig
    {
        [Serializable]
        private class LanguageInfo
        {
            public SystemLanguage language;
            public List<FontInfo> fontInfos;
        }

        [Serializable]
        private class FontInfo
        {
            public FontType type;
            public TMP_FontAsset fontAsset;
            public Material fontMaterial;
        }
        
        public List<SystemLanguage> SupportedLanguages => languageInfos.ConvertAll(info => info.language);
        
        [VerticalGroup("Tables", order:10)]
        [PropertyOrder(0)]
        public List<string> tableNames = new List<string> { "text_table" };

        [PropertySpace(spaceBefore:20)]
        [VerticalGroup("Language Infos", order:20)]
        [SerializeField] private List<LanguageInfo> languageInfos = new List<LanguageInfo>()
        {
            new LanguageInfo()
            {
                language = SystemLanguage.English,
                fontInfos = new List<FontInfo>()
                {
                    new FontInfo()
                    {
                        type = FontType.Default,
                        fontAsset = null,
                        fontMaterial = null,
                    }
                }
            }
        };

        [PropertySpace(spaceBefore:20)]
        [VerticalGroup("FontType", order:11)]
        [PropertyOrder(0)]
        [SerializeField] private List<string> fontTypes = new List<string>() { "Default" };
        
        public (TMP_FontAsset, Material) GetFontInfo(SystemLanguage languageValue, FontType fontType)
        {
            var languageInfo = languageInfos.Find(info => info.language == languageValue);
            if (languageInfo == null)
            {
                Debug.LogError($"Unsupported language: {languageValue}");
                return (null, null);
            }

            var fontInfo = languageInfo.fontInfos.Find(info => info.type == fontType);
            if (fontInfo == null)
            {
                Debug.LogError($"Unsupported font type: {languageValue}, {fontType}");
                return (languageInfo.fontInfos[0].fontAsset, languageInfo.fontInfos[0].fontMaterial);
            }

            return (fontInfo.fontAsset, fontInfo.fontMaterial);
        }

        public void Reset()
        {
            tableNames = new List<string> { "text_table" };
            languageInfos = new List<LanguageInfo>()
            {
                new LanguageInfo()
                {
                    language = SystemLanguage.English,
                    fontInfos = new List<FontInfo>()
                    {
                        new FontInfo()
                        {
                            type = FontType.Default,
                            fontAsset = null,
                            fontMaterial = null,
                        }
                    }
                }
            };
            fontTypes = new List<string>() { "Default" };
        }
        
#if UNITY_EDITOR
        [VerticalGroup("Tables", order:10)]
        [PropertyOrder(1)]
        [Button]
        public void RefreshTextTable()
        {
            TextTableV2.Initialize();
        }
        
        [InfoBox("fontMaterialTypes를 기반으로 enum 생성")]
        [Button]
        [VerticalGroup("FontType")]
        [PropertyOrder(1)]
        [PropertySpace(0, 20)]
        public void GenerateFontTypeEnums()
        {
            EnumGenerator<string, FontType>.New()
                .SetSources(fontTypes)
                .SetFilePath($"Assets/Coconut/Runtime/Core/Text/{nameof(FontType)}.cs")
                .SetGeneratorClass(nameof(TextTableConfig))
                .SetNameSelector(s => s)
                .SetIntValueSelector(s => s == "Default" ? 0 : StringToIntHash(s))
                .Generate();
        }

        private static int StringToIntHash(string s)
        {
            var hash = 0;
            for (var i = 0; i < s.Length; i++)
            {
                hash = (hash << 5) - hash + s[i];
            }

            return hash;
        }
        
        [BoxGroup("Test Select Language", order:15)]
        public SystemLanguage testLanguage = SystemLanguage.English;
        
        [BoxGroup("Test Select Language", order:15)]
        [Button]
        public void SelectLanguage()
        {
            TextTableV2.ChangeLanguage(testLanguage);
        }

        private bool IsPlaying()
        {
            return Application.isPlaying;
        }
#endif
    }
}
