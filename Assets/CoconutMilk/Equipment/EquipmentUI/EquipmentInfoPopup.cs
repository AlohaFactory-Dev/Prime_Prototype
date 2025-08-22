using System;
using System.Collections;
using System.Collections.Generic;
using Aloha.Coconut;
using Aloha.Coconut.UI;
using Aloha.CoconutMilk;
using CoconutMilk.Equipments;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class EquipmentInfoPopup : UISlice, IDimClosable
{
    public class Args : UIOpenArgs
    {
        public EquipmentIconState State { get; set; }
        public Equipment Equipment { get; set; }
    }

    [Inject] private PropertyIconPool _propertyIconPool;
    [Inject] private EquipmentSystem _equipmentSystem;
    [Inject] private PropertyManager _propertyManager;

    [SerializeField] private RectTransform propertyIconParent;
    [SerializeField] private GameObject buttonGroup;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button unequipButton;
    [SerializeField] private Button levelUpButton;
    [SerializeField] private Button batchUpgradeButton;
    [SerializeField] private TextMeshProUGUI levelUpCostText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI partTypeText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statText;
    [SerializeField] private Image requiredIcon;
    [SerializeField] private Sprite stoneIcon;
    [SerializeField] private Sprite scrollIcon;

    private EquipmentOptionViewer _equipmentOptionViewer;
    private EquipmentStatViewer _equipmentStatViewer;
    private bool _isInitialized;
    private Args _equipmentArgs;
    private EquipmentSlot _equipmentSlot;
    private IDisposable _levelUpSubscription;
    private EquipmentIcon _equipmentIcon;
    private LayoutGroup[] _layoutGroups;

    private static readonly string[] PartTypeTexts = { "무기", "방어구", "반지", "장신구", "Unknown" };


    protected override void Open(UIOpenArgs args)
    {
        Init();
        base.Open(args);
        _equipmentArgs = args as Args;
        var equipment = _equipmentArgs.Equipment;

        _equipmentSlot = _equipmentSystem.GetSlot(equipment.Type.Part);
        SetStateButton();
        _equipmentOptionViewer.Open(equipment);
        Refresh();
        _levelUpSubscription = _equipmentSlot.OnLevelUp.Subscribe(_ => { Refresh(); });
        SetPartTypeText(equipment.Type.Part);

        // 등급이름
        rarityText.text = equipment.Type.RarityData.rarity.ToString();
        nameText.text = "장비 이름";

        _equipmentIcon?.Remove();
        _equipmentIcon = _propertyIconPool.Get(equipment, propertyIconParent, propertyIconParent.sizeDelta);
        StartCoroutine(RebuildLayout());
    }

    private IEnumerator RebuildLayout()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return null;
            foreach (var layoutGroup in _layoutGroups)
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layoutGroup.transform);
        }
    }

    private void Init()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        _layoutGroups = GetComponentsInChildren<LayoutGroup>(true);
        _equipmentOptionViewer = GetComponentInChildren<EquipmentOptionViewer>(true);
        _equipmentStatViewer = GetComponentInChildren<EquipmentStatViewer>(true);

        _equipmentOptionViewer.Init();
        _equipmentStatViewer.Init();

        levelUpButton.onClick.AddListener(OnClickLevelUp);
        batchUpgradeButton.onClick.AddListener(OnClickBatchLevelUp);
        unequipButton.onClick.AddListener(OnClickUnequip);
        equipButton.onClick.AddListener(OnClickEquip);
    }

    private void OnClickLevelUp()
    {
        _equipmentSystem.LevelUp(_equipmentArgs.Equipment);
    }

    private void OnClickBatchLevelUp()
    {
        _equipmentSystem.BatchLevelUp(_equipmentArgs.Equipment);
    }

    private void OnClickUnequip()
    {
        _equipmentSystem.Unequip(_equipmentArgs.Equipment.Type.Part, _equipmentArgs.Equipment.UID);
        CloseView();
    }

    private void OnClickEquip()
    {
        _equipmentSystem.Equip(_equipmentArgs.Equipment);
        CloseView();
    }

    private void SetPartTypeText(EquipmentPart part)
    {
        int idx = (int)part;
        partTypeText.text = idx >= 0 && idx < PartTypeTexts.Length ? PartTypeTexts[idx] : PartTypeTexts[^1];
    }

    private void SetStateButton()
    {
        bool isAllOff = _equipmentArgs.State == EquipmentIconState.ETC;
        buttonGroup.SetActive(!isAllOff);
        if (isAllOff) return;

        bool isInventory = _equipmentArgs.State == EquipmentIconState.Inventory;
        unequipButton.gameObject.SetActive(!isInventory);
        equipButton.gameObject.SetActive(isInventory);
    }

    private void Refresh()
    {
        _equipmentStatViewer.Refresh(_equipmentArgs.Equipment);
        var property = _equipmentSlot.GetLevelUpCost();
        var balance = _propertyManager.GetBalance(property.type);

        if (property.type.Alias == PropertyTypeAlias.EquipmentStone)
            requiredIcon.sprite = stoneIcon;
        else if (property.type.Alias == PropertyTypeAlias.EquipmentScroll)
            requiredIcon.sprite = scrollIcon;

        bool affordable = _equipmentSlot.IsLevelUpAffordable();
        levelUpCostText.text = affordable
            ? $"{balance}/{property.amount}"
            : $"<color=#FF0000>{balance}</color>/{property.amount}";

        levelText.text = $"{_equipmentSlot.Level}/{_equipmentSlot.MaxLevel}";
    }


    protected override void OnClose()
    {
        _levelUpSubscription?.Dispose();
        base.OnClose();
    }

    public void CloseByDim() => CloseView();
}