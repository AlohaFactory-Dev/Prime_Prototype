using System;
using System.Collections;
using System.Collections.Generic;
using Aloha.Coconut;
using Aloha.CoconutMilk;
using Aloha.CoconutMilk.EquipmentSample;
using CoconutMilk.Equipments;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class EquipmentMergeIngredientSlotContainer : MonoBehaviour
{
    [SerializeField] private EquipmentMergeResultPreviewSlot resultSlot;
    [SerializeField] private EquipmentMergeIngredientSlot[] slots;
    [SerializeField] private Button mergeButton;
    [SerializeField] private Button batchMergeButton;
    [Inject] private PropertyIconPool _propertyIconPool;
    [Inject] private EquipmentSystem _equipmentSystem;
    [Inject] private LobbyUI _lobbyUI;

    public Animator animator;
    public EquipmentMergeIngredientLeftSlot leftSlot;

    public EquipmentMergeIngredientSlot FirstSlot => slots[0];
    public EquipmentMergeIngredientSlot SecondSlot => slots[1];
    public bool FirstSlotIsEmpty => FirstSlot.IsEmpty && !FirstSlot.IsOff;
    public bool SecondSlotIsEmpty => SecondSlot.IsEmpty && !SecondSlot.IsOff;


    // [Inject] private EquipmentInventoryFilterManager _equipmentInventoryFilterManager;

    public void Init()
    {
        leftSlot.Init(Refresh);
        foreach (var slot in slots)
        {
            slot.Init(CheckAllFilledSlots);
        }

        mergeButton.onClick.AddListener(Merge);
        batchMergeButton.onClick.AddListener(BatchMerge);
    }


    private void BatchMerge()
    {
        var mergeResults = _equipmentSystem.BatchMerge();
        if (mergeResults.Count == 0)
        {
            SystemUI.ShowToastMessage(TextTableV2.Get("Equipment/Merge/Toast/NotingToMerge"));
            return;
        }

        var properties = new List<Property>();
        foreach (var result in mergeResults)
        {
            properties.Add(new Property(result.resultEquipment.ToPropertyType(), 1));
        }

        Refresh();
        _lobbyUI.OpenPopup(LobbyPopupId.ItemObtainedPopupConfig, new ItemObtainedPopup.Args(properties));
    }

    private void Merge()
    {
        var ingredients = new List<Equipment>();
        foreach (var t in slots)
        {
            if (t.EquipmentIcon)
            {
                ingredients.Add(t.EquipmentIcon.Equipment);
            }
        }

        var mergeResult = _equipmentSystem.Merge(leftSlot.EquipmentIcon.Equipment, ingredients);
        var property = new Property(mergeResult.resultEquipment.ToPropertyType(), 1);
        Refresh();
        _lobbyUI.OpenPopup(LobbyPopupId.ItemObtainedPopupConfig, new ItemObtainedPopup.Args(new List<Property>() { property }));
    }

    public void Show()
    {
        animator.SetTrigger("Off");
        Refresh();
        CheckAllFilledSlots();
    }

    public void CheckAllFilledSlots()
    {
        if (!FirstSlotIsEmpty && !SecondSlotIsEmpty && !leftSlot.IsEmpty)
        {
            OnMergeButton();
        }
        else
        {
            OffMergeButton();
        }
    }

    private void OnMergeButton()
    {
        mergeButton.gameObject.SetActive(true);
    }

    private void OffMergeButton()
    {
        mergeButton.gameObject.SetActive(false);
    }

    public void SetSlot(EquipmentIcon equipmentIcon)
    {
        animator.SetTrigger("On");

        resultSlot.Show(equipmentIcon.Equipment);
        leftSlot.SetEquipment(equipmentIcon);
        var rarityData = equipmentIcon.Equipment.Type.RarityData;
        if (rarityData.requiredSource == EquipmentRequiredSource.SameEquipment)
        {
            for (var i = 0; i < slots.Length; i++)
            {
                if (i >= rarityData.requiredCount)
                {
                    slots[i].Off();
                }
                else
                {
                    slots[i].On(equipmentIcon.Equipment);
                }
            }
        }
        else if (rarityData.requiredSource == EquipmentRequiredSource.SameEquipmentOrMaterial)
        {
            var ingredient = _equipmentSystem.GetIngredientOf(equipmentIcon.Equipment);
            for (var i = 0; i < slots.Length; i++)
            {
                if (i >= rarityData.requiredCount)
                {
                    slots[i].Off();
                }
                else
                {
                    slots[i].On(ingredient);
                }
            }
        }
    }

    public void SetFirstSlot(EquipmentIcon equipmentIcon)
    {
        FirstSlot.SetEquipmentIcon(equipmentIcon);
    }

    public void SetSecondSlot(EquipmentIcon equipmentIcon)
    {
        SecondSlot.SetEquipmentIcon(equipmentIcon);
    }


    public void Refresh()
    {
        resultSlot.Refresh();
        leftSlot.Refresh();
        foreach (var slot in slots)
        {
            slot.Refresh();
        }

        animator.SetTrigger("Off");
        CheckAllFilledSlots();
    }
}