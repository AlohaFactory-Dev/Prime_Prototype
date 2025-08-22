using System.Collections.Generic;
using System.Numerics;
using UnityEngine.Assertions;

namespace Aloha.Coconut
{
    public class SeasonPassDatabase : ISeasonPassDatabase
    {
        private struct SeasonPassNodeData
        {
            [CSVColumn] public int passId;
            [CSVColumn] public int passLevel;
            [CSVColumn] public string reward1Alias;
            [CSVColumn] public BigInteger reward1Amount;
            [CSVColumn] public string reward2Alias;
            [CSVColumn] public BigInteger reward2Amount;
            [CSVColumn] public string reward3Alias;
            [CSVColumn] public BigInteger reward3Amount;
        }

        private readonly PropertyTypeGroup _seasonPassTypeGroup;
        private readonly string _redDotPath;

        public SeasonPassDatabase()
        {
            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/season_pass_type_group"));
            Assert.IsTrue(GameConfig.HaveKey("coconut_bm/season_pass_red_dot_path"));
            
            _seasonPassTypeGroup = GameConfig.GetPropertyTypeGroup("coconut_bm/season_pass_type_group");
            _redDotPath = GameConfig.GetString("coconut_bm/season_pass_red_dot_path");
        }
        
        public SeasonPassData GetCurrentSeasonPassData(GameDate now)
        {
            SeasonPassData result = null;
            
            var passData = TableManager.Get<SeasonPassData>("bm_season_pass");
            passData.Sort((a, b) => a.endDate.CompareTo(b.endDate));
            foreach (var data in passData)
            {
                if (now <= new GameDate(data.endDate))
                {
                     result = data; 
                     result.nodeDatas = GetSeasonPassNodeDatas(data.id);
                     break;
                }
            }

            return result;
        }

        private List<PassNodeData> GetSeasonPassNodeDatas(int passId)
        {
            var passNodes = TableManager.Get<SeasonPassNodeData>("bm_season_pass_nodes");
            var result = new List<PassNodeData>();
            foreach (var node in passNodes)
            {
                if (node.passId == passId)
                {
                    result.Add(new PassNodeData
                    {
                        passLevel = node.passLevel,
                        reward1Alias = node.reward1Alias,
                        reward1Amount = node.reward1Amount,
                        reward2Alias = node.reward2Alias,
                        reward2Amount = node.reward2Amount,
                        reward3Alias = node.reward3Alias,
                        reward3Amount = node.reward3Amount  
                    });
                }
            }

            if (result.Count == 0 && passId > 0)
            {
                return GetSeasonPassNodeDatas(passId - 1);
            }
        
            result.Sort((a, b) => a.passLevel.CompareTo(b.passLevel));
            return result;
        }
        
        public PropertyTypeGroup GetSeasonPassTypeGroup()
        {
            return _seasonPassTypeGroup;
        }
        
        public string GetRedDotPath()
        {
            return _redDotPath;
        }
    }
}