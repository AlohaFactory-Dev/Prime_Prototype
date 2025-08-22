using System.Collections.Generic;

namespace Aloha.Coconut.Attendances
{
    public class AttendanceNode
    {
        public readonly int day;
        public readonly List<Property> rewards;
        public bool IsClaimed { get; internal set; }
        public bool IsClaimable { get; internal set; }

        public AttendanceNode(int day, List<Property> rewards)
        {
            this.day = day;
            this.rewards = rewards;
        }
    }
}
