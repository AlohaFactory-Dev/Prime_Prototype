using System.Collections;
using System.Collections.Generic;
using CoconutMilk.Equipments;
using TMPro;
using UnityEngine;

public class EquipmentOptionViewer : MonoBehaviour
{
    private EquipmentOptionCard[] _optionCards;

    public void Init()
    {
        _optionCards = GetComponentsInChildren<EquipmentOptionCard>(true);
    }

    public void Open(Equipment equipment)
    {
        foreach (var card in _optionCards)
        {
            card.Close();
        }

        foreach (var option in equipment.Type.PotentialOptions)
        {
            foreach (var card in _optionCards)
            {
                card.Show(option);
            }
        }
    }
}