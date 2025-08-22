using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Aloha.Coconut;
using CoconutMilk.Equipments;

namespace Aloha.CoconutMilk
{
    public class PropertyIconPool
    {
        // 필드
        private readonly DiContainer _container;
        private readonly EquipmentSystem _equipmentSystem;
        private readonly Dictionary<PropertyIcon, Queue<PropertyIcon>> _pools = new();
        private readonly Transform _poolParent;
        private const float DefaultIconSize = 160f;

        // 프로퍼티
        public PropertyIconConfig Config { get; }

        private Vector2 DefaultSize => new(DefaultIconSize, DefaultIconSize);

        // 생성자
        public PropertyIconPool(DiContainer container, EquipmentSystem equipmentSystem)
        {
            _container = container;
            _equipmentSystem = equipmentSystem;

            _poolParent = new GameObject("PropertyIconPool").transform;
            _poolParent.gameObject.SetActive(false);
            Object.DontDestroyOnLoad(_poolParent.gameObject);

            Config = CoconutConfig.Get<PropertyIconConfig>();
        }

        // Property 타입 아이콘 반환
        public PropertyIcon Get(Property property, RectTransform parent)
        {
            if (property.type.group == PropertyTypeGroup.Equipment)
                return Get(_equipmentSystem.CreateEquipment(property), parent);

            return Get(property, parent, DefaultSize);
        }

        // Property 타입 아이콘 반환 (사이즈 지정)
        private PropertyIcon Get(Property property, RectTransform parent, Vector2 size)
        {
            var prefab = Config.GetPropertyIconPrefab(property.type.group);
            var icon = GetOrCreateIcon<PropertyIcon>(prefab, parent);
            icon.Set(property, size);
            return icon;
        }

        // Equipment 타입 아이콘 반환 (기본 사이즈)
        public EquipmentIcon Get(Equipment equipment, RectTransform parent, EquipmentIconState state = EquipmentIconState.ETC)
        {
            return Get(equipment, parent, DefaultSize, state);
        }

        // Equipment 타입 아이콘 반환 (사이즈 지정)
        public EquipmentIcon Get(Equipment equipment, RectTransform parent, Vector2 size, EquipmentIconState state = EquipmentIconState.ETC)
        {
            var property = new Property(equipment.ToPropertyType(), 1);
            var prefab = Config.GetPropertyIconPrefab(property.type.group);
            var icon = GetOrCreateIcon<EquipmentIcon>(prefab, parent);
            icon.SetEquipmentIcon(equipment, property, size, state);
            return icon;
        }

        // 아이콘 반환 및 풀에서 꺼내기/생성
        private T GetOrCreateIcon<T>(PropertyIcon prefab, RectTransform parent) where T : PropertyIcon
        {
            if (!_pools.ContainsKey(prefab))
                _pools[prefab] = new Queue<PropertyIcon>();

            var pool = _pools[prefab];
            if (pool.Count == 0)
            {
                var newIcon = _container.InstantiatePrefab(prefab, _poolParent).GetComponent<T>();
                newIcon.LinkPool(this);
                newIcon.transform.localScale = Vector3.one;
                pool.Enqueue(newIcon);
            }

            var icon = (T)pool.Dequeue();
            icon.transform.SetParent(parent);
            icon.transform.localScale = Vector3.one;
            icon.transform.localPosition = Vector3.zero;
            return icon;
        }

        // 아이콘 반환(풀에 다시 넣기)
        public void Remove(PropertyIcon propertyIcon)
        {
            propertyIcon.Clear();
            propertyIcon.gameObject.SetActive(true);
            propertyIcon.transform.SetParent(_poolParent);
            var prefab = Config.GetPropertyIconPrefab(propertyIcon.Property.type.group);
            _pools[prefab].Enqueue(propertyIcon);
        }
    }
}