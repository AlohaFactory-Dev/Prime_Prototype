using Aloha.Coconut.UI;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class LobbyInstaller : MonoInstaller
{
    private CoconutCanvas _coconutCanvas;

    public override void InstallBindings()
    {
        _coconutCanvas = GetComponentInChildren <CoconutCanvas>(true);
        
        LobbyConainer.Initialize(Container);
        Container.Bind<CoconutCanvas>().FromInstance(_coconutCanvas).AsSingle().NonLazy();
        Container.Bind<EquipmentInventoryFilterManager>().AsSingle().NonLazy();
    }
}