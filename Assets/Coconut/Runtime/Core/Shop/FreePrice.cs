using Cysharp.Threading.Tasks;

namespace Aloha.Coconut
{
    public class FreePrice : IPrice
    {
        public async UniTask<bool> Pay(PlayerAction playerAction)
        {
            return true;
        }

        public bool IsPayable()
        {
            return true;
        }

        public string GetPriceString()
        {
            return "Free";
        }
    }
}