using Aloha.Coconut;
using Aloha.Durian;
using Zenject;

public class SampleInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<SampleGameData>().AsSingle().NonLazy();
        Container.Bind<PropertyManager>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<NotificationManager>().AsSingle().NonLazy();
        Container.Bind<SimpleValues>().AsSingle().NonLazy();
    }
}
