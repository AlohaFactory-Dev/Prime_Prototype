namespace Aloha.Durian
{
    public interface ILeagueBotDataGenerator<TPlayerData> where TPlayerData : PlayerPublicData
    {
        LeagueBot<TPlayerData> GetBot(string seed);
    }

    public abstract class LeagueBot<TPlayerData> where TPlayerData : PlayerPublicData
    {
        public abstract TPlayerData GetPlayerData();
    }
}