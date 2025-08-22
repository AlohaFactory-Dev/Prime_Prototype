using Aloha.CoconutMilk;
using CoconutMilk.Equipments;
using UnityEngine;
using Zenject;

public class EquipmentMergeResultPreviewSlot : MonoBehaviour
{
    [Inject] private PropertyIconPool _propertyIconPool;
    [Inject] private Equipment.Factory _equipmentFactory;
    private EquipmentIcon _equipmentIcon;

    public void Show(Equipment equipment)
    {
        // Next EquipmentType으로 Equipment 인스턴스 생성
        var nextType = equipment.Type.Next;
        var tempSaveData = new Equipment.SaveData(nextType.TypeId, 0); // uid는 임시값(0)
        var nextEquipment = _equipmentFactory.Create(tempSaveData);

        _equipmentIcon = _propertyIconPool.Get(nextEquipment, (RectTransform)transform, ((RectTransform)transform).sizeDelta);
    }

    public void Refresh()
    {
        _equipmentIcon?.Remove();
        _equipmentIcon = null;
    }
}