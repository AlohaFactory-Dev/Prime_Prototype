namespace Aloha.Coconut.Missions
{
    public class MissionAttribute : System.Attribute
    {
        public MissionType MissionType { get; }

        public MissionAttribute(MissionType missionType)
        {
            MissionType = missionType;
        }
    }
}