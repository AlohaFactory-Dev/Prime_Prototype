#if COCONUT_DURIAN
using Aloha.Coconut;
using Aloha.Coconut.UI;
using Aloha.Durian;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Aloha.CoconutMilk
{
    public class AccountPopup : UISlice
    {
        [Inject] private AuthManager _authManager;
        [Inject] private SaveDataManager _saveDataManager;

        [SerializeField] private GameObject googleGroup;
        [SerializeField] private GameObject googleLinked;
        [SerializeField] private Button signInWithGoogleButton;
    
        [SerializeField] private GameObject appleGroup;
        [SerializeField] private GameObject appleLinked;
        [SerializeField] private Button signInWithAppleButton;
    
        [SerializeField] private Button signOutButton;
        [SerializeField] private Button deleteAccountButton;

        void Start()
        {
            signInWithGoogleButton.onClick.AddListener(() => SignIn(AuthProvider.Google));
            signInWithAppleButton.onClick.AddListener(() => SignIn(AuthProvider.Apple));
            signOutButton.onClick.AddListener(() => SignOut());
            deleteAccountButton.onClick.AddListener(() => DeleteAccount());
        }

        protected override void Open(UIOpenArgs openArgs)
        {
            base.Open(openArgs);
            RefreshButtons();
        }

        private void RefreshButtons()
        {
            bool isSignedWithGoogle = _authManager.IsSignedInWithCredential(AuthProvider.Google);
            bool isSignedWithApple = _authManager.IsSignedInWithCredential(AuthProvider.Apple);
            
            googleGroup.SetActive(_authManager.CredentialManagerExists(AuthProvider.Google) && !isSignedWithApple);
            googleLinked.SetActive(isSignedWithGoogle);
            signInWithGoogleButton.gameObject.SetActive(!isSignedWithGoogle);
        
            appleGroup.SetActive(_authManager.CredentialManagerExists(AuthProvider.Apple) && !isSignedWithGoogle);
            appleLinked.SetActive(isSignedWithApple);
            signInWithAppleButton.gameObject.SetActive(!isSignedWithApple);
        }

        private async UniTaskVoid SignIn(AuthProvider provider)
        {
            AuthManager.LinkCredentialResult result = await SystemUI.Await(_authManager.LinkCredential(provider));
            if (result.isSuccess)
            {
                RefreshButtons();
            }
            else
            {
                if (result.IsAlreadyLinkedException)
                {
                    // cancel이 빨간색이므로 ok와 cancel을 반대로 취급
                    bool accountChangeConfirmed = !await SystemUI.ShowDialogueYesNo(TextTableV2.Get("Common/Warning"),
                        TextTableV2.Get("Account/AccountChangeWarning"),
                        TextTableV2.Get("Common/No"), TextTableV2.Get("Common/Yes"));

                    if (accountChangeConfirmed)
                    {
                        _saveDataManager.Lock(true);
                        await SystemUI.Await(_authManager.SignInWithCredential(provider));
                        await SystemUI.ShowDialogue(TextTableV2.Get("Common/Notice"), TextTableV2.Get("Account/RestartGame"));
                        Application.Quit();
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#endif
                    }
                    else
                    {
                        _authManager.ForceSignOutCredential(provider);
                    }
                }
                else
                {
                    Debug.LogError(result.exception.Message);
                    Debug.LogError(result.exception.StackTrace);
                    SystemUI.ShowToastMessage(TextTableV2.Get("Account/SignInFailed"));
                    SystemUI.ShowDialogue(TextTableV2.Get("Common/Error"), result.exception.Message);   
                }
            }
        }

        private async UniTaskVoid SignOut()
        {
            bool confirmed = false;
        
            if (_authManager.IsAnonymous)
            {
                // cancel이 빨간색이므로 ok와 cancel을 반대로 취급
                confirmed = !await SystemUI.ShowDialogueYesNo(TextTableV2.Get("Common/Warning"),
                    TextTableV2.Get("Account/AnonymousSignOutWarning"),
                    TextTableV2.Get("Common/No"), TextTableV2.Get("Common/Yes"));
            }
            else
            {
                // cancel이 빨간색이므로 ok와 cancel을 반대로 취급
                confirmed = !await SystemUI.ShowDialogueYesNo(TextTableV2.Get("Common/Warning"),
                    TextTableV2.Get("Account/SignOutWarning"),
                    TextTableV2.Get("Common/No"), TextTableV2.Get("Common/Yes"));
            }

            if (confirmed)
            {
                // Dialogue팝업이 닫히고 다시 열도록 1프레임 대기
                var eventSystem = EventSystem.current;
                eventSystem.enabled = false;
                await UniTask.DelayFrame(1);
                eventSystem.enabled = true;
            
                _saveDataManager.Lock(true);
                _authManager.SignOut();
                await SystemUI.ShowDialogue(TextTableV2.Get("Common/Notice"), TextTableV2.Get("Account/RestartGame"));
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }

        private async UniTaskVoid DeleteAccount()
        {
            // cancel이 빨간색이므로 ok와 cancel을 반대로 취급
            var confirmed = !await SystemUI.ShowDialogueYesNo(TextTableV2.Get("Common/Warning"),
                TextTableV2.Get("Account/WithdrawWarning"),
                TextTableV2.Get("Common/No"), TextTableV2.Get("Common/Yes"));

            if (confirmed)
            {
                // Dialogue팝업이 닫히고 다시 열도록 1프레임 대기
                var eventSystem = EventSystem.current;
                eventSystem.enabled = false;
                await UniTask.DelayFrame(1);
                eventSystem.enabled = true;
                
                _saveDataManager.Lock(true);
                await SystemUI.Await(_authManager.DeleteAccount());
                await SystemUI.ShowDialogue(TextTableV2.Get("Common/Notice"), TextTableV2.Get("Account/RestartGame"));
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }
    }
}

#endif