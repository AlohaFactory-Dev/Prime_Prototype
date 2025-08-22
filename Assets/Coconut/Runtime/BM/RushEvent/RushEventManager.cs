using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using Zenject;

namespace Aloha.Coconut
{
    public class RushEventManager : IDisposable
    {
        public List<RushEvent> ActiveEvents { get; private set; }

        private readonly EventScheduleManager _eventScheduleManager;
        private readonly IRushEventDatabase _rushEventDatabase;
        private readonly IRushEventProgressHandler _rushEventProgressHandler;
        private readonly IRushEventLeaderboardAdapter _leaderboardAdapter;
        private RushEvent.Factory _rushEventFactory;

        private readonly SaveData _saveData;

        private const string EVENT_TYPE = "rush";

        private CompositeDisposable _disposables = new();

        public RushEventManager(IRushEventDatabase rushEventDatabase, SaveDataManager saveDataManager,
            EventScheduleManager eventScheduleManager, IRushEventProgressHandler rushEventProgressHandler,
            [InjectOptional] IRushEventLeaderboardAdapter leaderboardAdapter)
        {
            _rushEventDatabase = rushEventDatabase;
            _eventScheduleManager = eventScheduleManager;
            _rushEventProgressHandler = rushEventProgressHandler;
            _leaderboardAdapter = leaderboardAdapter;

            _saveData = saveDataManager.Get<SaveData>("rush_event_manager");
        }

        [Inject]
        internal void Inject(RushEvent.Factory rushEventFactory)
        {
            _rushEventFactory = rushEventFactory;
            
            ActiveEvents = new List<RushEvent>();
            var activeEventSchedules = _eventScheduleManager.GetActiveEventSchedules(EVENT_TYPE);

            foreach (var eventSchedule in activeEventSchedules)
            {
                var eventData = _rushEventDatabase.GetRushEventData(eventSchedule.Var);
                var saveData = _saveData.rushEventSaveDatas.TryGetValue(eventSchedule.Id, out var data)
                    ? data
                    : new RushEvent.SaveData();
                _saveData.rushEventSaveDatas.TryAdd(eventSchedule.Id, saveData);

                ActiveEvents.Add(rushEventFactory.Create(eventSchedule.Id, eventData, saveData));
            }

            _eventScheduleManager.OnEventStarted
                .Where(ev => ev.Type == EVENT_TYPE)
                .Subscribe(OnEventStarted).AddTo(_disposables);

            _eventScheduleManager.OnEventEnded
                .Where(ev => ev.Type == EVENT_TYPE)
                .Subscribe(ev => { OnEventEnded(ev.Id); }).AddTo(_disposables);

            _rushEventProgressHandler.OnProgressAdded
                .Subscribe(args =>
                {
                    foreach (var activeEvent in ActiveEvents)
                    {
                        if (activeEvent.TargetAction.Equals(args.action))
                        {
                            activeEvent.MissionGroup.AddProgress(args.progress);
                            _leaderboardAdapter?.UpdateScore(activeEvent).Forget();
                        }
                    }
                }).AddTo(_disposables);
        }

        private void OnEventStarted(EventSchedule eventSchedule)
        {
            var saveData = new RushEvent.SaveData();
            RushEvent rushEvent =_rushEventFactory.Create(eventSchedule.Id, _rushEventDatabase.GetRushEventData(eventSchedule.Var), saveData);
            ActiveEvents.Add(rushEvent);
            _saveData.rushEventSaveDatas[eventSchedule.Id] = saveData;
        }

        private void OnEventEnded(int eventId)
        {
            foreach (var activeEvent in ActiveEvents)
            {
                if (activeEvent.EventScheduleId != eventId) continue;

                activeEvent.Dispose();
                ActiveEvents.Remove(activeEvent);
                break;
            }
        }

        public void Dispose()
        {
            foreach (var activeEvent in ActiveEvents)
            {
                activeEvent.Dispose();
            }

            _disposables.Dispose();
        }

        private class SaveData
        {
            public Dictionary<int, RushEvent.SaveData> rushEventSaveDatas = new();
        }
    }
}