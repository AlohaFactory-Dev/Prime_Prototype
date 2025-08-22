using UnityEngine;

/// <summary>
/// 건물 기본 클래스 - GlobalClock을 사용하여 주기적 동작을 관리
/// </summary>
public abstract class Building : MonoBehaviour
{
    [Header("Building Settings")]
    [SerializeField] protected string buildingName;
    [SerializeField] protected float actionInterval = 1f;
    
    protected string timerID;
    protected bool isActive = false;

    protected virtual void Start()
    {
        // 고유한 타이머 ID 생성
        timerID = $"{buildingName}_{GetInstanceID()}";
        StartBuilding();
    }

    /// <summary>
    /// 건물 동작 시작
    /// </summary>
    public virtual void StartBuilding()
    {
        if (isActive) return;

        // GlobalClock에 타이머 등록
        bool success = GlobalClock.Instance.RegisterRepeatingTimer(timerID, actionInterval, PerformAction);
        if (success)
        {
            isActive = true;
            Debug.Log($"{buildingName} started with {actionInterval}s interval");
        }
    }

    /// <summary>
    /// 건물 동작 중지
    /// </summary>
    public virtual void StopBuilding()
    {
        if (!isActive) return;

        GlobalClock.Instance.UnregisterTimer(timerID);
        isActive = false;
        Debug.Log($"{buildingName} stopped");
    }

    /// <summary>
    /// 건물 일시정지/재개
    /// </summary>
    public virtual void SetPaused(bool paused)
    {
        if (!isActive) return;
        
        GlobalClock.Instance.SetTimerPaused(timerID, paused);
        Debug.Log($"{buildingName} {(paused ? "paused" : "resumed")}");
    }

    /// <summary>
    /// 주기적으로 실행될 동작 - 상속받는 클래스에서 구현
    /// </summary>
    protected abstract void PerformAction();

    protected virtual void OnDestroy()
    {
        StopBuilding();
    }
}
