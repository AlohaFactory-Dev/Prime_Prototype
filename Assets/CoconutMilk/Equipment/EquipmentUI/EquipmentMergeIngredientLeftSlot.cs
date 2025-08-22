using System;
using System.Collections;
using System.Collections.Generic;
using Aloha.CoconutMilk;
using CoconutMilk.Equipments;
using UnityEngine;
using Zenject;

public class EquipmentMergeIngredientLeftSlot : MonoBehaviour
{
    public bool IsEmpty => _equipmentIcon == null;

    [Inject] private PropertyIconPool _propertyIconPool;
    [Inject] private EquipmentInventoryFilterManager _equipmentInventoryFilterManager;
    private Equipment _equipment;
    private EquipmentIcon _equipmentIcon;
    private Action _refreshAction;
    private EquipmentIcon _originalIcon;
    private Action _originalOnClickAction;
    public EquipmentIcon EquipmentIcon => _originalIcon;

    public void Init(Action refreshAction)
    {
        _refreshAction = refreshAction;
    }

    public void Refresh()
    {
        _equipmentIcon?.Remove();
        _equipmentIcon = null;
        _originalIcon?.OffCheck();
        _originalIcon?.SetCustomOnClickAction(_originalOnClickAction);
        _originalIcon = null;
        _equipmentInventoryFilterManager.CurrentPartFilterType.SetValueAndForceNotify(_equipmentInventoryFilterManager.CurrentPartFilterType.Value);
        _equipmentInventoryFilterManager.OnMergePartFilter.SetValueAndForceNotify(null);
    }

    public void SetEquipment(EquipmentIcon equipmentIcon)
    {
        _originalIcon = equipmentIcon;
        _originalOnClickAction = equipmentIcon.ButtonAction;
        _originalIcon.SetCustomOnClickAction(OnClickAction);
        _equipmentIcon = _propertyIconPool.Get(equipmentIcon.Equipment, (RectTransform)transform);
        _equipmentIcon.SetCustomOnClickAction(OnClickAction);
        _equipmentInventoryFilterManager.OnMergePartFilter.SetValueAndForceNotify(equipmentIcon);
    }

    private void OnClickAction()
    {
        _originalIcon.SetCustomOnClickAction(_originalOnClickAction);
        _originalIcon.OffCheck();
        _refreshAction.Invoke();
    }
}