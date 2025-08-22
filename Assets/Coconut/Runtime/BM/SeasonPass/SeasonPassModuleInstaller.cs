using Zenject;

namespace Aloha.Coconut
{
    public class SeasonPassModuleInstaller : Installer<SeasonPassModuleInstaller>
    {
        public override void InstallBindings()
        {
            if (!Container.HasBinding<Pass.Factory>())
            {
                Container.Bind<Pass.Factory>().AsSingle().NonLazy();    
            }

            Container.BindInterfacesAndSelfTo<SeasonPass>().AsSingle().NonLazy();
        }
    }
}
