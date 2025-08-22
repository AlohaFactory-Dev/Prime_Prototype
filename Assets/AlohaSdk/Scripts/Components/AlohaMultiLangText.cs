using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Aloha.Sdk
{
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class AlohaMultiLangText : MonoBehaviour
    {
        [Serializable]
        public struct TextPair
        {
            public SystemLanguage language;
            public string text;
        }
    
        [SerializeField] private List<TextPair> texts;
    
        private TMP_Text _text;
        private int _index;
    
        void Start()
        {
            _text = GetComponent<TMP_Text>();
            var systemLanguage = Application.systemLanguage;
        
            _index = texts.FindIndex(pair => pair.language == systemLanguage);
            if(_index >= 0) _text.text = texts[_index].text;
        }
    
        [ContextMenu("Next")]
        public void NextText()
        {
            _index = (_index + 1) % texts.Count;
            _text.text = texts[_index].text;
        }
    }
}