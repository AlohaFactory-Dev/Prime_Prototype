using System;
using System.Linq;
using Aloha.Coconut;
using Aloha.CoconutMilk;
using CoconutMilk.Equipments;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Aloha.CoconutMilk.EquipmentSample;

public enum EquipmentIconState
{
    Inventory,
    Slot,
    Merge,
    ETC
}

public class EquipmentIcon : PropertyIcon
{
    [Serializable]
    public struct PartIcon
    {
        public EquipmentPart Part;
        public Sprite Icon;
    }

    [SerializeField] private PartIcon[] partIcons;
    [SerializeField] private CCNTextSetter levelTextSetter;
    [SerializeField] private Image partIconImage;
    [SerializeField] private Image partRarityFrameImage;
    [SerializeField] private GameObject checkObj;
    [SerializeField] private GameObject lockObj;
    [SerializeField] private Button button;
    [SerializeField] private CCNTextSetter rarityTextSetter;
    [SerializeField] private GameObject canLevelUpObj;
    [SerializeField] private GameObject equipTextObj;

    [Inject] private EquipmentSystem _equipmentSystem;

    private Equipment _equipment;
    private EquipmentIconState _state;
    private IDisposable _levelUpSubscription;
    private IDisposable _canLevelUpSubscription;
    private IDisposable _equipSubscription;
    private IDisposable _unequipSubscription;

    private const string VALUE = "value";

    public Equipment Equipment => _equipment;
    public Action ButtonAction { get; private set; }

    public void SetEquipmentIcon(Equipment equipment, Property property, Vector2 size, EquipmentIconState state)
    {
        _equipment = equipment;
        _state = state;

        partIconImage.sprite = partIcons.FirstOrDefault(x => x.Part == equipment.Type.Part).Icon;
        Set(property, size);

        var slot = _equipmentSystem.GetSlot(equipment.Type.Part);

        SubscribeLevelUp(slot);
        SetupEquipTextObj(state);
        SetupCanLevelUpObj(slot, state);

        levelTextSetter.enabled = true;
        OffCheck();
        Unlock();
        levelTextSetter.SetParam(VALUE, slot.Level.ToString());
        rarityTextSetter.enabled = false;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OpenPopup);
        ButtonAction = OpenPopup;

        SetRarityText();
    }

    private void OpenPopup()
    {
        LobbyConainer.Get<LobbyUI>().OpenPopup(
            LobbyPopupId.EquipmentInfoPopupConfig,
            new EquipmentInfoPopup.Args
            {
                State = _state,
                Equipment = _equipment
            });
    }

    private void SubscribeLevelUp(EquipmentSlot slot)
    {
        _levelUpSubscription?.Dispose();
        _levelUpSubscription = slot.OnLevelUp
            .Subscribe(_ => levelTextSetter.SetParam(VALUE, slot.Level.ToString()))
            .AddTo(this);
    }

    private void SetupEquipTextObj(EquipmentIconState state)
    {
        equipTextObj.SetActive(false);

        if (state != EquipmentIconState.Merge)
            return;

        _equipSubscription?.Dispose();
        _equipSubscription = _equipmentSystem.OnEquipStateUpdated.Subscribe(e => { equipTextObj.SetActive(e.EquippedEquipment == _equipment); }).AddTo(this);

        _unequipSubscription?.Dispose();
        _unequipSubscription = _equipmentSystem.OnUnequipStateUpdated.Subscribe(e =>
        {
            if (e.EquippedEquipment == _equipment)
                equipTextObj.SetActive(false);
        }).AddTo(this);

        var equippedList = _equipmentSystem.GetEquipped(_equipment.Type.Part);
        equipTextObj.SetActive(equippedList.Contains(_equipment));
    }

    private void SetupCanLevelUpObj(EquipmentSlot slot, EquipmentIconState state)
    {
        if (state == EquipmentIconState.ETC)
        {
            canLevelUpObj.SetActive(false);
            return;
        }

        _canLevelUpSubscription?.Dispose();
        _canLevelUpSubscription = slot.CanLevelUp
            .Subscribe(canLevelUp => canLevelUpObj.SetActive(canLevelUp))
            .AddTo(this);

        slot.CheckCanLevelUp();
        canLevelUpObj.SetActive(slot.CanLevelUp.Value);
    }

    private void SetRarityText()
    {
        var rarity = _equipment.Type.RarityData.rarity;
        int rarityLevel = rarity switch
        {
            EquipmentRarity.Epic1 or EquipmentRarity.Legendary1 or EquipmentRarity.Mythic1 => 1,
            EquipmentRarity.Epic2 or EquipmentRarity.Legendary2 or EquipmentRarity.Mythic2 => 2,
            EquipmentRarity.Legendary3 or EquipmentRarity.Mythic3 => 3,
            EquipmentRarity.Mythic4 => 4,
            _ => 0
        };

        if (rarityLevel == 0)
        {
            rarityTextSetter.gameObject.SetActive(false);
            return;
        }

        rarityTextSetter.gameObject.SetActive(true);
        rarityTextSetter.SetParam(VALUE, rarityLevel.ToString());
    }


    public void SetPosition(Vector2 position)
    {
        transform.position = position;
    }

    public void SetCustomOnClickAction(Action action)
    {
        ButtonAction = action;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => action?.Invoke());
    }

    public void OffLevelText() => levelTextSetter.enabled = false;
    public void OnCheck() => checkObj.SetActive(true);
    public void OffCheck() => checkObj.SetActive(false);
    public void Lock() => lockObj.SetActive(true);
    public void Unlock() => lockObj.SetActive(false);

    public override void Clear()
    {
        base.Clear();
        DisposeSubscriptions();
    }

    public override void Remove()
    {
        base.Remove();
        DisposeSubscriptions();
    }

    private void DisposeSubscriptions()
    {
        _levelUpSubscription?.Dispose();
        _canLevelUpSubscription?.Dispose();
        _equipSubscription?.Dispose();
        _unequipSubscription?.Dispose();
    }

    public void SetReddot(string path)
    {
        //TODO : 레드닷 기능 구현
    }
}