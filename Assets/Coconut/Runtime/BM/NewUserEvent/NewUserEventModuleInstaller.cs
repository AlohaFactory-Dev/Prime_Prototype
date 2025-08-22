using Aloha.Coconut.Missions;
using Zenject;

namespace Aloha.Coconut
{
    public class NewUserEventModuleInstaller : Installer<NewUserEventModuleInstaller>
    {
        public override void InstallBindings()
        {
            if (!Container.HasBinding<Pass.Factory>())
            {
                Container.Bind<Pass.Factory>().AsSingle().NonLazy();
            }

            if (!Container.HasBinding<MissionFactory>())
            {
                Container.Bind<MissionFactory>().AsSingle().NonLazy();
            }

            if (!Container.HasBinding<INewUserEventDatabase>())
            {
                Container.BindInterfacesTo<DefaultNewUserEventDatabase>().AsSingle().NonLazy();
            }

            Container.Bind<NewUserMissionGroup.Factory>().AsSingle().NonLazy();
            Container.Bind<NewUserPackageGroup.Factory>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<NewUserEvent>().AsSingle().NonLazy();
        }
    }
}