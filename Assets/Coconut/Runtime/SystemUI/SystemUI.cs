using System;
using Aloha.CoconutMilk;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;

namespace Aloha.Coconut
{
    public class SystemUI : MonoBehaviour
    {
        private static SystemUI _instance;
        
        // SystemDialogue에서 특정 동작을 수행하면 호출되도록 처리, 권한 오버라이드 등을 위한 용도로 사용
        public static IObservable<Unit> OnSpecialGesture => _onSpecialGesture;
        private static Subject<Unit> _onSpecialGesture = new Subject<Unit>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            _instance = null;
            _onSpecialGesture = new Subject<Unit>();
        }

        public static async UniTask ShowDialogue(string title, string content, string yes = null)
        {
            if(yes == null) yes = TextTableV2.Get("Common/Yes");
            await _instance._systemDialogueViewer.ShowDialogueYesNo(title, content, yes);
        }

        public static async UniTask<bool> ShowDialogueYesNo(string title, string content, string yes = null, string no =null)
        {
            if(yes == null) yes = TextTableV2.Get("Common/Yes");
            if(no == null) no = TextTableV2.Get("Common/No");
            return await _instance._systemDialogueViewer.ShowDialogueYesNo(title, content, yes, no);
        }

        public static void ShowToastMessage(string message, float duration = 1f)
        {
            _instance._toastMessageViewer.Show(message, duration);
        }

        public static async UniTask Await(UniTask task, bool showDialogueOnException = true)
        {
            _instance._awaitScreenViewer.Show();
            try
            {
                await task;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
                if (showDialogueOnException)
                {
                    await ShowDialogue(TextTableV2.Get("Common/Error"), e.Message);   
                }
            }
            finally
            {
                _instance._awaitScreenViewer.Hide();
            }
        }

        public static async UniTask<T> Await<T>(UniTask<T> task, bool showDialogueOnException = true)
        {
            _instance._awaitScreenViewer.Show();
            try
            {
                return await task;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
                if (showDialogueOnException)
                {
                    await ShowDialogue(TextTableV2.Get("Common/Error"), e.Message);
                }
                return default;
            }
            finally
            {
                _instance._awaitScreenViewer.Hide();
            }
        }
    
        private ISystemDialogueViewer _systemDialogueViewer;
        private IToastMessageViewer _toastMessageViewer;
        private IAwaitScreenViewer _awaitScreenViewer;
    
        void Awake()
        {
            Assert.IsNull(_instance, "SystemUI is singleton");
            
            _systemDialogueViewer = GetComponentInChildren<ISystemDialogueViewer>();
            _toastMessageViewer = GetComponentInChildren<IToastMessageViewer>();
            _awaitScreenViewer = GetComponentInChildren<IAwaitScreenViewer>();
            
            Assert.IsNotNull(_systemDialogueViewer, "SystemUI must have a component that implement ISystemDialogueViewer");
            Assert.IsNotNull(_toastMessageViewer, "SystemUI must have a component that implement IToastMessageViewer");
            Assert.IsNotNull(_awaitScreenViewer, "SystemUI must have a component that implement IAwaitScreenViewer");
            
            _instance = this;
            _systemDialogueViewer.OnSpecialGesture.Subscribe(_ => _onSpecialGesture.OnNext(Unit.Default));
        }
    }
}