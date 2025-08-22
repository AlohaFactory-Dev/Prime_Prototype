using Cysharp.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace Aloha.Coconut.Launcher
{
    public class IAPModuleLaunchingProcess : ILaunchingProcess
    {
        public int Order => 0;
        public string Message => "Initializing IAP Module";
        public bool IsBlocker => true;
        
        private readonly IIAPManager _iapManager;
        
        public IAPModuleLaunchingProcess(IIAPManager iapManager)
        {
            _iapManager = iapManager;
        }

        public async UniTask Run(ITitleScreen titleScreen)
        {
            titleScreen.Report(0);
            
            if (!string.IsNullOrEmpty(Application.cloudProjectId))
            {
                var options = new InitializationOptions().SetEnvironmentName("production");
                await UnityServices.InitializeAsync(options);
            }
            
            var uniTaskSource = new UniTaskCompletionSource<bool>();
            _iapManager.AddOnInitializedListener(success =>
            {
                uniTaskSource.TrySetResult(success);
            });

            _iapManager.Initialize();

            await uniTaskSource.Task;
            titleScreen.Report(1);
        }
    }
}