using Cysharp.Threading.Tasks;
using Firebase.Auth;

namespace Aloha.Durian
{
    public interface ICredentialManager
    {
        AuthProvider Provider { get; }
        UniTask<Credential> GetCredential();
        void SignOut();
    }
}
