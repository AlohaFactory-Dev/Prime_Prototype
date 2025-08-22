namespace Aloha.Durian
{
    public struct PlayerScoreDelta
    {
        public string uid;
        public int scoreDelta;

        public PlayerScoreDelta(string uid, int scoreDelta)
        {
            this.uid = uid;
            this.scoreDelta = scoreDelta;
        }
    }
}
