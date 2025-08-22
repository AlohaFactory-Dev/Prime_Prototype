using Zenject;

namespace Aloha.Coconut
{
    public class DirectStoreModuleInstaller : Installer<DirectStoreModuleInstaller>
    {
        public override void InstallBindings()
        {
            if (!Container.HasBinding<IDirectStoreDatabase>())
            {
                Container.BindInterfacesTo<DefaultDirectStoreDatabase>().AsSingle().NonLazy();
            }
            
            Container.BindInterfacesAndSelfTo<DirectStore>().AsSingle().NonLazy();
        }
    }
}
