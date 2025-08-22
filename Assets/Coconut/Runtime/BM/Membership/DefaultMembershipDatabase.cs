using System.Collections.Generic;
using System.Numerics;
using UnityEngine.Assertions;

namespace Aloha.Coconut
{
    public class DefaultMembershipDatabase : IMembershipDatabase
    {
        private struct MembershipTableData
        {
            [CSVColumn] public int id;
            [CSVColumn] public string iapId;
            [CSVColumn] public PropertyTypeAlias dailyReward1AliasType;
            [CSVColumn] public BigInteger dailyReward1Amount;
            [CSVColumn] public PropertyTypeAlias dailyReward2AliasType;
            [CSVColumn] public BigInteger dailyReward2Amount;
            [CSVColumn] public PropertyTypeAlias dailyReward3AliasType;
            [CSVColumn] public BigInteger dailyReward3Amount;
            [CSVColumn] public string pv1;
            [CSVColumn] public string pv1Desc;
            [CSVColumn] public string pv2;
            [CSVColumn] public string pv2Desc;
            [CSVColumn] public string pv3;
            [CSVColumn] public string pv3Desc;
            [CSVColumn] public string pv4;
            [CSVColumn] public string pv4Desc;
            [CSVColumn] public string pv5;
            [CSVColumn] public string pv5Desc;

            public List<Property> GetDailyRewards()
            {
                List<Property> dailyRewards = new List<Property>();

                if (dailyReward1Amount > 0)
                {
                    dailyRewards.Add(new Property(dailyReward1AliasType, dailyReward1Amount));
                }

                if (dailyReward2Amount > 0)
                {
                    dailyRewards.Add(new Property(dailyReward2AliasType, dailyReward2Amount));
                }

                if (dailyReward3Amount > 0)
                {
                    dailyRewards.Add(new Property(dailyReward3AliasType, dailyReward3Amount));
                }

                return dailyRewards;
            }

            public List<MembershipPrivilege> GetPrivileges()
            {
                List<MembershipPrivilege> privileges = new List<MembershipPrivilege>();

                if (!string.IsNullOrEmpty(pv1))
                {
                    privileges.Add(new MembershipPrivilege(pv1, pv1Desc));
                }

                if (!string.IsNullOrEmpty(pv2))
                {
                    privileges.Add(new MembershipPrivilege(pv2, pv2Desc));
                }

                if (!string.IsNullOrEmpty(pv3))
                {
                    privileges.Add(new MembershipPrivilege(pv3, pv3Desc));
                }

                if (!string.IsNullOrEmpty(pv4))
                {
                    privileges.Add(new MembershipPrivilege(pv4, pv4Desc));
                }

                if (!string.IsNullOrEmpty(pv5))
                {
                    privileges.Add(new MembershipPrivilege(pv5, pv5Desc));
                }

                return privileges;
            }
        }

        private readonly PropertyTypeGroup _membershipTypeGroup;
        private readonly string _redDotPath;
        
        public DefaultMembershipDatabase()
        {
            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/membership_type_group"));
            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/membership_red_dot_path"));
            
            _membershipTypeGroup = GameConfig.GetPropertyTypeGroup("coconut_bm/membership_type_group");
            _redDotPath = GameConfig.GetString("coconut_bm/membership_red_dot_path");
        }

        public List<MembershipData> GetMembershipDataList()
        {
            var tableData = TableManager.Get<MembershipTableData>("bm_membership");
            var result = new List<MembershipData>();
            foreach (var data in tableData)
            {
                result.Add(new MembershipData(data.id, data.iapId, data.GetDailyRewards(), data.GetPrivileges()));
            }

            return result;
        }

        public PropertyTypeGroup GetMembershipTypeGroup()
        {
            return _membershipTypeGroup;
        }

        public string GetRedDotPath()
        {
            return _redDotPath;
        }
    }
}