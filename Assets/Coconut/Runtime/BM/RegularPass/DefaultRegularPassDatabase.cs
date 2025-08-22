using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine.Assertions;

namespace Aloha.Coconut
{
    public class DefaultRegularPassDatabase : IRegularPassDatabase
    {
        private class RegularPassTableData
        {
            [CSVColumn] public int passId;
            [CSVColumn] public string passType;
            [CSVColumn] public int from;
            [CSVColumn] public int to;
            [CSVColumn] public string iapId;
            [CSVColumn] public int passLevel;
            [CSVColumn] public string reward1Alias;
            [CSVColumn] public BigInteger reward1Amount;
            [CSVColumn] public string reward2Alias;
            [CSVColumn] public BigInteger reward2Amount;
        }

        private readonly PropertyTypeGroup _regularPassTypeGroup;
        private readonly string _redDotPath;

        public DefaultRegularPassDatabase()
        {
            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/regular_pass_type_group"));
            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/regular_pass_red_dot_path"));
            
            _regularPassTypeGroup = GameConfig.GetPropertyTypeGroup("coconut_bm/regular_pass_type_group");
            _redDotPath = GameConfig.GetString("coconut_bm/regular_pass_red_dot_path");
        }

        public List<RegularPassData> GetEveryRegularPassData()
        {
            var tableData = TableManager.Get<RegularPassTableData>("bm_reg_pass");
            var regularPassDatas = new Dictionary<int, RegularPassData>();
            var nodes = new Dictionary<int, List<PassNodeData>>();

            foreach (var data in tableData)
            {
                if (!nodes.ContainsKey(data.passId))
                {
                    nodes[data.passId] = new List<PassNodeData>();
                }

                nodes[data.passId].Add(new PassNodeData
                {
                    passLevel = data.passLevel,
                    reward1Alias = data.reward1Alias,
                    reward1Amount = data.reward1Amount,
                    reward2Alias = data.reward2Alias,
                    reward2Amount = data.reward2Amount
                });
            }

            foreach (var data in tableData)
            {
                if (regularPassDatas.ContainsKey(data.passId)) continue;

                var regularPassData = new RegularPassData
                {
                    id = data.passId,
                    type = data.passType,
                    from = data.from,
                    to = data.to,
                    iapId = data.iapId,
                    nodeDatas = nodes[data.passId]
                };
                regularPassDatas[data.passId] = regularPassData;
            }

            return regularPassDatas.Values.ToList();
        }

        public PropertyTypeGroup GetRegularPassTypeGroup()
        {
            return _regularPassTypeGroup;
        }
        
        public string GetRedDotPath()
        {
            return _redDotPath;
        }
    }
}