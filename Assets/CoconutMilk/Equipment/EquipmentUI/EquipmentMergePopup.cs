using Aloha.Coconut.UI;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentMergePopup : UISlice
{
    [SerializeField] private EquipmentMergeViewer mergeViewer;
    [SerializeField] private EquipmentInventory inventory;
    [SerializeField] private EquipmentMergeMoveIconPanel moveIconPanel;
    [SerializeField] private EquipmentInventoryPartFilter partFilter;

    private bool _isInitialized;

    protected override void Open(UIOpenArgs args)
    {
        Init();
        if (IsOpened) return;
        base.Open(args);
        mergeViewer.Show();
        moveIconPanel.Show();
        partFilter.Show();
    }

    private void Init()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        mergeViewer.Init();
        inventory.Init(moveIconPanel);
        partFilter.Init();
    }

    protected override void OnClose()
    {
        mergeViewer.Refresh();
        base.OnClose();
    }
}