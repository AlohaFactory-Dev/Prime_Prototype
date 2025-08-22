using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Aloha.Coconut.UI
{
    [RequireComponent(typeof(Image))]
    public class StyledImage : StyledSpriteComponent<Image>
    {
        [SerializeField, ValueDropdown(nameof(GetSpriteKeys))] private string key;

        protected override void Fetch()
        {
            target.sprite = GetSprite(key);
        }
    }
}