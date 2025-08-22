using Aloha.Coconut;
using Aloha.Coconut.UI;
using CoconutMilk.Equipments;
using UnityEngine;
using Zenject;

namespace Aloha.CoconutMilk.EquipmentSample
{
    public class LobbyInstaller : MonoInstaller
    {
        [SerializeField] private CoconutCanvas coconutCanvas;
        [SerializeField] private LobbyUI lobbyUI;

        public override void InstallBindings()
        {
            LobbyConainer.Initialize(Container);

            Container.Bind<SaveDataManager>().AsSingle().NonLazy();
            Container.Resolve<SaveDataManager>().LinkFileDataSaver();
            Container.Bind<PropertyIconPool>().AsSingle().NonLazy();

            Container.Bind<PropertyManager>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<EquipmentSystem>().AsSingle().NonLazy();
            Container.Bind<EquipmentDatabase>().AsSingle().NonLazy();
            Container.Bind<Equipment.Factory>().AsSingle().NonLazy();
            Container.Bind<EquipmentSlot.Factory>().AsSingle().NonLazy();


            Container.Bind<CoconutCanvas>().FromInstance(coconutCanvas).AsSingle().NonLazy();
            Container.Bind<EquipmentInventoryFilterManager>().AsSingle().NonLazy();
            Container.Bind<LobbyUI>().FromInstance(lobbyUI).AsSingle().NonLazy();
        }
    }
}