using Cysharp.Threading.Tasks;

namespace Aloha.Durian
{
    public class SessionManager
    {
        private string _sessionId;

        public async UniTask InitSessionAsync()
        {
            var sessionApi = await DurianApis.SessionApi();
            var rootSessionDto = await RequestHandler.Request(sessionApi.InitSessionAsync(), resp => resp.Data);
            _sessionId = rootSessionDto.SessionId;
        }

        public async UniTask CheckSessionAsync()
        {
            var sessionApi = await DurianApis.SessionApi();
            await RequestHandler.Request(sessionApi.CheckSessionAsync(_sessionId), resp => resp.Data, false, 3);
        }
    }
}
