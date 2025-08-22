using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace Aloha.Coconut
{
    public class PurchaseRewardsEventManager : IDisposable
    {
        public bool IsInitialized { get; private set; }
        public List<PurchaseRewardsEvent> ActiveEvents { get; } = new List<PurchaseRewardsEvent>();
        
        private PurchaseRewardsEvent.Factory _purchaseRewardsEventFactory;
        private readonly IIAPManager _iapManager;
        private readonly EventScheduleManager _eventScheduleManager;
        private readonly SaveData _saveData;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private const string EVENT_TYPE = "purchase_rewards";

        public PurchaseRewardsEventManager(SaveDataManager saveDataManager, IIAPManager iapManager,
            EventScheduleManager eventScheduleManager)
        {
            _iapManager = iapManager;
            _eventScheduleManager = eventScheduleManager;
            _saveData = saveDataManager.Get<SaveData>("purchase_rewards");
        }

        // Factory class들을 internal로 유지하기 위해 별도의 internal Inject method를 둠
        [Inject]
        internal void Inject(PurchaseRewardsEvent.Factory purchaseRewardsGroupFactory)
        {
            _purchaseRewardsEventFactory = purchaseRewardsGroupFactory;
            _iapManager.AddOnInitializedListener(InitializeEvents);
        }

        private void InitializeEvents(bool isInitialized)
        {
            if (!isInitialized)
            {
                Debug.Log("PurchaseRewardsEventManager :: IAPManager 초기화 실패");
                return;
            }
            
            IsInitialized = true;
            List<EventSchedule> activeEventSchedules = _eventScheduleManager.GetActiveEventSchedules(EVENT_TYPE);

            foreach (var eventSchedule in activeEventSchedules)
            {
                ActivateEvent(eventSchedule);
            }

            _eventScheduleManager.OnEventStarted
                .Where(ev => ev.Type.Equals(EVENT_TYPE))
                .Subscribe(ActivateEvent).AddTo(_disposables);
            
            _eventScheduleManager.OnEventEnded
                .Where(ev => ev.Type.Equals(EVENT_TYPE))
                .Subscribe(RemoveEvent).AddTo(_disposables);
        }

        private void ActivateEvent(EventSchedule eventSchedule)
        {
            if (!_saveData.eventSaveDatas.ContainsKey(eventSchedule.Id))
            {
                _saveData.eventSaveDatas.Add(eventSchedule.Id, new PurchaseRewardsEvent.SaveData());
            }
            
            PurchaseRewardsEvent newEvent = _purchaseRewardsEventFactory.Create(eventSchedule, _saveData.eventSaveDatas[eventSchedule.Id]);
            if (newEvent == null) return; // 지원하지 않는 currency code의 이벤트일 경우 null이 반환됨
            
            Assert.IsTrue(!ActiveEvents.Exists(ev => ev.Type == newEvent.Type), 
                $"PurchaseRewardsEventManager.ActivateEvent: Event of type {newEvent.Type} is already active");
            ActiveEvents.Add(newEvent);
        }

        private void RemoveEvent(EventSchedule eventSchedule)
        {
            PurchaseRewardsEvent targetEvent = ActiveEvents.Find(ev => ev.EventId == eventSchedule.Id);
            targetEvent.Dispose();
            ActiveEvents.Remove(targetEvent);
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
            public Dictionary<int, PurchaseRewardsEvent.SaveData> eventSaveDatas = new Dictionary<int, PurchaseRewardsEvent.SaveData>();
        }
    }
}
