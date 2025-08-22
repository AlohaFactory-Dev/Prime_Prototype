using Aloha.Coconut;
using Aloha.Coconut.Launcher;
using UnityEngine;

namespace Aloha.Durian
{
    public class DurianInstaller : LauncherInstaller
    {
        public override void InstallBindings()
        {
            DurianApis.Initialize();
            Container.Bind<DurianConfig>().FromInstance(CoconutConfig.Get<DurianConfig>()).AsSingle();
            Container.BindInterfacesAndSelfTo<SessionManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ServerStatusChecker>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<AppVersionChecker>().AsSingle().NonLazy();
            Container.BindInterfacesTo<PlayerDataManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DurianDaemon>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MailManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<AnnouncementManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<NotificationManager>().AsSingle().NonLazy();
            Container.BindInterfacesTo<DurianReceiptValidator>().AsSingle().NonLazy();
            Container.Bind<CouponHandler>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<LeaderboardManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<OtherPlayerDataManager>().AsSingle().NonLazy();

            Container.Bind<DurianUnityEventHandler>().FromNewComponentOnRoot().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<GoogleCredentialManager>().AsSingle().NonLazy();
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Container.BindInterfacesAndSelfTo<AppleCredentialManager>().AsSingle().NonLazy();
            }
            Container.Bind<AuthManager>().AsSingle().NonLazy();

            Container.BindInterfacesTo<DurianLaunchingProcess>().AsSingle().NonLazy();
        }
    }
}
