using System.Collections.Generic;
using System.Linq;
using Aloha.CoconutMilk;
using CoconutMilk.Equipments;
using Sirenix.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;


public class EquipmentInventory : MonoBehaviour
{
    [Inject] private EquipmentSystem _equipmentSystem;
    [Inject] private PropertyIconPool _propertyIconPool;
    [Inject] private EquipmentInventoryFilterManager _equipmentInventoryFilterManager;
    [SerializeField] private RectTransform content;

    private readonly Dictionary<Equipment, EquipmentIcon> _equipments = new();
    [SerializeField] private EquipmentIconState iconState;
    private EquipmentMergeMoveIconPanel _moveIconPanel;


    public void Init()
    {
        Refresh();
        if (iconState == EquipmentIconState.Inventory)
        {
            _equipmentSystem.Inventory.ObserveAdd().Subscribe(x => AddEquipment(x.Value)).AddTo(this);
            _equipmentSystem.Inventory.ObserveRemove().Subscribe(x => RemoveEquipment(x.Value)).AddTo(this);
            _equipmentSystem.OnEquipStateUpdated.Subscribe(x =>
            {
                RemoveEquipment(x.EquippedEquipment);
                ApplyOrderFilter();
            }).AddTo(this);
            _equipmentSystem.OnUnequipStateUpdated.Subscribe(x =>
            {
                AddEquipment(x.EquippedEquipment);
                ApplyOrderFilter();
            }).AddTo(this);
            _equipmentInventoryFilterManager.CurrentOrderFilterType.Subscribe(_ => { ApplyOrderFilter(); }).AddTo(this);
        }
        else
        {
            _equipmentInventoryFilterManager.CurrentPartFilterType.Subscribe(_ => { ApplyPartFilter(); }).AddTo(this);
            _equipmentInventoryFilterManager.OnMergePartFilter.Subscribe(ApplyMergeFilter).AddTo(this);
        }
    }

    private void Refresh()
    {
        foreach (var equipment in _equipmentSystem.Inventory)
        {
            if (iconState == EquipmentIconState.Inventory)
            {
                if (!equipment.IsEquipped)
                    AddEquipment(equipment);
            }
            else
            {
                AddEquipment(equipment);
                ApplyPartFilter();
            }
        }
    }

    public void Init(EquipmentMergeMoveIconPanel moveIconPanel)
    {
        Init();
        _moveIconPanel = moveIconPanel;
    }


    private void AddEquipment(Equipment equipment)
    {
        var icon = _propertyIconPool.Get(equipment, content, iconState);
        _equipments[equipment] = icon;
        icon.transform.localScale = Vector3.one;
        icon.transform.localPosition = Vector3.zero;

        if (iconState == EquipmentIconState.Merge)
        {
            icon.SetCustomOnClickAction(() => { _moveIconPanel.MoveIcon(icon); });
        }

        if (iconState == EquipmentIconState.Inventory)
        {
            //TODO : Inventory 아이콘에 맞는 레드닷 설정
            // icon.SetReddot("Equipment/Inventory/" + equipment.Type.EquipmentTypeTableData.id);
        }
        else if (iconState == EquipmentIconState.Merge)
        {
            //TODO : Merge 아이콘에 맞는 레드닷 설정
            // icon.SetReddot("Equipment/Inventory/CanMerge/" + equipment.Type.EquipmentTypeTableData.id);
        }
    }

    private void RemoveEquipment(Equipment equipment)
    {
        var icon = _equipments[equipment];
        _propertyIconPool.Remove(icon);
        _equipments.Remove(equipment);
    }

    private void ApplyOrderFilter()
    {
        // 현재 _equipments의 key(Equipment) 리스트를 필터 타입에 따라 정렬
        List<Equipment> sorted;
        switch (_equipmentInventoryFilterManager.CurrentOrderFilterType.Value)
        {
            case OrderFilterType.Rarity:
                sorted = _equipmentInventoryFilterManager.SortByRarity(_equipments.Keys);
                break;
            case OrderFilterType.Part:
                sorted = _equipmentInventoryFilterManager.SortByPart(_equipments.Keys);
                break;
            default:
                sorted = new List<Equipment>(_equipments.Keys);
                break;
        }

        // 정렬된 순서대로 아이콘의 SiblingIndex를 재설정
        for (int i = 0; i < sorted.Count; i++)
        {
            var icon = _equipments[sorted[i]];
            icon.transform.SetSiblingIndex(i);
        }
    }

    private void ApplyPartFilter()
    {
        var partType = _equipmentInventoryFilterManager.CurrentPartFilterType.Value;

        // 파트 필터에 맞는 장비만 활성화, 나머지는 비활성화
        var filtered = _equipments.Keys
            .Where(e => partType == PartFilterType.All || (PartFilterType)e.Type.Part == partType)
            .ToList();

        foreach (var kvp in _equipments)
        {
            kvp.Value.gameObject.SetActive(filtered.Contains(kvp.Key));
        }

        // 활성화된 아이콘만 등급순으로 정렬
        var sorted = _equipmentInventoryFilterManager.SortByRarity(filtered);
        for (int i = 0; i < sorted.Count; i++)
        {
            _equipments[sorted[i]].transform.SetSiblingIndex(i);
            _equipments[sorted[i]].Unlock();
        }
    }

    private void ApplyMergeFilter(EquipmentIcon equipmentIcon)
    {
        if (!equipmentIcon)
        {
            _equipments.Values.ForEach(icon => icon.Remove());
            _equipments.Clear();
            Refresh();
            return;
        }


        // 파트 필터에 맞는 장비만 활성화, 나머지는 비활성화
        var filtered = _equipments.Keys
            .Where(e => e.Type.Part == equipmentIcon.Equipment.Type.Part)
            .ToList();

        foreach (var kvp in _equipments)
        {
            kvp.Value.gameObject.SetActive(filtered.Contains(kvp.Key));
        }

        // 활성화된 아이콘만 등급순으로 정렬
        var sorted = _equipmentInventoryFilterManager.SortByRarity(filtered);
        for (int i = 0; i < sorted.Count; i++)
        {
            _equipments[sorted[i]].transform.SetSiblingIndex(i);
            if (!equipmentIcon.Equipment.IsIngredientOf(sorted[i]))
            {
                _equipments[sorted[i]].Lock();
            }
        }

        equipmentIcon.Unlock();
    }
}