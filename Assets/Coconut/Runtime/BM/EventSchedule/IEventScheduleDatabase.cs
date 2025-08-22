using System.Collections.Generic;

namespace Aloha.Coconut
{
    public interface IEventScheduleDatabase
    {
        public List<EventScheduleData> GetEventScheduleDatas();
    }
    
    public struct EventScheduleData
    {
        [CSVColumn] public int id;
        [CSVColumn] public string type;
        [CSVColumn] public int var;
        [CSVColumn] public int isCustom;
        [CSVColumn] public int from;
        [CSVColumn] public int to;
        
        public EventScheduleData(int id, string type, int var, bool isCustom, int from, int to)
        {
            this.id = id;
            this.type = type;
            this.var = var;
            this.isCustom = isCustom ? 1 : 0;
            this.from = from;
            this.to = to;
        }
    }
}
