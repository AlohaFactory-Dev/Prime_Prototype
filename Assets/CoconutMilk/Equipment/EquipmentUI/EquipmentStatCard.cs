using System.Collections;
using System.Collections.Generic;
using Aloha.Coconut;
using CoconutMilk.Equipments;
using UnityEngine;
using Zenject;

public class EquipmentStatCard : MonoBehaviour
{
    [SerializeField] private StatType propertyType;
    [SerializeField] private CCNTextSetter statText;
    [Inject] private EquipmentSystem _equipmentSystem;

    public void Show(EquipmentPart part, EquipmentStatBonus statBonus)
    {
        if (statBonus.type != propertyType) return;
        gameObject.SetActive(true);
        var slot = _equipmentSystem.GetSlot(part);
        statText.SetParam("value", slot.CalculateStatBonus(statBonus).ToString());
    }


    public void Close()
    {
        gameObject.SetActive(false);
    }
}