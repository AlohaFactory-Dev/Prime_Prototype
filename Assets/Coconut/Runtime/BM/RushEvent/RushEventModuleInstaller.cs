using Zenject;

namespace Aloha.Coconut
{
    public class RushEventModuleInstaller : Installer<RushEventModuleInstaller>
    {
        public override void InstallBindings()
        {
            if (!Container.HasBinding<IRushEventDatabase>())
            {
                Container.BindInterfacesAndSelfTo<DefaultRushEventDatabase>().AsSingle().NonLazy();
            }

            Container.BindInterfacesAndSelfTo<RushEventManager>().AsSingle().NonLazy();
            Container.Bind<RushEvent.Factory>().AsSingle().NonLazy();
            Container.Bind<RushEventMissionGroup.Factory>().AsSingle().NonLazy();
            Container.Bind<RushEventPackageGroup.Factory>().AsSingle().NonLazy();
        }
    }
}
