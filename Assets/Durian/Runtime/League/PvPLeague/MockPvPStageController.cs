using Cysharp.Threading.Tasks;

namespace Aloha.Durian
{
    public class MockPvPStageController<TPlayerData> : IPvPStageController<TPlayerData> where TPlayerData : PlayerPublicData
    {
        public PvPOpponentEntry<TPlayerData> OpponentEntry { get; private set; }

        public async UniTask<bool> PlayPvP(PvPOpponentEntry<TPlayerData> opponentEntry)
        {
            OpponentEntry = opponentEntry;
            return true;
        }
    }
}