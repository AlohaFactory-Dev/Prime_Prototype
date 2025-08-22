using Cysharp.Threading.Tasks;

namespace Aloha.Coconut
{
    public interface IRVAdapter
    {
        UniTask<bool> ShowRewardedAdAsync(int placementId, string placementName);
    }
}
