using System.Collections.Generic;

namespace Aloha.Coconut
{
    internal class DefaultEventScheduleDatabase : IEventScheduleDatabase
    {
        public List<EventScheduleData> GetEventScheduleDatas()
        {
            return TableManager.Get<EventScheduleData>("bm_event_schedules");
        }
    }
}
