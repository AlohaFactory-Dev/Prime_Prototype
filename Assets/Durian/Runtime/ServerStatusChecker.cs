using System;
using Aloha.Coconut;
using Alohacorp.Durian.Model;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Aloha.Durian
{
    internal struct ServerStatus
    {
        // 유지보수 체크는 서버에서 받은 ServerTime(UTC+9 기준)을 그대로 사용합니다.
        public bool IsUnderMaintenance => _maintenanceScheduleDto != null && ServerTime >= StartsAt && ServerTime <= EndsAt;
        public DateTime StartsAt => _maintenanceScheduleDto.StartsAt;
        public DateTime EndsAt => _maintenanceScheduleDto.EndsAt;
        public string AnnouncementMessage => _maintenanceScheduleDto.AnnouncementMessage;
        public DateTime ServerTime { get; }

        private readonly SystemMaintenanceScheduleDto _maintenanceScheduleDto;

        public ServerStatus(ServerInfoDto serverInfoDto)
        {
            _maintenanceScheduleDto = serverInfoDto.SystemMaintenanceSchedule;
            ServerTime = serverInfoDto.ServerTime;
        }
    }
    
    internal class ServerUnderMaintenanceException : Exception
    {
        public ServerUnderMaintenanceException(string message) : base(message) { }
    }

    public class ServerStatusChecker : IDisposable
    {
        private readonly SessionManager _sessionManager;
        private readonly DurianConfig _durianConfig;
        private string _sessionId;

        private CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public ServerStatusChecker(SessionManager sessionManager, DurianConfig durianConfig)
        {
            _sessionManager = sessionManager;
            _durianConfig = durianConfig;
        }

        internal async UniTask InitialCheck()
        {
            var serverInfoDto = await RequestHandler.Request(DurianApis.RootApi().GetServerInfoAsync(), resp => resp.Data);

            var serverStatus = new ServerStatus(serverInfoDto);
            if (serverStatus.IsUnderMaintenance)
            {
                await ShowServerMaintenancePopup(serverStatus, true);
            }

            await SyncServerTimeAsync(serverInfoDto);
            DurianUnityEventHandler.AppForegroundSubject
                .Subscribe(_ => SyncServerTimeAsync().Forget())
                .AddTo(_compositeDisposable);

            _sessionManager.InitSessionAsync().ContinueWith(() =>
            {
                CheckSessionDeactivated().Forget();
            });
        }

        private async UniTask ShowServerMaintenancePopup(ServerStatus serverStatus, bool dontQuitIfSuperUser)
        {
            bool isSuperUser = false;
            IDisposable specialGestureCheck = SystemUI.OnSpecialGesture.Subscribe(_ => { isSuperUser = true; });
            await SystemUI.ShowDialogue(TextTableV2.Get("Common/Notice"), serverStatus.AnnouncementMessage);

            specialGestureCheck.Dispose();
            if (dontQuitIfSuperUser && isSuperUser)
            {
                return;
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            throw new ServerUnderMaintenanceException("Server is under maintenance.");
        }

        private async UniTask SyncServerTimeAsync(ServerInfoDto serverInfoDto = null)
        {
            Clock.SetIsTicking(false);
            try
            {
                if (serverInfoDto == null)
                {
                    serverInfoDto = await RequestHandler.Request(DurianApis.RootApi().GetServerInfoAsync(), resp => resp.Data);
                }
                DateTime serverTime = serverInfoDto.ServerTime;
                DateTime localServerTime = serverTime - TimeSpan.FromHours(9) + TimeZoneInfo.Local.BaseUtcOffset;
                TimeSpan diff = localServerTime - Clock.NoDebugNow;
                Clock.AddOffset(diff);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to sync server time: " + e);
            }
            finally
            {
                Clock.SetIsTicking(true);
            }
        }

        private async UniTask CheckSessionDeactivated()
        {
            while (true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_durianConfig.statusCheckInterval));
                try
                {
                    await _sessionManager.CheckSessionAsync();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    var serverInfoDto = await RequestHandler.Request(DurianApis.RootApi().GetServerInfoAsync(), resp => resp.Data);
                    await ShowServerMaintenancePopup(new ServerStatus(serverInfoDto), false);
                }
            }
        }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }
    }
}
