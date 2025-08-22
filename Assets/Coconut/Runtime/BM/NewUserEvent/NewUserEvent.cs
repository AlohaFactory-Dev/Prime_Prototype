using System;
using System.Collections.Generic;
using System.Numerics;
using Aloha.Coconut.IAP;
using UniRx;
using UnityEngine;
using Zenject;

namespace Aloha.Coconut
{
    public class NewUserEvent : IPropertyHandler, IDisposable
    {
        public Pass ProgressPass { get; private set; }
        public List<NewUserMissionGroup> MissionGroupList { get; private set; }
        public List<NewUserPackageGroup> PackageGroupList { get; private set; }
        public GameDate StartDate => _saveData.startDate;
        public GameDate EndDate => _saveData.startDate.AddDay(EVENT_PERIOD);
        public bool IsActivated => Clock.GameDateNow < EndDate;
        public string RedDotPath => _database.GetRedDotPath();

        private const int EVENT_PERIOD = 10;

        private readonly SaveData _saveData;
        private readonly INewUserEventDatabase _database;

        private IDisposable _dayPassedSubscription;

        public NewUserEvent(SaveDataManager saveDataManager, Pass.Factory passFactory, INewUserEventDatabase database)
        {
            _saveData = saveDataManager.Get<SaveData>("NewUserEvent");
            _database = database;
            
            HandlingGroups = new List<PropertyTypeGroup> { _database.GetExpTypeGroup() };

            // 이벤트 시작일이 없으면 현재 시간으로 설정
            _saveData.startDate = _saveData.startDate == default ? Clock.GameDateNow : _saveData.startDate;

            var passNodeDataList = _database.GetPassNodeDataList();
            var passNodes = passNodeDataList.ConvertAll(nodeData => new PassNode(nodeData));
            if(_saveData.progressPassSaveData == null)
            {
                _saveData.progressPassSaveData = new Pass.SaveData();
                _saveData.progressPassSaveData.exp = -1;
            }
            var passSaveData = _saveData.progressPassSaveData;
            ProgressPass = passFactory.Create(0, passNodes, passSaveData);
        }

        [Inject]
        internal void Inject(IIAPManager iapManager, NewUserMissionGroup.Factory missionGroupFactory, 
            NewUserPackageGroup.Factory packageGroupFactory)
        {
            MissionGroupList = new List<NewUserMissionGroup>();
            foreach (var missionGroupData in _database.GetMissionGroupDataList())
            {
                var saveData = _saveData.missionGroupSaveDatas.TryGetValue(missionGroupData.day, out var data)
                    ? data
                    : new NewUserMissionGroup.SaveData();

                _saveData.missionGroupSaveDatas.TryAdd(missionGroupData.day, saveData);
                
                // 이벤트 종료일이 지났으면 미션 생성하지 않음
                if (EndDate < Clock.GameDateNow) return;

                var missionGroup = missionGroupFactory.Create(missionGroupData, saveData);
                // 정해진 일차에 도달했을 때 미션 시작 / 첫 시작 : 1일차
                if ((Clock.GameDateNow.Date - _saveData.startDate.Date).Days + 1 >= missionGroupData.day)
                {
                    missionGroup.StartMissions();
                }

                MissionGroupList.Add(missionGroup);
            }

            _dayPassedSubscription = Clock.OnGameDatePassed.Subscribe(_ => OnDayPassed());

            PackageGroupList = new List<NewUserPackageGroup>();
            iapManager.AddOnInitializedListener(result =>
            {
                if (!result)
                {
                    Debug.Log("NewUserEvent :: IAPManager 초기화 실패");
                    return;
                }
                
                foreach (var packageGroupData in _database.GetPackageGroupDataList())
                {
                    var saveData = _saveData.packageGroupSaveDatas.TryGetValue(packageGroupData.day, out var data)
                        ? data
                        : new NewUserPackageGroup.SaveData();
                    _saveData.packageGroupSaveDatas.TryAdd(packageGroupData.day, saveData);
                    var packageGroup = packageGroupFactory.Create(packageGroupData, saveData);
                    PackageGroupList.Add(packageGroup);
                }
            });
        }

        private void OnDayPassed()
        {
            if (EndDate < Clock.GameDateNow)
            {
                _dayPassedSubscription.Dispose();
                return;
            }

            foreach (var missionGroup in MissionGroupList)
            {
                if (missionGroup.IsMissionStarted) continue;
                if ((Clock.GameDateNow.Date - _saveData.startDate.Date).Days + 1 >= missionGroup.Day)
                {
                    missionGroup.StartMissions();
                }
            }
        }

        public void Dispose()
        {
            _dayPassedSubscription?.Dispose();
        }

        private class SaveData
        {
            public GameDate startDate;
            public Pass.SaveData progressPassSaveData;
            public Dictionary<int, NewUserMissionGroup.SaveData> missionGroupSaveDatas = new();
            public Dictionary<int, NewUserPackageGroup.SaveData> packageGroupSaveDatas = new();
        }

        public List<PropertyTypeGroup> HandlingGroups { get; }

        public void Obtain(Property property)
        {
            if (HandlingGroups.Contains(property.type.group))
            {
                ProgressPass.AddExp((int)property.amount);
            }
        }

        public void Use(Property property) { }

        public void Set(Property property) { }

        public BigInteger GetBalance(PropertyType property)
        {
            return 0;
        }
    }
}