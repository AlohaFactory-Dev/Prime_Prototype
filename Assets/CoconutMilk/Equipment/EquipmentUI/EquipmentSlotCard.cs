using System;
using Aloha.Coconut;
using Aloha.CoconutMilk;
using CoconutMilk.Equipments;
using UniRx;
using UnityEngine;
using Zenject;

public class EquipmentSlotCard : MonoBehaviour
{
    [SerializeField] private EquipmentPart part;
    [SerializeField] private RectTransform content;
    [SerializeField] private int index = 0;
    private EquipmentIcon _equippedIcon;
    private EquipmentSlot EquipmentSlot => _equipmentSystem.GetSlot(part, index);
    [Inject] private EquipmentSystem _equipmentSystem;
    [Inject] private PropertyIconPool _propertyIconPool;

    public void Init()
    {
        var slot = EquipmentSlot;
        if (slot.EquippedEquipment != null)
        {
            var equipment = slot.EquippedEquipment;
            _equippedIcon = _propertyIconPool.Get(equipment, content, content.sizeDelta, EquipmentIconState.Slot);
        }

        _equipmentSystem.OnEquipStateUpdated.Subscribe(x =>
        {
            if (x.Part != part || x.Index != index) return;
            _equippedIcon = _propertyIconPool.Get(EquipmentSlot.EquippedEquipment, content, content.sizeDelta, EquipmentIconState.Slot);
        }).AddTo(this);

        _equipmentSystem.OnUnequipStateUpdated.Subscribe(x =>
        {
            if (x.Part != part || x.Index != index) return;
            _equippedIcon.Remove();
        }).AddTo(this);
    }
}