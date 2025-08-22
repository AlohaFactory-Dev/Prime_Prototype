using Aloha.Coconut.IAP;

namespace Aloha.Coconut.Launcher
{
    public class IAPModuleInstaller : LauncherInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<IAPManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<IAPModuleLaunchingProcess>().AsSingle().NonLazy();
        }
    }
}