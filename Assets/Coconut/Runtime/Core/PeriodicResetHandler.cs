using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Aloha.Coconut
{
    public enum ResetPeriod
    {
        Daily,
        Weekly,
        Monthly
    }
    
    public class PeriodicResetHandler : IDisposable
    {
        private IEnumerable<ResetPeriod> _periods = new List<ResetPeriod>() { ResetPeriod.Daily, ResetPeriod.Weekly, ResetPeriod.Monthly };

        private Dictionary<ResetPeriod, Dictionary<string, Action>> _resetCallbacks = new ()
        {
            {ResetPeriod.Daily, new Dictionary<string, Action>()},
            {ResetPeriod.Weekly, new Dictionary<string, Action>()},
            {ResetPeriod.Monthly, new Dictionary<string, Action>()}
        };

        private bool _isInitialized;
        private Dictionary<ResetPeriod, bool> _isChecking = new ()
        {
            {ResetPeriod.Daily, false},
            {ResetPeriod.Weekly, false},
            {ResetPeriod.Monthly, false}
        };

        private IDisposable _clockSubscription;
        private readonly SimpleValues _simpleValues;
        private SaveData _saveData;
        
        private Queue<(ResetPeriod, string, Action)> _addBuffer = new();
        private Queue<(ResetPeriod, string)> _removeBuffer = new();
        
        public PeriodicResetHandler(SaveDataManager saveDataManager, SimpleValues simpleValues)
        {
            _simpleValues = simpleValues;

            _saveData = saveDataManager.Get<SaveData>("periodic_reset_manager");
            if(!_saveData.invokedCallbacks.ContainsKey(ResetPeriod.Daily)) _saveData.invokedCallbacks[ResetPeriod.Daily] = new List<string>();
            if(!_saveData.invokedCallbacks.ContainsKey(ResetPeriod.Weekly)) _saveData.invokedCallbacks[ResetPeriod.Weekly] = new List<string>();
            if(!_saveData.invokedCallbacks.ContainsKey(ResetPeriod.Monthly)) _saveData.invokedCallbacks[ResetPeriod.Monthly] = new List<string>();
            
            foreach (var period in _periods)
            {
                if (!_simpleValues.HaveDateTime(GetLastResetTimeKey(period))) _simpleValues.SetDateTime(GetLastResetTimeKey(period), DateTime.MinValue);
            }

            CheckToReset();
            _clockSubscription = Clock.OnSecondTick.Subscribe(_ =>
            {
                CheckToReset();
            });

            _isInitialized = true;
        }

        private string GetLastResetTimeKey(ResetPeriod resetPeriod)
        {
            return $"periodic_reset_{resetPeriod}";
        }

        private void CheckToReset()
        {
            var now = Clock.Now;
            foreach (var period in _periods)
            {
                if (now > GetNextResetTime(period))
                {
                    _isChecking[period] = true;
                    _saveData.invokedCallbacks[period].Clear();
        
                    foreach (var pair in _resetCallbacks[period])
                    {
                        pair.Value.Invoke();
                        _saveData.invokedCallbacks[period].Add(pair.Key);
                        Debug.Log($"Invoke Callback : {pair.Key}");
                    }
                
                    _simpleValues.SetDateTime(GetLastResetTimeKey(period), Clock.Now);
                    _isChecking[period] = false;
                    
                    while (_addBuffer.Count > 0)
                    {
                        var (addPeriod, addId, addCallback) = _addBuffer.Dequeue();
                        AddResetCallback(addPeriod, addId, addCallback);
                    }
                    
                    while (_removeBuffer.Count > 0)
                    {
                        var (removePeriod, removeId) = _removeBuffer.Dequeue();
                        RemoveResetCallback(removePeriod, removeId);
                    }
                }
            }
        }

        private DateTime GetNextResetTime(ResetPeriod resetPeriod)
        {
            var lastResetTime = _simpleValues.GetDateTime(GetLastResetTimeKey(resetPeriod), Clock.Now);
            switch (resetPeriod)
            {
                case ResetPeriod.Daily:
                    if (lastResetTime.Hour < Clock.RESET_TIME)
                    {
                        return lastResetTime.Date.AddHours(Clock.RESET_TIME);
                    }
                    return lastResetTime.Date.AddDays(1).AddHours(Clock.RESET_TIME);
            
                case ResetPeriod.Weekly:
                    // DayOfWeek은 Sunday를 0으로 해서 시작됨
                    if (lastResetTime.DayOfWeek == DayOfWeek.Sunday)
                    {
                        return lastResetTime.Date.AddDays(1).AddHours(Clock.RESET_TIME);
                    }
                    if (lastResetTime.DayOfWeek == DayOfWeek.Monday && lastResetTime.Hour < Clock.RESET_TIME)
                    {
                        return lastResetTime.Date.AddHours(Clock.RESET_TIME);   
                    }
                    var daysUntilNextMonday = (int)DayOfWeek.Monday - (int)lastResetTime.DayOfWeek + 7;
                    return lastResetTime.Date.AddDays(daysUntilNextMonday).AddHours(Clock.RESET_TIME);
            
                case ResetPeriod.Monthly:
                    if (lastResetTime.Day == 1 && lastResetTime.Hour < Clock.RESET_TIME)
                    {
                        return lastResetTime.Date.AddHours(Clock.RESET_TIME);
                    }
                    return lastResetTime.Date.AddDays(-lastResetTime.Day + 1).AddMonths(1).AddHours(Clock.RESET_TIME);
            
                default:
                    throw new ArgumentOutOfRangeException(nameof(resetPeriod), resetPeriod, null);
            }
        }
    
        public TimeSpan GetRemainingTime(ResetPeriod resetPeriod)
        {
            return GetNextResetTime(resetPeriod) - Clock.Now;
        }

        public void AddResetCallback(ResetPeriod resetPeriod, string id, Action callback)
        {
            if (_isChecking[resetPeriod])
            {
                // CheckReset 중에는 _resetCallbacks를 순회하므로, 중간에 Add되면 Collection was modified 에러 발생 
                _addBuffer.Enqueue((resetPeriod, id, callback));
                return;
            }
            if (_resetCallbacks[resetPeriod].ContainsKey(id)) return;
        
            if (_isInitialized && !_saveData.invokedCallbacks[resetPeriod].Contains(id))
            {
                callback.Invoke();
                _saveData.invokedCallbacks[resetPeriod].Add(id);
                Debug.Log($"Invoke Callback : {id}");
            }
        
            _resetCallbacks[resetPeriod][id] = callback;
            Debug.Log($"{id} Added To {resetPeriod} EventCallback");
        }
    
        public void RemoveResetCallback(ResetPeriod resetPeriod, string id)
        {
            if (_isChecking[resetPeriod])
            {
                // CheckReset 중에는 _resetCallbacks를 순회하므로, 중간에 Remove되면 Collection was modified 에러 발생
                _removeBuffer.Enqueue((resetPeriod, id));
                return;
            }
            if (!_resetCallbacks[resetPeriod].ContainsKey(id)) return;
            _resetCallbacks[resetPeriod].Remove(id);
        }

        public void RemoveFromInvokedList(ResetPeriod resetPeriod, string id)
        {
            if (!_saveData.invokedCallbacks[resetPeriod].Contains(id)) return;
            _saveData.invokedCallbacks[resetPeriod].Remove(id);
        }

        public void InitializeLastResetTime()
        {
            foreach (var period in _periods)
            {
                _simpleValues.SetDateTime(GetLastResetTimeKey(period), DateTime.MinValue);
            }
        }

        public void Dispose()
        {
            _clockSubscription?.Dispose();
        }

        private class SaveData
        {
            public Dictionary<ResetPeriod, List<string>> invokedCallbacks = new();
        }
    }
}