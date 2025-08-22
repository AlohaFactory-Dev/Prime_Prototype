using System.Collections.Generic;
using System.Numerics;
using UnityEngine.Assertions;

namespace Aloha.Coconut
{
    public class DefaultRushEventDatabase : IRushEventDatabase
    {
        private struct RushEventTableData
        {
            [CSVColumn] public int eventVar;
            [CSVColumn] public string targetAction;
            [CSVColumn] public int totalRound;
            [CSVColumn] public string leaderboardName;
            [CSVColumn] public PropertyTypeAlias roundReward1Type;
            [CSVColumn] public BigInteger roundReward1Amount;
            [CSVColumn] public PropertyTypeAlias roundReward2Type;
            [CSVColumn] public BigInteger roundReward2Amount;
            [CSVColumn] public PropertyTypeAlias roundReward3Type;
            [CSVColumn] public BigInteger roundReward3Amount;
        }

        private struct RushEventMissionTableData
        {
            [CSVColumn] public int eventVar;
            [CSVColumn] public int objective;
            [CSVColumn] public PropertyTypeAlias reward1Type;
            [CSVColumn] public BigInteger reward1Amount;
            [CSVColumn] public PropertyTypeAlias reward2Type;
            [CSVColumn] public BigInteger reward2Amount;
            [CSVColumn] public PropertyTypeAlias reward3Type;
            [CSVColumn] public BigInteger reward3Amount;
        }
        
        private struct RushEventPackageTableData
        {
            [CSVColumn] public int eventVar;
            [CSVColumn] public int productId;
            [CSVColumn] public string packageId;
            [CSVColumn] public int limit;
            [CSVColumn] public int isFree;
        }
        
        private Dictionary<int, RushEventData> _rushEventDataById = new Dictionary<int, RushEventData>();

        private readonly string _redDotPath;
        
        public DefaultRushEventDatabase()
        {
            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/rush_event_red_dot_path"));
            _redDotPath = GameConfig.GetString("coconut_bm/rush_event_red_dot_path");
        }
        
        public RushEventData GetRushEventData(int rushEventVar)
        {
            if (_rushEventDataById.TryGetValue(rushEventVar, out var rushEventData))
            {
                return rushEventData;
            }

            var rushEventTableDataList = TableManager.Get<RushEventTableData>("bm_rush");
            foreach (var data in rushEventTableDataList)
            {
                _rushEventDataById[data.eventVar] = new RushEventData
                {
                    targetAction = data.targetAction,
                    missionGroupData = GetRushEventMissionGroupData(data),
                    packageGroupData = GetRushEventPackageGroupData(data)
                };
            }

            return _rushEventDataById[rushEventVar];
        }

        private RushEventMissionGroupData GetRushEventMissionGroupData(RushEventTableData data)
        {
            var missionTableDataList = TableManager.Get<RushEventMissionTableData>("bm_rush_missions");
            var missionDataList = missionTableDataList.FindAll(missionData => missionData.eventVar == data.eventVar);
            Assert.IsTrue(missionDataList.Count > 0);
            
            var targetEventMissionData = new List<RushEventMissionData>();
            foreach (var missionData in missionDataList)
            {
                var rewards = new List<Property>
                {
                    new(PropertyType.Get(missionData.reward1Type), missionData.reward1Amount),
                };
                if (missionData.reward2Amount > 0)
                {
                    rewards.Add(new Property(PropertyType.Get(missionData.reward2Type), missionData.reward2Amount));
                }
                if (missionData.reward3Amount > 0)
                {
                    rewards.Add(new Property(PropertyType.Get(missionData.reward3Type), missionData.reward3Amount));
                }
                    
                targetEventMissionData.Add(new RushEventMissionData
                {
                    objective = missionData.objective,
                    rewards = rewards,
                });
            }

            // 라운드 보상은 3개로 고정
            var roundRewards = new List<Property>
            {
                new(PropertyType.Get(data.roundReward1Type), data.roundReward1Amount),
                new(PropertyType.Get(data.roundReward2Type), data.roundReward2Amount),
                new(PropertyType.Get(data.roundReward3Type), data.roundReward3Amount),
            };
                
            return new RushEventMissionGroupData
            {
                totalRound = data.totalRound,
                missionDatas = targetEventMissionData,
                roundRewards = roundRewards,
                leaderboardName = data.leaderboardName,
            };
        }

        private RushEventPackageGroupData GetRushEventPackageGroupData(RushEventTableData data)
        {
            var packageTableDataList = TableManager.Get<RushEventPackageTableData>("bm_rush_packages");
            var packageDataList = packageTableDataList.FindAll(packageData => packageData.eventVar == data.eventVar);
            Assert.IsTrue(packageDataList.Count > 0);
            
            var targetEventPackageData = new List<RushEventPackageData>();
            foreach (var packageData in packageDataList)
            {
                bool isFree = packageData.isFree == 1;
                targetEventPackageData.Add(new RushEventPackageData
                {
                    id = packageData.productId,
                    packageId = packageData.packageId,
                    limit = packageData.limit,
                    isFree = isFree,
                });
            }

            return new RushEventPackageGroupData
            {
                packageDatas = targetEventPackageData,
            };
        }
        
        public string GetRedDotPath()
        {
            return _redDotPath;
        }
    }
}