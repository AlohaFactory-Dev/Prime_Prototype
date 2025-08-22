using Zenject;

namespace Aloha.Coconut
{
    public class BeginnerPackagesModuleInstaller : Installer<BeginnerPackagesModuleInstaller>
    {
        public override void InstallBindings()
        {
            if (!Container.HasBinding<IBeginnerPackageDatabase>())
            {
                Container.BindInterfacesAndSelfTo<DefaultBeginnerPackageDatabase>().AsSingle();
            }

            Container.BindInterfacesAndSelfTo<BeginnerPackagesManager>().AsSingle();
            Container.Bind<BeginnerPackage.Factory>().AsSingle();
            Container.Bind<BeginnerPackageComponent.Factory>().AsSingle();
        }
    }
}
