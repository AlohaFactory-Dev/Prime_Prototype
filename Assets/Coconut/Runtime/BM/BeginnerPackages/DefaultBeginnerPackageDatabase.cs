using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine.Assertions;

namespace Aloha.Coconut
{
    public class DefaultBeginnerPackageDatabase : IBeginnerPackageDatabase
    {
        private struct BeginnerPackageTableData
        {
            [CSVColumn] public int id;
            [CSVColumn] public string iapId;
            [CSVColumn] public int day;
            [CSVColumn] public PropertyTypeAlias reward1Alias;
            [CSVColumn] public BigInteger reward1Amount;
            [CSVColumn] public PropertyTypeAlias reward2Alias;
            [CSVColumn] public BigInteger reward2Amount;
            [CSVColumn] public PropertyTypeAlias reward3Alias;
            [CSVColumn] public BigInteger reward3Amount;
            [CSVColumn] public PropertyTypeAlias reward4Alias;
            [CSVColumn] public BigInteger reward4Amount;
            [CSVColumn] public PropertyTypeAlias reward5Alias;
            [CSVColumn] public BigInteger reward5Amount;
        }
        
        private readonly PropertyTypeGroup _beginnerPackageTypeGroup;
        private readonly string _redDotPath;

        public DefaultBeginnerPackageDatabase()
        {
            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/beginner_package_type_group"));
            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/beginner_package_reddot_path"));
            
            _beginnerPackageTypeGroup = GameConfig.GetPropertyTypeGroup("coconut_bm/beginner_package_type_group");
            _redDotPath = GameConfig.GetString("coconut_bm/beginner_package_reddot_path");
        }
        
        public List<BeginnerPackageData> GetBeginnerPackageDataList()
        {
            var tableData = TableManager.Get<BeginnerPackageTableData>("bm_beginner_packages");
            var packageDataList = new List<BeginnerPackageData>();
            
            var packageGroup = tableData.GroupBy(data => data.id);
            foreach (var group in packageGroup)
            {
                var componentDataList = new List<BeginnerPackageComponentData>();
                foreach (var data in group)
                {
                    var rewards = new List<Property>
                    {
                        new (data.reward1Alias, data.reward1Amount),
                        new (data.reward2Alias, data.reward2Amount),
                        new (data.reward3Alias, data.reward3Amount),
                        new (data.reward4Alias, data.reward4Amount),
                        new (data.reward5Alias, data.reward5Amount),
                    };
                    componentDataList.Add(new BeginnerPackageComponentData(data.day, rewards));
                }
                packageDataList.Add(new BeginnerPackageData(group.Key, group.First().iapId, componentDataList));
            }
            
            return packageDataList;
        }
        
        public PropertyTypeGroup GetBeginnerPackageTypeGroup()
        {
            return _beginnerPackageTypeGroup;
        }

        public string GetRedDotPath()
        {
            return _redDotPath;
        }
    }
}