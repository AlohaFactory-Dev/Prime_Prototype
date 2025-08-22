using System;
using Aloha.Coconut;
using Aloha.Coconut.Launcher;
using Cysharp.Threading.Tasks;
using Firebase;

namespace Aloha.Durian
{
    public class DurianLaunchingProcess : ILaunchingProcess
    {
        private readonly DurianConfig _durianConfig;
        private readonly ServerStatusChecker _serverStatusChecker;
        private readonly AppVersionChecker _appVersionChecker;
        private readonly AuthManager _authManager;

        public int Order => -1500;
        public string Message => "Initializing API...";
        public bool IsBlocker => true;

        public DurianLaunchingProcess(DurianConfig durianConfig,
            ServerStatusChecker serverStatusChecker, AppVersionChecker appVersionChecker, AuthManager authManager)
        {
            _durianConfig = durianConfig;
            _serverStatusChecker = serverStatusChecker;
            _appVersionChecker = appVersionChecker;
            _authManager = authManager;
        }
        
        public async UniTask Run(ITitleScreen titleScreen)
        {
            await FirebaseApp.CheckAndFixDependenciesAsync();
            
            bool isServerStatusChecked = false;
            titleScreen.SetMessage("Checking server status...");
            do
            {
                try
                {
                    await _serverStatusChecker.InitialCheck();
                    isServerStatusChecked = true;
                }
                catch (ServerUnderMaintenanceException e)
                {
                    return;
                }
                catch // 인터넷이 끊긴 상황 등에서 서버 상태 체크가 exception과 함께 실패할 수 있음
                {
                    await SystemUI.ShowDialogue("Server Error", "Failed to check server status. Please check your internet connection and try again.");
                }
            } while (!isServerStatusChecked);
            
            bool isAppVersionChecked = false;
            titleScreen.SetMessage("Checking app version...");
            do
            {
                try
                {
                    await _appVersionChecker.InitialCheck();
                    isAppVersionChecked = true;
                }
                catch (Exception e)
                {
                    await SystemUI.ShowDialogue("Server Error", $"Checking app version failed.\n{e.Message}");
                }   
            } while (!isAppVersionChecked);
            
            bool isAuthInitialized = false;
            titleScreen.SetMessage("Initializing auth...");
            do
            {
                try
                {
                    if(!_authManager.IsInitialized) await _authManager.Initialize();
                    if (!_authManager.IsSignedIn.Value && _durianConfig.autoSignInAsGuest)
                    {
                        titleScreen.SetMessage("Signing in as guest...");
                        await _authManager.SignInAsGuest();
                    }
                
                    isAuthInitialized = true;
                }
                catch (Exception e)
                {
                    await SystemUI.ShowDialogue("Server Error", $"Initializing auth failed.\n{e.Message}");
                }   
            } while (!isAuthInitialized);
        }
    }
}