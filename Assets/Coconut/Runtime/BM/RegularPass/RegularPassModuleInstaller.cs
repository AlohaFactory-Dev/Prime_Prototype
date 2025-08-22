using Zenject;

namespace Aloha.Coconut
{
    public class RegularPassModuleInstaller : Installer<RegularPassModuleInstaller>
    {
        public override void InstallBindings()
        {
            if (!Container.HasBinding<Pass.Factory>())
            {
                Container.Bind<Pass.Factory>().AsSingle().NonLazy();
            }

            if (!Container.HasBinding<IRegularPassDatabase>())
            {
                Container.BindInterfacesTo<DefaultRegularPassDatabase>().AsSingle().NonLazy();
            }

            Container.Bind<RegularPass.Factory>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<RegularPassManager>().AsSingle().NonLazy();
        }
    }
}