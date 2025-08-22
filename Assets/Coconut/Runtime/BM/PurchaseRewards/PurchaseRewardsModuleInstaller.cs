using Zenject;

namespace Aloha.Coconut
{
    public class PurchaseRewardsModuleInstaller : Installer<PurchaseRewardsModuleInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PurchaseRewardsEventManager>().AsSingle().NonLazy();
            Container.Bind<PurchaseRewardsEvent.Factory>().AsSingle().NonLazy();
            Container.Bind<PurchaseRewards.Factory>().AsSingle().NonLazy();
        }
    }
}
