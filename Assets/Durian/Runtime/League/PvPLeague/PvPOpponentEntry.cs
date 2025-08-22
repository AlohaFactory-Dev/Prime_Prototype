namespace Aloha.Durian
{
    public class PvPOpponentEntry<TPlayerData> where TPlayerData : PlayerPublicData
    {
        public TPlayerData PlayerData { get; }
        public string Nickname { get; }
        public string UID { get; }
        public int Power { get; }
        public int Rating { get; }
        public int WinScore { get; }
        public int LoseScore { get; }
        public bool IsBot { get; }

        public PvPOpponentEntry(LeaderboardEntry leaderboardEntry, TPlayerData playerData, int power, int winScore, int loseScore)
        {
            PlayerData = playerData;
            Nickname = leaderboardEntry.Nickname;
            UID = leaderboardEntry.UID;
            Rating = leaderboardEntry.Score;

            Power = power;
            WinScore = winScore;
            LoseScore = loseScore;
            IsBot = leaderboardEntry.IsBot;
        }
    }
}