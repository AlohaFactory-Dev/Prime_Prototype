using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Aloha.Coconut.Missions;
using UnityEngine.Assertions;

namespace Aloha.Coconut
{
    public class DefaultNewUserEventDatabase : INewUserEventDatabase
    {
        private struct NewUserEventPassNodeTableData
        {
            [CSVColumn] public int exp;
            [CSVColumn] public string reward1Alias;
            [CSVColumn] public BigInteger reward1Amount;
            [CSVColumn] public string reward2Alias;
            [CSVColumn] public BigInteger reward2Amount;
            [CSVColumn] public string reward3Alias;
            [CSVColumn] public BigInteger reward3Amount;
        }

        private struct NewUserMissionGroupTableData
        {
            [CSVColumn] public int day;
            [CSVColumn] public int missionId;
            [CSVColumn] public string missionType;
            [CSVColumn] public int var;
            [CSVColumn] public int objective;
            [CSVColumn] public int point;
            [CSVColumn] public string reward1Alias;
            [CSVColumn] public int reward1Amount;
            [CSVColumn] public string reward2Alias;
            [CSVColumn] public int reward2Amount;
        }

        private struct NewUserPackageGroupTableData
        {
            [CSVColumn] public int day;
            [CSVColumn] public int isFree;
            [CSVColumn] public string iapId;
            [CSVColumn] public int limit;
        }
        
        private readonly PropertyTypeGroup _expTypeGroup;
        private readonly string _redDotPath;

        public DefaultNewUserEventDatabase()
        {
            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/new_user_event_exp"));
            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/new_user_event_reddot_path"));
            
            _expTypeGroup = GameConfig.GetPropertyTypeGroup("coconut_bm/new_user_event_exp");
            _redDotPath = GameConfig.GetString("coconut_bm/new_user_event_reddot_path");
        }

        public List<PassNodeData> GetPassNodeDataList()
        {
            var tableData = TableManager.Get<NewUserEventPassNodeTableData>("bm_nue_rewards");
            var result = new List<PassNodeData>();

            foreach (var data in tableData)
            {
                result.Add(new PassNodeData
                {
                    passLevel = data.exp,
                    reward1Alias = data.reward1Alias,
                    reward1Amount = data.reward1Amount,
                    reward2Alias = data.reward2Alias,
                    reward2Amount = data.reward2Amount,
                    reward3Alias = data.reward3Alias,
                    reward3Amount = data.reward3Amount
                });
            }

            return result;
        }

        public List<NewUserMissionGroupData> GetMissionGroupDataList()
        {
            var tableData = TableManager.Get<NewUserMissionGroupTableData>("bm_nue_missions");
            var result = new List<NewUserMissionGroupData>();

            foreach (var data in tableData)
            {
                var rewards = new List<Property> { new(PropertyType.Get(_expTypeGroup, 1), data.point) };
                if (data.reward1Amount > 0)
                {
                    rewards.Add(new Property(data.reward1Alias, data.reward1Amount));
                }

                if (data.reward2Amount > 0)
                {
                    rewards.Add(new Property(data.reward2Alias, data.reward2Amount));
                }

                result.Add(new NewUserMissionGroupData
                {
                    day = data.day,
                    missionDataList = new List<MissionData>
                    {
                        new()
                        {
                            id = data.missionId,
                            type = Enum.TryParse<MissionType>(data.missionType, out var type)
                                ? type
                                : MissionType.Default,
                            var = data.var,
                            objective = data.objective,
                            rewards = rewards
                        }
                    }
                });
            }

            return result;
        }

        public List<NewUserPackageGroupData> GetPackageGroupDataList()
        {
            var tableData = TableManager.Get<NewUserPackageGroupTableData>("bm_nue_packages");
            var packageGroups = tableData.GroupBy(data => data.day)
                .ToDictionary(group => group.Key, group => group.ToList());
            var result = new List<NewUserPackageGroupData>();

            foreach (var (day, tableDatas) in packageGroups)
            {
                result.Add(new NewUserPackageGroupData()
                {
                    day = day,
                    packageDataList = tableDatas.Select(data => new NewUserPackageData()
                    {
                        isFree = data.isFree == 1,
                        iapId = data.iapId,
                        limit = data.limit
                    }).ToList()
                });
            }

            return result;
        }

        public PropertyTypeGroup GetExpTypeGroup()
        {
            return _expTypeGroup;
        }
        
        public string GetRedDotPath()
        {
            return _redDotPath;
        }
    }
}