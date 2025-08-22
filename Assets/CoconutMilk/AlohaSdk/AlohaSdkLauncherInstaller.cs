#if COCONUT_ALOHA_SDK

using Aloha.Coconut.Launcher;
using Aloha.Coconut.Player;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Aloha.CoconutMilk
{
    public class AlohaSdkLauncherInstaller : LauncherInstaller
    {
        public override void InstallBindings()
        {
            Assert.IsNotNull(Object.FindObjectOfType<Sdk.AlohaSdk>(), "AlohaSdk is not found in the scene.");
            Container.BindInterfacesTo<InitializeAlohaSdk>().AsSingle().NonLazy();
            Container.BindInterfacesTo<AlohaSdkCUIDProvider>().AsSingle().NonLazy();
            Container.BindInterfacesTo<AlohaSdkRVAdapter>().AsSingle().NonLazy();
        }
        
        private class InitializeAlohaSdk : ILaunchingProcess
        {
            public int Order => -1000;
            public string Message => "Initializing SDK...";
            public bool IsBlocker => true;
            
            public async UniTask Run(ITitleScreen titleScreen)
            {
                await UniTask.WaitUntil(() => Sdk.AlohaSdk.IsInitialized);
            }
        }
        
        private class AlohaSdkCUIDProvider : IMyUIDProvider
        {
            public string MyUID => Sdk.AlohaSdk.CUID;
        }
    }
}

#endif