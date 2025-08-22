using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Aloha.Coconut.UI
{
    [CreateAssetMenu(menuName = "Coconut/UI/StyleSheet")]
    public class StyleSheet : ScriptableObject
    {
        public static StyleSheet Instance
        {
            get
            {
                if (_instance == null)
                {
                    Load(Addressables.LoadAssetAsync<StyleSheet>("CoconutStyleSheet").WaitForCompletion());
                }
                if (_instance == null)
                {
                    Debug.LogError("CoconutStyleSheet Address를 가진 StyleSheet 에셋이 없습니다.");
                }
                return _instance;
            }
        }
        private static StyleSheet _instance;

        public static void Load(StyleSheet styleSheet)
        {
            _instance = styleSheet;
        }
        
        [Serializable]
        private struct SpriteEntry
        {
            [HorizontalGroup("Sprite", Width = 50), HideLabel, PreviewField]
            public Sprite sprite;
            
            [HorizontalGroup("Sprite"), HideLabel]
            public string key;
        }
    
        [SerializeField] private SpriteEntry[] sprites;
    
        public Sprite GetSprite(string key)
        {
            foreach (var entry in sprites)
            {
                if (entry.key == key) return entry.sprite;
            }

            return null;
        }
        
        public List<string> GetSpriteKeys()
        {
            var keys = new List<string>();
            foreach (var entry in sprites)
            {
                keys.Add(entry.key);
            }

            return keys;
        }
    }   
}