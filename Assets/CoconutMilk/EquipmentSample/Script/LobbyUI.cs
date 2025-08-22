using System;
using Aloha.Coconut;
using Aloha.Coconut.UI;
using CoconutMilk.Equipments;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Aloha.CoconutMilk.EquipmentSample
{
    public enum LobbyPopupId
    {
        EquipmentPopupConfig,
        EquipmentMergePopupConfig,
        EquipmentInfoPopupConfig,
        ItemObtainedPopupConfig,
    }

    public class LobbyUI : MonoBehaviour
    {
        [Inject] private CoconutCanvas _coconutCanvas;
        private TestEquipmentAdd _testEquipmentAdd;

        private void Start()
        {
            _testEquipmentAdd = new TestEquipmentAdd();
            OpenPopup(LobbyPopupId.EquipmentPopupConfig);
        }

        public void OpenPopup(LobbyPopupId lobbyPopupId, UIOpenArgs uiOpenArgs = null)
        {
            _coconutCanvas.Open(lobbyPopupId.ToString(), uiOpenArgs);
        }
    }
}