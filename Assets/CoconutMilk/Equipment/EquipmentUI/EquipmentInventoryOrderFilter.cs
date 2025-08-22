using System;
using Aloha.Coconut;
using TMPro;
using UnityEngine;
using Zenject;

public class EquipmentInventoryOrderFilter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI orderFilterText;
    private readonly OrderFilterType _defaultOrderFilterType = OrderFilterType.Rarity;
    [Inject] private EquipmentInventoryFilterManager _equipmentInventoryFilterManager;
    private OrderFilterType _currentFilterType;
    private OrderFilterType[] _orderFilterTypes;


    public void Init()
    {
        _orderFilterTypes = (OrderFilterType[])Enum.GetValues(typeof(OrderFilterType));
        _currentFilterType = _defaultOrderFilterType;
    }

    public void Show()
    {
        SetFilter(_currentFilterType);
    }

// Button을 클릭했을 때 호출되는 메서드
    public void ChangeFilterAction()
    {
        var currentIndex = Array.IndexOf(_orderFilterTypes, _currentFilterType);
        var nextIndex = (currentIndex + 1) % _orderFilterTypes.Length;
        SetFilter(_orderFilterTypes[nextIndex]);
    }

    private void SetFilter(OrderFilterType filterType)
    {
        _currentFilterType = filterType;
        _equipmentInventoryFilterManager.CurrentOrderFilterType.SetValueAndForceNotify(_currentFilterType);
        if (_currentFilterType == OrderFilterType.Rarity)
            orderFilterText.text = "Rarity";
        else if (_currentFilterType == OrderFilterType.Part)
            orderFilterText.text = "Part";
    }
}