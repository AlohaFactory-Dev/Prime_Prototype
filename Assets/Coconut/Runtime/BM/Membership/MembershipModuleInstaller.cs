using Zenject;

namespace Aloha.Coconut
{
    public class MembershipModuleInstaller : Installer<MembershipModuleInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MembershipManager>().AsSingle().NonLazy();
            Container.Bind<Membership.Factory>().AsSingle().NonLazy();
        }
    }
}
