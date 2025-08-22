namespace Aloha.Coconut
{
    public interface IRushEventDatabase
    {
        public RushEventData GetRushEventData(int rushEventId);
        public string GetRedDotPath();
    }
}
