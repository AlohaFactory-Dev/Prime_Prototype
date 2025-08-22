using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Aloha.Coconut.UI
{
    [RequireComponent(typeof(Button))]
    public class StyledButton : StyledSpriteComponent<Button>
    {
        [SerializeField, ValueDropdown(nameof(GetSpriteKeys))] private string normalKey;
        [SerializeField, ValueDropdown(nameof(GetSpriteKeys))] private string disabledKey;
        
        protected override void Fetch()
        {
            target.transition = Selectable.Transition.SpriteSwap;
            ((Image)target.targetGraphic).sprite = GetSprite(normalKey);
            
            var spriteState = target.spriteState;
            spriteState.disabledSprite = GetSprite(disabledKey);
            target.spriteState = spriteState;
        }
    }
}