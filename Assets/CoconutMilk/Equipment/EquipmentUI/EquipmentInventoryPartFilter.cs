using System;
using _Scripts.Popup.LobbyPopup.Equipment;
using Aloha.Coconut;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


public class EquipmentInventoryPartFilter : MonoBehaviour
{
    [Serializable]
    public struct PartType
    {
        public PartFilterType Type;
        public Sprite Icon;
    }

    [SerializeField] private GameObject filterPanel;
    [SerializeField] private Image filterArrow;
    [SerializeField] private Image filterIcon;
    [SerializeField] private PartType[] partTypes;
    [Inject] private EquipmentInventoryFilterManager _equipmentInventoryFilterManager;

    private readonly PartFilterType _defaultOrderFilterType = PartFilterType.All;
    private PartFilterType _currentFilterType;
    private bool _isOpened = false;


    public void Init()
    {
        var cards = filterPanel.GetComponentsInChildren<EquipmentInventoryPartFilterCard>();
        foreach (var card in cards)
        {
            card.Init(SetFilter);
        }

        _equipmentInventoryFilterManager.OnMergePartFilter.Subscribe(icon =>
        {
            if (icon)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
            }
        });

        SetFilter(_defaultOrderFilterType);
    }

    public void Show()
    {
        SetFilter(_defaultOrderFilterType);
    }

    public void OpenFilterPanel()
    {
        if (_isOpened)
        {
            filterPanel.SetActive(false);
            filterArrow.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            filterPanel.SetActive(true);
            filterArrow.transform.rotation = Quaternion.Euler(0, 0, 180);
        }

        _isOpened = !_isOpened;
    }


    private void SetFilter(PartFilterType filterType)
    {
        _currentFilterType = filterType;
        filterIcon.sprite = Array.Find(partTypes, partType => partType.Type == filterType).Icon;
        _equipmentInventoryFilterManager.CurrentPartFilterType.SetValueAndForceNotify(_currentFilterType);
        CloseFilterPanel();
    }

    private void CloseFilterPanel()
    {
        _isOpened = false;
        filterPanel.SetActive(false);
        filterArrow.transform.rotation = Quaternion.Euler(0, 0, 0);
    }
}