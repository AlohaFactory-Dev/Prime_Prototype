using Aloha.Coconut;
using CoconutMilk.Equipments;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentOptionCard : MonoBehaviour
{
    [SerializeField] private Color deActiveColor;
    [SerializeField] private Color activeColor;
    [SerializeField] private EquipmentRarity rarity;
    [SerializeField] private Image lockIcon;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public void Show(EquipmentOption option)
    {
        if (option.rarity != rarity) return;

        gameObject.SetActive(true);

        //TODO : 옵션 설명 텍스트를 설정하는 로직을 추가해야 합니다.
        descriptionText.text = $"{rarity} - 옵션";

        descriptionText.color = option.isActive ? activeColor : deActiveColor;
        lockIcon.enabled = !option.isActive;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}