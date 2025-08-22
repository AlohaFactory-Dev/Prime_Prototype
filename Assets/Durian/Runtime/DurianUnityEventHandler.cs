using UnityEngine;
using UniRx;

namespace Aloha.Durian
{
    public class DurianUnityEventHandler : MonoBehaviour
    {
        public static readonly Subject<Unit> AppBackgroundSubject = new Subject<Unit>();
        public static readonly Subject<Unit> AppForegroundSubject = new Subject<Unit>();
        public static readonly Subject<Unit> AppQuitSubject = new Subject<Unit>();

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                AppBackgroundSubject.OnNext(Unit.Default);
            }
            else
            {
                AppForegroundSubject.OnNext(Unit.Default);
            }
        }

        private void OnApplicationQuit()
        {
            AppQuitSubject.OnNext(Unit.Default);

            // 종료 시 각 Subject를 완료 처리하여 구독자에게 이벤트의 종료를 알림
            AppBackgroundSubject.OnCompleted();
            AppForegroundSubject.OnCompleted();
            AppQuitSubject.OnCompleted();
        }
    }
}