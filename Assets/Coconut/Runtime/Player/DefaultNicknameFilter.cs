using Cysharp.Threading.Tasks;

namespace Aloha.Coconut.Player
{
    public class DefaultNicknameFilter : INicknameFilter
    {
        private readonly MyProfileConfig _myProfileConfig;

        public DefaultNicknameFilter(MyProfileConfig myProfileConfig)
        {
            _myProfileConfig = myProfileConfig;
        }
        
        public async UniTask<NicknameFilterResult> Check(string nickname)
        {
            NicknameFilterResult result = new NicknameFilterResult();

            (bool isSuccess, string message) = await TextFilteringManager.IsValid(nickname);
            if (isSuccess == false)
            {
                result.isValid = false;
                result.failureMessage = message;
                return result;
            }

            if (IsValidLength(nickname, out message) == false)
            {
                result.isValid = false;
                result.failureMessage = message;
                return result;
            }

            result.isValid = true;
            return result;
        }

        // 닉네임 텍스트에만 해당하는 길이 체크
        private bool IsValidLength(string text, out string message)
        {
            bool result = text.Length >= _myProfileConfig.NicknameLengthMin &&
                          text.Length <= _myProfileConfig.NicknameLengthMax;
            message = result == false ? TextTableV2.Get("TextFilter/InvalidLength") : "";

            return result;
        }
    }
}
