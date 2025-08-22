using System;
using System.Collections.Generic;
using UniRx;
using Zenject;

namespace Aloha.Coconut
{
    public class EventScheduleManager : IDisposable
    {
        public GameDate FirstDate => _saveData.firstDate;
        
        public IObservable<EventSchedule> OnEventStarted => _onEventStarted;
        private readonly Subject<EventSchedule> _onEventStarted = new Subject<EventSchedule>();
        
        public IObservable<EventSchedule> OnEventEnded => _onEventEnded;
        private readonly Subject<EventSchedule> _onEventEnded = new Subject<EventSchedule>();
        
        private readonly List<EventSchedule> _eventSchedules = new List<EventSchedule>();
        private readonly List<EventSchedule> _activeEventSchedules = new List<EventSchedule>();
        
        private readonly SaveData _saveData;
        private readonly PeriodicResetHandler _periodicResetHandler;
        
        public EventScheduleManager([InjectOptional] IEventScheduleDatabase eventScheduleDatabase, 
            SaveDataManager saveDataManager, PeriodicResetHandler periodicResetHandler)
        {
            _saveData = saveDataManager.Get<SaveData>("event_schedule_manager");
            if (_saveData.firstDate.Year == 0) // 처음 생성됐을 경우
            {
                _saveData.firstDate = Clock.GameDateNow;
            }
            
            GameDate gameDateNow = Clock.GameDateNow;
            if (eventScheduleDatabase == null) eventScheduleDatabase = new DefaultEventScheduleDatabase();
            
            foreach (EventScheduleData eventScheduleData in eventScheduleDatabase.GetEventScheduleDatas())
            {
                EventSchedule eventSchedule = new EventSchedule(eventScheduleData, _saveData.firstDate);
                _eventSchedules.Add(eventSchedule);
                if (eventSchedule.From <= gameDateNow && gameDateNow <= eventSchedule.To)
                {
                    _activeEventSchedules.Add(eventSchedule);
                }
            }
            
            // Assert that there is no duplicated event schedule id
            HashSet<int> eventScheduleIds = new HashSet<int>();
            foreach (EventSchedule eventSchedule in _eventSchedules)
            {
                if (!eventScheduleIds.Add(eventSchedule.Id))
                {
                    throw new Exception($"Duplicated event schedule id: {eventSchedule.Id}");
                }
            }
            
            _periodicResetHandler = periodicResetHandler;
            _periodicResetHandler.AddResetCallback(ResetPeriod.Daily, "event_schedule_manager", OnDayReset);
        }

        private void OnDayReset()
        {
            GameDate gameDateNow = Clock.GameDateNow;
            
            List<EventSchedule> newActiveEventSchedules = new List<EventSchedule>();
            foreach (EventSchedule eventSchedule in _eventSchedules)
            {
                if (eventSchedule.From <= gameDateNow && gameDateNow <= eventSchedule.To)
                {
                    newActiveEventSchedules.Add(eventSchedule);
                }
            }
            
            foreach (EventSchedule activeEventSchedule in _activeEventSchedules)
            {
                if (!newActiveEventSchedules.Contains(activeEventSchedule))
                {
                    _onEventEnded.OnNext(activeEventSchedule);
                }
            }
            
            foreach (EventSchedule newActiveEventSchedule in newActiveEventSchedules)
            {
                if (!_activeEventSchedules.Contains(newActiveEventSchedule))
                {
                    _onEventStarted.OnNext(newActiveEventSchedule);
                }
            }
            
            _activeEventSchedules.Clear();
            _activeEventSchedules.AddRange(newActiveEventSchedules);
        }
        
        public List<EventSchedule> GetActiveEventSchedules(string type)
        {
            return _activeEventSchedules.FindAll(schedule => schedule.Type == type);
        }

        public void Dispose()
        {
            _onEventStarted?.Dispose();
            _onEventEnded?.Dispose();
            _periodicResetHandler.RemoveResetCallback(ResetPeriod.Daily, "event_schedule_manager");
        }

        private class SaveData
        {
            public GameDate firstDate;
        }
    }
}
