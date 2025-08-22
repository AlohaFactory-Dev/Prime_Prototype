using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Aloha.Sdk
{
    public class AlohaSimplePopup : MonoBehaviour
    {
        internal static async Task ShowFromResourceTask(string resourcePath)
        {
            var popUp = Instantiate(Resources.Load<AlohaSimplePopup>(resourcePath));
            var taskCompletionSource = new TaskCompletionSource<bool>();

            popUp.OnClickOK += () => { taskCompletionSource.SetResult(true); };
            popUp.Show();

            await taskCompletionSource.Task;
        }

        internal static IEnumerator ShowFromResourceCoroutine(string resourcePath)
        {
            var isDone = false;
            var popUp = Instantiate(Resources.Load<AlohaSimplePopup>(resourcePath));
            
            popUp.OnClickOK += () => { isDone = true; };
            popUp.Show();
            
            yield return new WaitUntil(() => isDone);
        }
        
        internal event Action OnClickOK;
    
        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private RectTransform popUpRectTransform;

        [SerializeField] private Button okButton;

        private void Show()
        {
            Debug.Assert(EventSystem.current != null, "EventSystem.current가 없습니다!");
        
            gameObject.SetActive(true);
            popUpRectTransform.localScale = Vector3.zero;

            okButton.onClick.AddListener(() =>
            {
                OnClickOK?.Invoke();
                Destroy(gameObject);
            });
            
            StartCoroutine(OpenPopup());
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
    }    
}

