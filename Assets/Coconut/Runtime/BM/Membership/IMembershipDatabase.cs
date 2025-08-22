using System.Collections.Generic;

namespace Aloha.Coconut
{
    public interface IMembershipDatabase
    {
        public List<MembershipData> GetMembershipDataList();
        public PropertyTypeGroup GetMembershipTypeGroup();
        public string GetRedDotPath();
    }

    public struct MembershipData
    {
        public int id;
        public string iapId;
        public List<Property> dailyRewards;
        public List<MembershipPrivilege> privileges;

        public MembershipData(int id, string iapId, List<Property> dailyRewards, List<MembershipPrivilege> privileges)
        {
            this.id = id;
            this.iapId = iapId;
            this.dailyRewards = dailyRewards;
            this.privileges = privileges;
        }
    }

    public struct MembershipPrivilege
    {
        public string type;
        public string descriptionKey;

        public MembershipPrivilege(string type, string descriptionKey)
        {
            this.type = type;
            this.descriptionKey = descriptionKey;
        }
    }
}