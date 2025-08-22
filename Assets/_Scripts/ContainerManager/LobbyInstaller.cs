using Aloha.Coconut.UI;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class LobbyInstaller : MonoInstaller
{
    [SerializeField] private CoconutCanvas coconutCanvas;

    public override void InstallBindings()
    {
        LobbyConainer.Initialize(Container);
        Container.Bind<CoconutCanvas>().FromInstance(coconutCanvas).AsSingle().NonLazy();
        Container.Bind<EquipmentInventoryFilterManager>().AsSingle().NonLazy();
    }
}