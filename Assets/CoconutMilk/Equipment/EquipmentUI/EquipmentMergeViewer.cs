using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentMergeViewer : MonoBehaviour
{
    private EquipmentMergeIngredientSlotContainer _ingredientSlotContainer;

    public void Init()
    {
        _ingredientSlotContainer = GetComponentInChildren<EquipmentMergeIngredientSlotContainer>();
        _ingredientSlotContainer.Init();
    }

    public void Show()
    {
        _ingredientSlotContainer.Show();
    }

    public void Refresh()
    {
        _ingredientSlotContainer.Refresh();
    }
}