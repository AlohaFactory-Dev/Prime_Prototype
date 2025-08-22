using Cysharp.Threading.Tasks;

namespace Aloha.Coconut
{
    public interface IPrice
    {
        UniTask<bool> Pay(PlayerAction playerAction);
        public bool IsPayable();
        string GetPriceString();
    }
}