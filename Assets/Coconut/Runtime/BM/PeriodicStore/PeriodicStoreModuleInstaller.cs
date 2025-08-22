using Zenject;

namespace Aloha.Coconut
{
    public class PeriodicStoreModuleInstaller : Installer<PeriodicStoreModuleInstaller>
    {
        public override void InstallBindings()
        {
            if (!Container.HasBinding<IPeriodicStoreDatabase>())
            {
                Container.BindInterfacesTo<DefaultPeriodicStoreDatabase>().AsSingle().NonLazy();
            }
            
            Container.BindInterfacesAndSelfTo<PeriodicStoreManager>().AsSingle().NonLazy();
            Container.Bind<PeriodicStore.Factory>().AsSingle();
        }
    }
}
