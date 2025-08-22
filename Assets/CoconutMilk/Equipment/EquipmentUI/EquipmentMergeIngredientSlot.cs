using System;
using Aloha.CoconutMilk;
using CoconutMilk.Equipments;
using UnityEngine;
using Zenject;

public class EquipmentMergeIngredientSlot : MonoBehaviour
{
    public EquipmentIcon EquipmentIcon => _originalIcon;
    public bool IsEmpty => _equipmentIcon == null;
    public bool IsOff => _previewIcon == null;
    [SerializeField] private RectTransform bgRectTransform;
    [Inject] private PropertyIconPool _propertyIconPool;
    private EquipmentIcon _previewIcon;
    private EquipmentIcon _equipmentIcon;
    private EquipmentIcon _originalIcon;
    private Action _originalOnClickAction;
    private Action _checkAllSlotFill;
    private Vector2 _sizeDelta;


    public void Init(Action checkAllSlotFill)
    {
        _checkAllSlotFill = checkAllSlotFill;
        _sizeDelta = ((RectTransform)transform).sizeDelta;
    }

    public void Off()
    {
        gameObject.SetActive(false);
    }

    public void On(Equipment equipment)
    {
        gameObject.SetActive(true);
        _previewIcon = _propertyIconPool.Get(equipment, bgRectTransform, _sizeDelta);
        _previewIcon.OffLevelText();
    }

    public void SetEquipmentIcon(EquipmentIcon equipmentIcon)
    {
        _originalIcon = equipmentIcon;
        _originalOnClickAction = _originalIcon.ButtonAction;
        _originalIcon.SetCustomOnClickAction(OnClickAction);

        _equipmentIcon = _propertyIconPool.Get(equipmentIcon.Equipment, (RectTransform)transform, _sizeDelta);
        _equipmentIcon.SetCustomOnClickAction(OnClickAction);
        _equipmentIcon.OffLevelText();
    }

    public void Refresh()
    {
        _previewIcon?.Remove();
        _equipmentIcon?.Remove();
        _equipmentIcon = null;
        _previewIcon = null;

        _originalIcon?.OffCheck();
        _originalIcon?.SetCustomOnClickAction(_originalOnClickAction);
        _originalIcon = null;
        Off();
    }

    private void OnClickAction()
    {
        _originalIcon?.SetCustomOnClickAction(_originalOnClickAction);
        _originalIcon?.OffCheck();

        _equipmentIcon?.Remove();
        _equipmentIcon = null;
        _checkAllSlotFill.Invoke();
    }
}