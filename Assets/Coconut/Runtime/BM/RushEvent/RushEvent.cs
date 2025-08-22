using System.Collections.Generic;
using Aloha.Durian;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Aloha.Coconut
{
    public struct RushEventData
    {
        public string targetAction;
        public RushEventMissionGroupData missionGroupData;
        public RushEventPackageGroupData packageGroupData;
    }

    public class RushEvent
    {
        public int EventScheduleId { get; private set; }
        public RushEventMissionGroup MissionGroup { get; }
        public RushEventPackageGroup PackageGroup { get; }
        public string TargetAction { get; }

        private readonly SaveData _saveData;
        private readonly IRushEventLeaderboardAdapter _leaderboardAdapter;

        private RushEvent(int eventScheduleId, RushEventMissionGroup missionGroup, RushEventPackageGroup packageGroup,
            string targetAction, SaveData saveData, IRushEventLeaderboardAdapter leaderboardAdapter)
        {
            EventScheduleId = eventScheduleId;
            MissionGroup = missionGroup;
            PackageGroup = packageGroup;
            TargetAction = targetAction;

            _saveData = saveData;
            _leaderboardAdapter = leaderboardAdapter;
        }
        
        public UniTask<Leaderboard> GetLeaderboard()
        {
            return _leaderboardAdapter.GetLeaderboard(this);
        }
        
        public UniTask<List<LeaderboardReward>> GetRewardTable()
        {
            return _leaderboardAdapter.GetRewardTable(this);
        }
        
        public void Dispose()
        {
            MissionGroup.Dispose();
        }

        internal class SaveData
        {
            public RushEventMissionGroup.SaveData missionGroupSaveData;
            public RushEventPackageGroup.SaveData packageGroupSaveData;
        }

        internal class Factory
        {
            private readonly RushEventMissionGroup.Factory _rushEventMissionGroupFactory;
            private readonly RushEventPackageGroup.Factory _rushEventPackageGroupFactory;
            private readonly IRushEventDatabase _rushEventDatabase;
            private readonly IRushEventLeaderboardAdapter _leaderboardAdapter;

            public Factory(RushEventMissionGroup.Factory rushEventMissionGroupFactory,
                RushEventPackageGroup.Factory rushEventPackageGroupFactory, IRushEventDatabase rushEventDatabase,
                [InjectOptional] IRushEventLeaderboardAdapter leaderboardAdapter)
            {
                _rushEventMissionGroupFactory = rushEventMissionGroupFactory;
                _rushEventPackageGroupFactory = rushEventPackageGroupFactory;
                _rushEventDatabase = rushEventDatabase;
                _leaderboardAdapter = leaderboardAdapter;
            }

            public RushEvent Create(int eventScheduleId, RushEventData eventData, SaveData saveData)
            {
                var missionGroupSaveData = saveData.missionGroupSaveData;
                if (missionGroupSaveData == null)
                {
                    missionGroupSaveData = new RushEventMissionGroup.SaveData();
                    saveData.missionGroupSaveData = missionGroupSaveData;
                }

                var missionGroup = _rushEventMissionGroupFactory.Create(eventData.missionGroupData, missionGroupSaveData);
                missionGroup.LinkRedDot($"{_rushEventDatabase.GetRedDotPath()}/Mission");

                var packageGroupSaveData = saveData.packageGroupSaveData;
                if (packageGroupSaveData == null)
                {
                    packageGroupSaveData = new RushEventPackageGroup.SaveData();
                    saveData.packageGroupSaveData = packageGroupSaveData;
                }

                var packageGroup = _rushEventPackageGroupFactory.Create(eventData.packageGroupData, packageGroupSaveData);

                return new RushEvent(eventScheduleId, missionGroup, packageGroup, eventData.targetAction, saveData, _leaderboardAdapter);
            }
        }
    }
}