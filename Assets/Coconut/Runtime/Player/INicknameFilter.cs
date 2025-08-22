using Cysharp.Threading.Tasks;

namespace Aloha.Coconut.Player
{
    public struct NicknameFilterResult
    {
        public bool isValid;
        public string failureMessage;
    }
    
    public interface INicknameFilter
    {
        public UniTask<NicknameFilterResult> Check(string nickname);
    }
}
