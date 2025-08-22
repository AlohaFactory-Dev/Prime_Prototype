namespace Aloha.Coconut.Launcher
{
    public class DefaultLauncherInstaller : LauncherInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<SaveDataManager>().AsSingle().NonLazy();
            Container.Bind<GameSceneManager>().AsSingle().NonLazy();
            Container.BindInterfacesTo<PackageRewardsManager>().AsSingle().NonLazy();
            Container.BindInterfacesTo<ITitleScreen>().FromComponentsInHierarchy().AsSingle().NonLazy();
            Container.Bind<ClockComponent>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            
            Container.BindInterfacesTo<SaveDataManagerLaunchingProcess>().AsSingle().NonLazy();
        }
    }
}
