using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Popup.LobbyPopup.Equipment
{
    public class EquipmentInventoryPartFilterCard : MonoBehaviour
    {
        [SerializeField] private PartFilterType partFilterType;

        public void Init(Action<PartFilterType> onClickAction)
        {
            GetComponentInChildren<Button>().onClick.AddListener(() => onClickAction(partFilterType));
        }
    }
}