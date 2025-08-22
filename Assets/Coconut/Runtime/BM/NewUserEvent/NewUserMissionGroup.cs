using System.Collections.Generic;
using Aloha.Coconut.Missions;

namespace Aloha.Coconut
{
    public class NewUserMissionGroup
    {
        public int Day { get; private set; }
        public List<Mission> MissionList { get; }
        public bool IsMissionStarted { get; private set; }

        private NewUserMissionGroup(int day, List<Mission> missionList)
        {
            Day = day;
            MissionList = missionList;
        }

        public void StartMissions()
        {
            foreach (var mission in MissionList)
            {
                mission.Start();
            }

            IsMissionStarted = true;
        }

        internal class Factory
        {
            private readonly INewUserEventDatabase _database;
            private readonly MissionFactory _missionFactory;
            private readonly INewUserEventDatabase _newUserEventDatabase;

            public Factory(MissionFactory missionFactory, INewUserEventDatabase newUserEventDatabase)
            {
                _missionFactory = missionFactory;
                _newUserEventDatabase = newUserEventDatabase;
            }

            public NewUserMissionGroup Create(NewUserMissionGroupData data, SaveData saveData)
            {
                var missionList = new List<Mission>();
                foreach (var missionData in data.missionDataList)
                {
                    var missionSaveData = saveData.missionSaveDatas.TryGetValue(missionData.id, out var missionSave)
                        ? missionSave
                        : new Mission.SaveData();
                    saveData.missionSaveDatas.TryAdd(missionData.id, missionSaveData);

                    var mission = _missionFactory.Create(missionData, missionSaveData);
                    mission.LinkRedDot($"{_newUserEventDatabase.GetRedDotPath()}/Mission/{data.day}/{missionData.id}");
                    missionList.Add(mission);
                }

                return new NewUserMissionGroup(data.day, missionList);
            }
        }

        internal class SaveData
        {
            public Dictionary<int, Mission.SaveData> missionSaveDatas = new();
        }
    }
}