using Zenject;

namespace Aloha.Coconut
{
    public class EventScheduleModuleInstaller: Installer<EventScheduleModuleInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<EventScheduleManager>().AsSingle().NonLazy();
        }
    }
}
