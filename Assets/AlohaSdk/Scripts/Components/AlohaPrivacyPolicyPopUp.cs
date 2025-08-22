using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Aloha.Sdk
{
    public class AlohaPrivacyPolicyPopUp : MonoBehaviour
    {
        [Serializable]
        private class CheckBox
        {
            [SerializeField] private Button button;
            [SerializeField] private GameObject checkMark;

            public bool IsOn { get; private set; }

            public void Initialize()
            {
                button.onClick.AddListener(() =>
                {
                    SetOn(!IsOn);
                });
                
                SetOn(false);
            }
            
            public void SetOn(bool isOn)
            {
                IsOn = isOn;
                checkMark.SetActive(isOn);
            }
        }
        
        internal event Action OnStartGame;
    
        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private RectTransform popUpRectTransform;

        [SerializeField] private CheckBox tosCheckBox;
        [SerializeField] private CheckBox ppCheckBox;
        [SerializeField] private CheckBox notificationCheckBox;

        [SerializeField] private Button startButton;
        [SerializeField] private Button agreeAllAndStartButton;
        [SerializeField] private Button tosLinkButton;
        [SerializeField] private Button ppLinkButton;

        internal void Show()
        {
            Debug.Assert(EventSystem.current != null, "EventSystem.current가 없습니다!");
        
            gameObject.SetActive(true);
            popUpRectTransform.localScale = Vector3.zero;
            
            tosCheckBox.Initialize();
            ppCheckBox.Initialize();
            notificationCheckBox.Initialize();
            
            startButton.onClick.AddListener(StartGame);
            agreeAllAndStartButton.onClick.AddListener(AgreeAllAndStartGame);
            
            tosLinkButton.onClick.AddListener(() => Application.OpenURL(AlohaSdk.PrivacyPolicy.GetTermsLink(Application.systemLanguage)));
            ppLinkButton.onClick.AddListener(() => Application.OpenURL(AlohaSdk.PrivacyPolicy.GetTermsLink(Application.systemLanguage)));
            
            StartCoroutine(OpenPopup());
        }

        void Update()
        {
            startButton.interactable = tosCheckBox.IsOn && ppCheckBox.IsOn;
        }

        private IEnumerator OpenPopup()
        {
            float elapsed = 0f;
            
            while (elapsed <= 0.35f)
            {
                elapsed += Time.deltaTime;
                if (elapsed > 0.35f)
                    elapsed = 0.35f;
                
                popUpRectTransform.localScale = Vector3.one * animationCurve.Evaluate(elapsed / 0.35f);
                yield return null;
            }
        }

        private void StartGame()
        {
            AlohaSdk.Context.NotificationAgreed = notificationCheckBox.IsOn;
            OnStartGame?.Invoke();
            Destroy(gameObject);
        }

        private void AgreeAllAndStartGame()
        {
            tosCheckBox.SetOn(true);
            ppCheckBox.SetOn(true);
            notificationCheckBox.SetOn(true);
            StartGame();
        }
    }    
}

