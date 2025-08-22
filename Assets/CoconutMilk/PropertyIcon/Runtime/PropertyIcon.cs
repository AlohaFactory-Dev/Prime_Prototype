using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Aloha.Coconut;

namespace Aloha.CoconutMilk
{
    public class PropertyIcon : MonoBehaviour
    {
        public Property Property => _property;
    
        [SerializeField] private Image iconImage;
        [SerializeField] private Image frameImage;
        [SerializeField] private TextMeshProUGUI amountText;

        private Property _property;
        private AsyncOperationHandle<Sprite> _iconSpriteLoaded;

        private PropertyIconPool _pool;

        public virtual void Set(Property property)
        {
            _property = property;
        
            _iconSpriteLoaded = Addressables.LoadAssetAsync<Sprite>(property.type.iconPath);
            iconImage.sprite = _iconSpriteLoaded.WaitForCompletion();

            frameImage.sprite = _pool.Config.GetFrame(property.type.rarity);
            amountText.text = property.amount.ToString();
        }

        public void Set(Property property, Vector2 size)
        {
            Set(property);
            ((RectTransform)transform).sizeDelta = size;
        }

        public void Set(Property property, int sizeX, int sizeY)
        {
            Set(property, new Vector2(sizeX, sizeY));
        }

        public void LinkPool(PropertyIconPool pool)
        {
            _pool = pool;
        }

        public virtual void Clear()
        {
            ((RectTransform)transform).sizeDelta = new Vector2(170, 170);
            if(_iconSpriteLoaded.IsValid()) Addressables.Release(_iconSpriteLoaded);
        }

        public virtual void Remove()
        {
            _pool.Remove(this);
        }
    }
}