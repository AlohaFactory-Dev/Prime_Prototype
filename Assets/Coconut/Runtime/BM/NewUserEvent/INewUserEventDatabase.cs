using System.Collections.Generic;
using Aloha.Coconut.Missions;

namespace Aloha.Coconut
{
    public interface INewUserEventDatabase
    {
        public List<PassNodeData> GetPassNodeDataList();
        public List<NewUserMissionGroupData> GetMissionGroupDataList();
        public List<NewUserPackageGroupData> GetPackageGroupDataList();
        public PropertyTypeGroup GetExpTypeGroup();
        public string GetRedDotPath();
    }

    public struct NewUserMissionGroupData
    {
        public int day;
        public List<MissionData> missionDataList;
    }

    public struct NewUserPackageGroupData
    {
        public int day;
        public List<NewUserPackageData> packageDataList;
    }
    
    public struct NewUserPackageData
    {
        public bool isFree;
        public string iapId;
        public int limit;
    }
}