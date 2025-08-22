using System.Collections.Generic;
using UnityEngine;

namespace Aloha.Coconut.UI
{
    public abstract class StyledSpriteComponent<T> : StyledComponent<T> where T: Component
    {
        protected Sprite GetSprite(string key)
        {
            return GetStyleSheet()?.GetSprite(key);
        }
        
        // key 선택에서 ValueDropdown(nameof(GetSpriteKeys)을 사용할 수 있도록 protected 처리
        protected IEnumerable<string> GetSpriteKeys()
        {
            var styleSheet = GetStyleSheet();
            if(styleSheet == null) return new string[0];
            
            return styleSheet.GetSpriteKeys();
        }
    }
}