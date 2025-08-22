using System.Collections.Generic;

namespace Aloha.Coconut
{
    public class RegularPassData
    {
        public int id;
        public string type;
        public int from;
        public int to;
        public string iapId;
        
        public List<PassNodeData> nodeDatas = new List<PassNodeData>();
    }
    
    public interface IRegularPassDatabase
    {
        List<RegularPassData> GetEveryRegularPassData();
        PropertyTypeGroup GetRegularPassTypeGroup();
        string GetRedDotPath();
    }
}
