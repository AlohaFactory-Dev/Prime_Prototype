#if COCONUT_ALOHA_SDK

using Aloha.Coconut;
using Aloha.Sdk;
using Cysharp.Threading.Tasks;

namespace Aloha.CoconutMilk
{
    public class AlohaSdkRVAdapter : IRVAdapter
    {
        public async UniTask<bool> ShowRewardedAdAsync(int placementId, string placementName)
        {
            return await AlohaSdk.Ads.ShowRewardedAdAsync(placementId, placementName);
        }
    }
}

#endif