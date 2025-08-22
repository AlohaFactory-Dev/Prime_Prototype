using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전역 시계 시스템 - 여러 오브젝트가 서로 다른 주기로 동작할 수 있도록 관리
/// </summary>
public class GlobalClock : MonoBehaviour
{
    private static GlobalClock _instance;
    public static GlobalClock Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("GlobalClock");
                _instance = go.AddComponent<GlobalClock>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    /// <summary>
    /// 타이머 정보를 담는 클래스
    /// </summary>
    private class Timer
    {
        public string id;
        public float interval;
        public float nextExecuteTime;
        public Action callback;
        public bool isActive;
        public bool isOneShot;

        public Timer(string id, float interval, Action callback, bool isOneShot = false)
        {
            this.id = id;
            this.interval = interval;
            this.callback = callback;
            this.isOneShot = isOneShot;
            this.isActive = true;
            this.nextExecuteTime = Time.time + interval;
        }
    }

    private List<Timer> timers = new List<Timer>();
    private List<Timer> timersToRemove = new List<Timer>();

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        float currentTime = Time.time;

        // 타이머 실행 및 관리
        for (int i = 0; i < timers.Count; i++)
        {
            Timer timer = timers[i];
            
            if (!timer.isActive) continue;

            if (currentTime >= timer.nextExecuteTime)
            {
                try
                {
                    timer.callback?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"GlobalClock Timer '{timer.id}' callback error: {e.Message}");
                }

                if (timer.isOneShot)
                {
                    timersToRemove.Add(timer);
                }
                else
                {
                    timer.nextExecuteTime = currentTime + timer.interval;
                }
            }
        }

        // 제거할 타이머들 정리
        if (timersToRemove.Count > 0)
        {
            foreach (Timer timer in timersToRemove)
            {
                timers.Remove(timer);
            }
            timersToRemove.Clear();
        }
    }

    /// <summary>
    /// 반복 타이머 등록
    /// </summary>
    /// <param name="id">타이머 고유 ID</param>
    /// <param name="interval">실행 간격(초)</param>
    /// <param name="callback">실행할 콜백</param>
    /// <returns>등록 성공 여부</returns>
    public bool RegisterRepeatingTimer(string id, float interval, Action callback)
    {
        if (string.IsNullOrEmpty(id) || callback == null || interval <= 0)
        {
            Debug.LogError("GlobalClock: Invalid timer parameters");
            return false;
        }

        // 중복 ID 체크
        if (HasTimer(id))
        {
            Debug.LogWarning($"GlobalClock: Timer with ID '{id}' already exists");
            return false;
        }

        Timer timer = new Timer(id, interval, callback, false);
        timers.Add(timer);
        
        Debug.Log($"GlobalClock: Registered repeating timer '{id}' with interval {interval}s");
        return true;
    }

    /// <summary>
    /// 일회성 타이머 등록
    /// </summary>
    /// <param name="id">타이머 고유 ID</param>
    /// <param name="delay">지연 시간(초)</param>
    /// <param name="callback">실행할 콜백</param>
    /// <returns>등록 성공 여부</returns>
    public bool RegisterOneShotTimer(string id, float delay, Action callback)
    {
        if (string.IsNullOrEmpty(id) || callback == null || delay < 0)
        {
            Debug.LogError("GlobalClock: Invalid timer parameters");
            return false;
        }

        // 중복 ID 체크
        if (HasTimer(id))
        {
            Debug.LogWarning($"GlobalClock: Timer with ID '{id}' already exists");
            return false;
        }

        Timer timer = new Timer(id, delay, callback, true);
        timers.Add(timer);
        
        Debug.Log($"GlobalClock: Registered one-shot timer '{id}' with delay {delay}s");
        return true;
    }

    /// <summary>
    /// 타이머 제거
    /// </summary>
    /// <param name="id">제거할 타이머 ID</param>
    /// <returns>제거 성공 여부</returns>
    public bool UnregisterTimer(string id)
    {
        Timer timer = FindTimer(id);
        if (timer != null)
        {
            timers.Remove(timer);
            Debug.Log($"GlobalClock: Unregistered timer '{id}'");
            return true;
        }
        
        Debug.LogWarning($"GlobalClock: Timer '{id}' not found");
        return false;
    }

    /// <summary>
    /// 타이머 일시정지/재개
    /// </summary>
    /// <param name="id">타이머 ID</param>
    /// <param name="paused">일시정지 여부</param>
    /// <returns>성공 여부</returns>
    public bool SetTimerPaused(string id, bool paused)
    {
        Timer timer = FindTimer(id);
        if (timer != null)
        {
            timer.isActive = !paused;
            Debug.Log($"GlobalClock: Timer '{id}' {(paused ? "paused" : "resumed")}");
            return true;
        }

        Debug.LogWarning($"GlobalClock: Timer '{id}' not found");
        return false;
    }

    /// <summary>
    /// 타이머 존재 여부 확인
    /// </summary>
    /// <param name="id">타이머 ID</param>
    /// <returns>존재 여부</returns>
    public bool HasTimer(string id)
    {
        return FindTimer(id) != null;
    }

    /// <summary>
    /// 모든 타이머 제거
    /// </summary>
    public void ClearAllTimers()
    {
        timers.Clear();
        timersToRemove.Clear();
        Debug.Log("GlobalClock: All timers cleared");
    }

    /// <summary>
    /// 등록된 타이머 개수
    /// </summary>
    public int TimerCount => timers.Count;

    /// <summary>
    /// 타이머 찾기
    /// </summary>
    private Timer FindTimer(string id)
    {
        return timers.Find(t => t.id == id);
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
