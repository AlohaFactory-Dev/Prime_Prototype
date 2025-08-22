using Zenject;

namespace Aloha.Coconut
{
    public class SpecialPackageModuleInstaller : Installer<SpecialPackageModuleInstaller>
    {
        public override void InstallBindings()
        {
            if (!Container.HasBinding<ISpecialPackageDatabase>())
            {
                Container.BindInterfacesTo<DefaultSpecialPackageDatabase>().AsSingle();
            }
            
            Container.BindInterfacesAndSelfTo<SpecialPackageManager>().AsSingle();
        }
    }
}
