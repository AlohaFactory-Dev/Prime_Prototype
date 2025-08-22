using System;
using UniRx;
using UnityEngine;

namespace Aloha.Coconut
{
    /// <summary>
    /// 시계 클래스입니다.
    /// </summary>
    public static class Clock
    {
        public const int RESET_TIME = 5; // 5AM을 기본 리셋 시간으로 사용
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            _onSecondTick?.Dispose();
            _onSecondTick = new Subject<Unit>();
            
            _onMinuteTick?.Dispose();
            _onMinuteTick = new Subject<Unit>();
            
            _onHourTick?.Dispose();
            _onHourTick = new Subject<Unit>();
            
            _onGameDatePassed?.Dispose();
            _onGameDatePassed = new Subject<Unit>();
            
            _isFirstTick = true;
            LastTick = Now;
        }
        
        /// <summary>
        /// SecondTick!
        /// </summary>
        public static System.IObservable<Unit> OnSecondTick => _onSecondTick; // Docfx가 IObservable만 쓰면 에러를 뱉음, System.IObservable로 표기 필요
        private static Subject<Unit> _onSecondTick = new Subject<Unit>();

        public static System.IObservable<Unit> OnMinuteTick => _onMinuteTick; // Docfx가 IObservable만 쓰면 에러를 뱉음, System.IObservable로 표기 필요
        private static Subject<Unit> _onMinuteTick = new Subject<Unit>();

        public static System.IObservable<Unit> OnHourTick => _onHourTick; // Docfx가 IObservable만 쓰면 에러를 뱉음, System.IObservable로 표기 필요
        private static Subject<Unit> _onHourTick = new Subject<Unit>();
        
        public static System.IObservable<Unit> OnGameDatePassed => _onGameDatePassed; // Docfx가 IObservable만 쓰면 에러를 뱉음, System.IObservable로 표기 필요
        private static Subject<Unit> _onGameDatePassed = new Subject<Unit>();

        public static DateTime Now => DateTime.UtcNow + TimeSpan.FromHours(9) + Offset + DebugOffset; // UTC+9
        public static DateTime NoDebugNow => DateTime.UtcNow + TimeSpan.FromHours(9) + Offset;
        
        public static GameDate GameDateNow => new GameDate(Now);
        public static GameDate GameDateNoDebugNow => new GameDate(NoDebugNow);

        internal static DateTime LastTick { get; private set; } = DateTime.MinValue;
        internal static bool IsTicking { get; private set; } = true;
        private static bool _isFirstTick = true;

        public static TimeSpan Offset
        {
            get => TimeSpan.FromSeconds(PlayerPrefs.GetInt(OFFSET_KEY, 0)); 
            private set => PlayerPrefs.SetInt(OFFSET_KEY, (int)value.TotalSeconds);
        }

        public static TimeSpan DebugOffset
        {
            get => TimeSpan.FromSeconds(PlayerPrefs.GetInt(DEBUG_OFFSET_KEY, 0)); 
            private set => PlayerPrefs.SetInt(DEBUG_OFFSET_KEY, (int)value.TotalSeconds);
        }
        
        private const string OFFSET_KEY = "Aloha.Coconut.Clock.Offset";
        private const string DEBUG_OFFSET_KEY = "Aloha.Coconut.Clock.DebugOffset";

        public static void SetIsTicking(bool isTicking)
        {
            IsTicking = isTicking;
        }
        
        public static void AddOffset(TimeSpan timeSpan)
        {
            Offset += timeSpan;
            Tick();
        }

        public static void AddDebugOffset(TimeSpan timeSpan)
        {
            DebugOffset += timeSpan;
            Tick();
        }

        public static void DebugSetNow(DateTime dateTime)
        {
            var offset = dateTime - Now;
            DebugOffset += offset;
            Tick();
        }
        
        public static void ResetDebugOffset()
        {
            DebugOffset = TimeSpan.Zero;
            Tick();
        }

        public static void Tick()
        {
            _onSecondTick.OnNext(UniRx.Unit.Default);
            if (Now.Minute != LastTick.Minute || Now.Hour != LastTick.Hour || Now.Date != LastTick.Date)
            {
                _onMinuteTick.OnNext(UniRx.Unit.Default);
            }

            if (Now.Hour != LastTick.Hour || Now.Date != LastTick.Date)
            {
                _onHourTick.OnNext(UniRx.Unit.Default);
            }
            
            if (!_isFirstTick && new GameDate(LastTick) != GameDateNow)
            {
                _onGameDatePassed.OnNext(UniRx.Unit.Default);
            }

            _isFirstTick = false;
            LastTick = Now;
        }
    }
    
    public class ClockComponent : MonoBehaviour
    {
        private float _secondTimer = 1f;
        private bool _skipFrame = false;

        void Update()
        {
            if (_skipFrame)
            {
                _skipFrame = false;
            }
            else
            {
                _secondTimer -= Time.unscaledDeltaTime;
                if (_secondTimer <= 0f && Clock.IsTicking)
                {
                    _secondTimer = 1f;
                    Clock.Tick();
                }
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus) _skipFrame = true;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus) _skipFrame = true;
        }
    }
}