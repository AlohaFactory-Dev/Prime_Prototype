using Zenject;

namespace Aloha.Coconut
{
    public class HardCurrencyModuleInstaller : Installer<HardCurrencyModuleInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<HardCurrencyProductsManager>().AsSingle().NonLazy();
        }
    }
}