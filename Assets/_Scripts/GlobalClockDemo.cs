using UnityEngine;

/// <summary>
/// GlobalClock 사용 예시를 보여주는 데모 클래스
/// </summary>
public class GlobalClockDemo : MonoBehaviour
{
    [Header("Demo Settings")]
    [SerializeField] private bool runDemo = true;

    void Start()
    {
        if (!runDemo) return;

        // 다양한 주기의 타이머 등록 예시
        DemoBasicTimers();
        DemoGameplayTimers();
    }

    private void DemoBasicTimers()
    {
        // 4.5초마다 자원 생산
        GlobalClock.Instance.RegisterRepeatingTimer("ResourceProduction", 4.5f, () =>
        {
            Debug.Log("🏭 자원 생산: 골드 +100");
        });

        // 10초마다 건물 수리
        GlobalClock.Instance.RegisterRepeatingTimer("BuildingMaintenance", 10f, () =>
        {
            Debug.Log("🔧 건물 수리 완료");
        });

        // 2초마다 적 탐지
        GlobalClock.Instance.RegisterRepeatingTimer("EnemyScanning", 2f, () =>
        {
            Debug.Log("👁️ 적 탐지 중...");
        });

        // 7초마다 상점 아이템 갱신
        GlobalClock.Instance.RegisterRepeatingTimer("ShopRefresh", 7f, () =>
        {
            Debug.Log("🛒 상점 아이템 갱신");
        });
    }

    private void DemoGameplayTimers()
    {
        // 30초 후 보너스 이벤트 (일회성)
        GlobalClock.Instance.RegisterOneShotTimer("BonusEvent", 30f, () =>
        {
            Debug.Log("🎉 보너스 이벤트 시작!");
            
            // 보너스 기간 동안 자원 생산량 2배
            GlobalClock.Instance.RegisterRepeatingTimer("BonusProduction", 2f, () =>
            {
                Debug.Log("💰 보너스 자원: 골드 +200");
            });

            // 60초 후 보너스 종료 (일회성)
            GlobalClock.Instance.RegisterOneShotTimer("BonusEnd", 60f, () =>
            {
                GlobalClock.Instance.UnregisterTimer("BonusProduction");
                Debug.Log("⏰ 보너스 이벤트 종료");
            });
        });

        // 15초마다 자동 저장
        GlobalClock.Instance.RegisterRepeatingTimer("AutoSave", 15f, () =>
        {
            Debug.Log("💾 게임 자동 저장");
        });
    }

    [ContextMenu("Show Timer Status")]
    void ShowTimerStatus()
    {
        Debug.Log($"현재 활성 타이머 수: {GlobalClock.Instance.TimerCount}");
    }

    [ContextMenu("Pause Resource Production")]
    void PauseResourceProduction()
    {
        GlobalClock.Instance.SetTimerPaused("ResourceProduction", true);
        Debug.Log("자원 생산 일시정지");
    }

    [ContextMenu("Resume Resource Production")]
    void ResumeResourceProduction()
    {
        GlobalClock.Instance.SetTimerPaused("ResourceProduction", false);
        Debug.Log("자원 생산 재개");
    }

    [ContextMenu("Clear All Timers")]
    void ClearAllTimers()
    {
        GlobalClock.Instance.ClearAllTimers();
        Debug.Log("모든 타이머 제거");
    }
}
