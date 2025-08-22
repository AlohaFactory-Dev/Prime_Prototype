using Aloha.Coconut;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Google;
using UnityEngine;

namespace Aloha.Durian
{
    public class GoogleCredentialManager : ICredentialManager
    {
        public AuthProvider Provider => AuthProvider.Google;
        private string _idToken;

        public async UniTask<Credential> GetCredential()
        {
            string idToken = "";
            try
            {
                idToken = await GetIDToken();   
            }
            catch (System.Exception e)
            {
                Debug.LogError("GoogleSignInHandler.GetIDToken encountered an error: " + e);
                return null;
            }

            return GoogleAuthProvider.GetCredential(idToken, null);
        }

        private async UniTask<string> GetIDToken()
        {
            if (_idToken != null) return _idToken;

            if (GoogleSignIn.Configuration == null)
            {
                GoogleSignIn.Configuration = new GoogleSignInConfiguration()
                {
                    RequestEmail = true,
                    RequestProfile = true,
                    RequestIdToken = true,
                    RequestAuthCode = true,
                    // must be web client ID, not android client ID
                    WebClientId = CoconutConfig.Get<DurianConfig>().gcpWebClientId,
#if UNITY_EDITOR || UNITY_STANDALONE
                    // optional for windows/macos and test in editor
                    ClientSecret = CoconutConfig.Get<DurianConfig>().gcpWebClientSecret
#endif
                };
            }
            else
            {
                GoogleSignIn.Configuration.RequestEmail = true;
                GoogleSignIn.Configuration.RequestProfile = true;
                GoogleSignIn.Configuration.RequestIdToken = true;
                GoogleSignIn.Configuration.RequestAuthCode = true;
                GoogleSignIn.Configuration.WebClientId = CoconutConfig.Get<DurianConfig>().gcpWebClientId;
#if UNITY_EDITOR || UNITY_STANDALONE
                GoogleSignIn.Configuration.ClientSecret = CoconutConfig.Get<DurianConfig>().gcpWebClientSecret;
#endif
            }

            try
            {
                Debug.Log("Trying to sign in silently...");
                var signInResult = await GoogleSignIn.DefaultInstance.SignInSilentlyAsync();
                _idToken = signInResult.IdToken;
                return _idToken;
            }
            catch (GoogleSignIn.SignInException e)
            {
                Debug.LogWarning(e);
                Debug.Log("Trying to sign in again...");

                var signInResult = await GoogleSignIn.DefaultInstance.SignIn();
                _idToken = signInResult.IdToken;
                return _idToken;
            }
        }

        public void SignOut()
        {
            _idToken = null;
            GoogleSignIn.DefaultInstance.SignOut();
        }
    }
}