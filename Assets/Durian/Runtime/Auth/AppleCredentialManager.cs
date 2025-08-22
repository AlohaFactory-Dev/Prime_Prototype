using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
#if UNITY_IOS
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
#endif
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;
using Zenject;

namespace Aloha.Durian
{
    public class AppleCredentialManager : ICredentialManager, ITickable
    {
        public AuthProvider Provider => AuthProvider.Apple;
        
#if UNITY_IOS
        private IAppleAuthManager _appleAuthManager;
#endif

        public AppleCredentialManager()
        {
#if UNITY_IOS
            var deserializer = new PayloadDeserializer();
            _appleAuthManager = new AppleAuthManager(deserializer);
#endif
        }
        
        public async UniTask<Credential> GetCredential()
        {
            try
            {
                var (idToken, rawNonce) = await GetIDTokenAndNonce();
                Debug.Log("Apple ID Token: " + idToken);
                return OAuthProvider.GetCredential("apple.com", idToken, rawNonce, null);
            }
            catch (System.Exception e)
            {
                Debug.LogError("AppleCredentialManager.GetCredential encountered an error: " + e);
                return null;
            }
        }

        private async UniTask<(string, string)> GetIDTokenAndNonce()
        {
#if UNITY_IOS
            var rawNonce = GenerateRandomString(32);
            var nonce = GenerateSHA256NonceFromRawNonce(rawNonce);
        
            var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName, nonce);
            var tcs = new UniTaskCompletionSource<string>();
        
            _appleAuthManager.LoginWithAppleId(
                loginArgs,
                credential =>
                {
                    // Obtained credential, cast it to IAppleIDCredential
                    if (credential is IAppleIDCredential appleIdCredential)
                    {
                        // Apple User ID
                        // You should save the user ID somewhere in the device
                        var userId = appleIdCredential.User;
                        PlayerPrefs.SetString("apple_user_id", userId);

                        // Email (Received ONLY in the first login)
                        var email = appleIdCredential.Email;

                        // Full name (Received ONLY in the first login)
                        var fullName = appleIdCredential.FullName;

                        // Identity token
                        var identityToken = Encoding.UTF8.GetString(
                            appleIdCredential.IdentityToken,
                            0,
                            appleIdCredential.IdentityToken.Length);

                        // Authorization code
                        var authorizationCode = Encoding.UTF8.GetString(
                            appleIdCredential.AuthorizationCode,
                            0,
                            appleIdCredential.AuthorizationCode.Length);
                    
                        // And now you have all the information to create/login a user in your system
                        tcs.TrySetResult(identityToken);
                    }
                },
                error =>
                {
                    // Something went wrong
                    var authorizationErrorCode = error.GetAuthorizationErrorCode();
                    tcs.TrySetException(new System.Exception(authorizationErrorCode.ToString()));
                });

            return (await tcs.Task, rawNonce);
#else
            throw new System.NotImplementedException();
#endif
        }

        public void SignOut()
        {
            // Apple은 SignOut이 없음
        }

        public void Tick()
        {
#if UNITY_IOS
            _appleAuthManager.Update();
#endif
        }

        #region NONCE
        // https://github.com/lupidan/apple-signin-unity/blob/master/docs/Firebase_NOTES.md
        private static string GenerateRandomString(int length)
        {
            if (length <= 0)
            {
                throw new Exception("Expected nonce to have positive length");
            }

            const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._";
            var cryptographicallySecureRandomNumberGenerator = new RNGCryptoServiceProvider();
            var result = string.Empty;
            var remainingLength = length;

            var randomNumberHolder = new byte[1];
            while (remainingLength > 0)
            {
                var randomNumbers = new List<int>(16);
                for (var randomNumberCount = 0; randomNumberCount < 16; randomNumberCount++)
                {
                    cryptographicallySecureRandomNumberGenerator.GetBytes(randomNumberHolder);
                    randomNumbers.Add(randomNumberHolder[0]);
                }

                for (var randomNumberIndex = 0; randomNumberIndex < randomNumbers.Count; randomNumberIndex++)
                {
                    if (remainingLength == 0)
                    {
                        break;
                    }

                    var randomNumber = randomNumbers[randomNumberIndex];
                    if (randomNumber < charset.Length)
                    {
                        result += charset[randomNumber];
                        remainingLength--;
                    }
                }
            }

            return result;
        }

        private static string GenerateSHA256NonceFromRawNonce(string rawNonce)
        {
            var sha = new SHA256Managed();
            var utf8RawNonce = Encoding.UTF8.GetBytes(rawNonce);
            var hash = sha.ComputeHash(utf8RawNonce);

            var result = string.Empty;
            for (var i = 0; i < hash.Length; i++)
            {
                result += hash[i].ToString("x2");
            }

            return result;
        }

        #endregion
    }
}