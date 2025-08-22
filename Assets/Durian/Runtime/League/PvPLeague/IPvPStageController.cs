using Cysharp.Threading.Tasks;

namespace Aloha.Durian
{
    public interface IPvPStageController<TPlayerData> where TPlayerData : PlayerPublicData
    {
        UniTask<bool> PlayPvP(PvPOpponentEntry<TPlayerData> opponentEntry); // 플레이어 승리시 true, 패배시 false 반환
    }
}