using UnityEngine;
using TMPro;

public class EquipmentSlotViewer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI attackPowerText;
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private TextMeshProUGUI maxHpText;
    [SerializeField] private TextMeshProUGUI totalStatText;

    private EquipmentSlotCard[] _equipmentSlotCards;

    public void Init()
    {
        _equipmentSlotCards = GetComponentsInChildren<EquipmentSlotCard>(true);
        foreach (var card in _equipmentSlotCards)
        {
            card.Init();
        }
        
        //TODO: 스텟 텍스트 초기화
        Debug.Log("스텟 텍스트 입력");

    }
}