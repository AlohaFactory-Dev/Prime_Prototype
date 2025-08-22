using Aloha.Coconut.UI;
using Aloha.CoconutMilk.EquipmentSample;
using CoconutMilk.Equipments;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class EquipmentPopup : UISlice
{
    [SerializeField] private EquipmentInventory inventory;
    [SerializeField] private EquipmentSlotViewer equipmentSlotViewer;
    [SerializeField] private Button mergeButton;
    [SerializeField] private EquipmentInventoryOrderFilter equipmentInventoryOrderFilter;
    [Inject] private EquipmentSystem _equipmentSystem;
    private bool _isInitialized;

    protected override void Open(UIOpenArgs args)
    {
        Init();
        equipmentInventoryOrderFilter.Show();

        base.Open(args);
    }

    private void Init()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        inventory.Init();
        equipmentSlotViewer.Init();
        mergeButton.onClick.AddListener(() => { LobbyConainer.Get<LobbyUI>().OpenPopup(LobbyPopupId.EquipmentMergePopupConfig); });
        equipmentInventoryOrderFilter.Init();
        _equipmentSystem.OnEquipStateUpdated.Subscribe(x =>
        {
            //TODO 장비 장착 했을 때  
            Debug.Log("장비 장착: " + x.EquippedEquipment.Type.Part);
        }).AddTo(this);
        _equipmentSystem.OnUnequipStateUpdated.Subscribe(x =>
        {
            //TODO 장비 해제 했을 때
            Debug.Log("장비 해제: " + x.EquippedEquipment.Type.Part);
        }).AddTo(this);
    }
}