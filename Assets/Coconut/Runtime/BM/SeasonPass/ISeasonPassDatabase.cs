using System.Collections.Generic;

namespace Aloha.Coconut
{
    public class SeasonPassData
    {
        [CSVColumn] public int id;
        [CSVColumn] public int expPerLevel;
        [CSVColumn] public int endDate;
        
        public List<PassNodeData> nodeDatas = new List<PassNodeData>();
    }
    
    public interface ISeasonPassDatabase
    {
        SeasonPassData GetCurrentSeasonPassData(GameDate now);
        PropertyTypeGroup GetSeasonPassTypeGroup();
        string GetRedDotPath();
    }
}