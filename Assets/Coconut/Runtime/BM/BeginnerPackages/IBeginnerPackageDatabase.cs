using System.Collections.Generic;

namespace Aloha.Coconut
{
    public interface IBeginnerPackageDatabase
    {
        public List<BeginnerPackageData> GetBeginnerPackageDataList();
        public PropertyTypeGroup GetBeginnerPackageTypeGroup();
        public string GetRedDotPath();
    }

    public struct BeginnerPackageData
    {
        public int id;
        public string iapId;
        public List<BeginnerPackageComponentData> componentDataList;

        public BeginnerPackageData(int id, string iapId, List<BeginnerPackageComponentData> componentDataList)
        {
            this.id = id;
            this.iapId = iapId;
            this.componentDataList = componentDataList;
        }
    }

    public struct BeginnerPackageComponentData
    {
        public readonly int day;
        public readonly List<Property> rewards;

        public BeginnerPackageComponentData(int day, List<Property> rewards)
        {
            this.day = day;
            this.rewards = rewards;
        }
    }
}