using Zenject;

namespace Aloha.Coconut
{
    public class DailyFreeRewardsModuleInstaller : Installer<DailyFreeRewardsModuleInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<DailyFreeRewardsManager>().AsSingle();
        }
    }
}
