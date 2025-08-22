using System;
using System.Collections.Generic;
using Aloha.Coconut;
using UnityEngine;

namespace Aloha.CoconutMilk
{
    [CreateAssetMenu(fileName = "PropertyIconConfig", menuName = "Coconut/Config/PropertyIconConfig")]
    public class PropertyIconConfig : CoconutConfig
    {
        [Serializable]
        private struct RarityFrame
        {
            public int rarity;
            public Sprite sprite;
        }

        [Serializable]
        private struct PropertyIconVariant
        {
            public PropertyTypeGroup typeGroup;
            public PropertyIcon iconPrefab;
        }
    
        [SerializeField] private List<RarityFrame> rarityFrames;
        [SerializeField] private List<PropertyIconVariant> propertyIconVariants;
        [SerializeField] private PropertyIcon defaultPropertyIconPrefab;

        public Sprite GetFrame(int rarity)
        {
            foreach (var rarityBackground in rarityFrames)
            {
                if (rarityBackground.rarity == rarity)
                {
                    return rarityBackground.sprite;
                }
            }

            return rarityFrames[0].sprite;
        }
    
        public PropertyIcon GetPropertyIconPrefab(PropertyTypeGroup typeGroup)
        {
            foreach (var propertyIconVariant in propertyIconVariants)
            {
                if (propertyIconVariant.typeGroup.HasFlag(typeGroup))
                {
                    return propertyIconVariant.iconPrefab;
                }
            }

            return defaultPropertyIconPrefab;
        }
    }
}