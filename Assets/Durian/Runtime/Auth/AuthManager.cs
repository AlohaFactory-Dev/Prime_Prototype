using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using Aloha.Sdk;
using Alohacorp.Durian.Api;
using Alohacorp.Durian.Client;
using Alohacorp.Durian.Model;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;

namespace Aloha.Durian
{
    public class AuthManager
    {
        [DataContract(Name = "BlockedPlayerInfo")]
        public class BlockedPlayerInfo
        {
            public BlockedPlayerInfo(
                string playerUid = default,
                string deviceId = default,
                string reason = default,
                long? createdAt = default,
                long? expiresAt = default
            )
            {
                PlayerUid = playerUid;
                DeviceId = deviceId;
                Reason = reason;
                CreatedAt = createdAt;
                ExpiresAt = expiresAt;
            }

            [DataMember(Name = "playerUid")] public string PlayerUid { get; set; }
            [DataMember(Name = "deviceId")] public string DeviceId { get; set; }
            [DataMember(Name = "reason")] public string Reason { get; set; }
            [DataMember(Name = "createdAt")] public long? CreatedAt { get; set; }
            [DataMember(Name = "expiresAt")] public long? ExpiresAt { get; set; }
        }

        public class BlockedPlayerException : Exception
        {
            public BlockedPlayerException(string message) : base(message) { }
        }

        public IReadOnlyReactiveProperty<bool> IsSignedIn => _isSignedIn;
        private ReactiveProperty<bool> _isSignedIn = new(false);

        private readonly DurianConfig _durianConfig;
        private readonly List<ICredentialManager> _credentialManagers;

        public string UID { get; private set; }
        public bool IsAnonymous => _auth.CurrentUser.IsAnonymous;
        public bool IsInitialized { get; private set; }
        public ICredentialManager CurrentCredentialManager { get; private set; }

        private FirebaseAuth _auth;
        private FirebaseUser _user;

        public AuthManager(DurianConfig durianConfig, List<ICredentialManager> credentialManagers)
        {
            _durianConfig = durianConfig;
            _credentialManagers = credentialManagers;
        }

        public async UniTask Initialize()
        {
            // Firebase dependency check이 이뤄진 상태여야 함. 일반적으로 Durian을 도입하는 건 AlohaSdk보다 나중이기 때문에,
            // AlohaSdk에서 Firebase dependency check을 완료했다고 가정하고 별도 체크 없이 진행
            // AlohaSdk 없이 사용할 경우 별도의 Firebase dependency check이 필요함 
            _auth = FirebaseAuth.DefaultInstance;
            _auth.StateChanged += OnFirebaseAuthStateChanged;
            OnFirebaseAuthStateChanged(this, null);

            // Firebase 로그인 정보는 세션 종료 후에도 유지됨
            // Initialize 즉시 _user가 있다는 건 이미 Firebase 로그인을 한 적이 있다는 뜻이므로, 바로 API 로그인 진행
            if (_user != null) await SignInDurian();

            IsInitialized = true;
        }

        private void OnFirebaseAuthStateChanged(object sender, System.EventArgs eventArgs)
        {
            if (_auth.CurrentUser != _user)
            {
                bool signedIn = _user != _auth.CurrentUser && _auth.CurrentUser != null;
                if (!signedIn && _user != null)
                {
                    Debug.Log("Signed out " + _user.UserId);
                }

                _user = _auth.CurrentUser;
                if (signedIn)
                {
                    Debug.Log("Signed in " + _user.UserId);
                }
            }
        }

        private async UniTask SignInDurian()
        {
            string providerId = _user.ProviderData.Any() ? _user.ProviderData.First().ProviderId : "";
            CurrentCredentialManager = _credentialManagers.FirstOrDefault(c => c.Provider.GetId() == providerId);
            DurianApis.SetTokenGetter(() => _user.TokenAsync(false));

            PrivatePlayerDto playerData = null;
            try
            {
                PlayerApi playerApi = await DurianApis.PlayerApi();
                playerData = await RequestHandler.Request(playerApi.GetPlayersAsync(), p => p.Data);
            }
            catch (ApiException e)
            {
                if (e.ErrorCode == (int)HttpStatusCode.Forbidden)
                {
                    var blockedPlayerInfo = JsonConvert.DeserializeObject<BlockedPlayerInfo>((string)e.ErrorContent);
                    // TODO: Handle blocked player info
                    throw new BlockedPlayerException($"Player is blocked\nUID: {blockedPlayerInfo.PlayerUid}\nReason: {blockedPlayerInfo.Reason}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }

            if (playerData != null)
            {
                Debug.LogFormat("PlayerData signed in successfully: {0} ({1})", playerData.Name, playerData.Uid);
                UID = playerData.Uid;
                AlohaSdk.SetCUID(UID);
                _isSignedIn.Value = true;
            }
            else
            {
                Debug.LogError("Failed to get player data");
                _user = null;
                throw new System.InvalidOperationException("Failed to get player data");
            }
        }

        public async UniTask SignInAsGuest()
        {
            if (_auth.CurrentUser == null)
            {
                AuthResult result = await _auth.SignInAnonymouslyAsync();
                Debug.LogFormat("User SignInAnonymouslyAsync in successfully: {0} ({1})", result.User.DisplayName,
                    result.User.UserId);
            }

            await UniTask.WaitUntil(() => _auth.CurrentUser != null);

            try
            {
                await SignInDurian();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                _user = null;
                throw;
            }
        }

        public async UniTask SignInWithCredential(AuthProvider provider)
        {
            if (_auth.CurrentUser != null)
            {
                _auth.SignOut();
            }

            var credential = await GetCredentialManager(provider).GetCredential();
            if (credential == null) return;

            Debug.Log("Get credential successfully. Signing in...");
            AuthResult result = await _auth.SignInAndRetrieveDataWithCredentialAsync(credential);
            Debug.LogFormat("User SignInWithCredential in successfully: {0} ({1})", result.User.DisplayName,
                result.User.UserId);
            await UniTask.WaitUntil(() => _auth.CurrentUser != null);

            await SignInDurian();
        }

        private ICredentialManager GetCredentialManager(AuthProvider provider)
        {
            return _credentialManagers.Find(c => c.Provider == provider);
        }

        public async UniTask<LinkCredentialResult> LinkCredential(AuthProvider provider)
        {
            ICredentialManager credentialManager = GetCredentialManager(provider);
            var credential = await credentialManager.GetCredential();
            if (credential == null) return new LinkCredentialResult();

            var task = _auth.CurrentUser.LinkWithCredentialAsync(credential);

            try
            {
                await task;
                CurrentCredentialManager = credentialManager;
                return new LinkCredentialResult { isSuccess = true };
            }
            catch (Exception e)
            {
                return new LinkCredentialResult { isSuccess = false, exception = e };
            }
        }

        public void SignOut()
        {
            _auth.SignOut();
            _user = null;
            UID = "";
            CurrentCredentialManager?.SignOut();
            CurrentCredentialManager = null;
            _isSignedIn.Value = false;
        }

        public bool IsSignedInWithCredential(AuthProvider provider)
        {
            return CurrentCredentialManager != null && CurrentCredentialManager.Provider == provider;
        }

        public bool CredentialManagerExists(AuthProvider provider)
        {
            return _credentialManagers.Exists(c => c.Provider == provider);
        }

        public void ForceSignOutCredential(AuthProvider provider)
        {
            var credentialManager = GetCredentialManager(provider);
            if (credentialManager != null)
            {
                credentialManager.SignOut();
            }
        }

        public async UniTask BanMySelf(string reason)
        {
            PlayerApi playerApi = await DurianApis.PlayerApi();
            BlockCurrentPlayerReqDto blockCurrentPlayerReqDto = new BlockCurrentPlayerReqDto(reason);
            await playerApi.BlockAccessAsync(blockCurrentPlayerReqDto);
        }

        public async UniTask DeleteAccount()
        {
            if (_auth.CurrentUser == null) return;

            await _auth.CurrentUser.DeleteAsync();
            SignOut();
        }

        public struct LinkCredentialResult
        {
            public bool isSuccess;
            public Exception exception;
            public bool IsAlreadyLinkedException => exception is FirebaseAccountLinkException ex && ex.ErrorCode == 10;
        }
    }
}