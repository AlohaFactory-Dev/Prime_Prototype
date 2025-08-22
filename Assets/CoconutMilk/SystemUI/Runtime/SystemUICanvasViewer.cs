using System;
using Aloha.Coconut;
using Aloha.Coconut.UI;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Aloha.CoconutMilk
{
    [RequireComponent(typeof(CoconutCanvas))]
    public class SystemUICanvasViewer : MonoBehaviour, ISystemDialogueViewer, IAwaitScreenViewer
    {
        public IObservable<Unit> OnSpecialGesture => _onSpecialGesture;
        private static Subject<Unit> _onSpecialGesture = new Subject<Unit>();
        
        [SerializeField] private UIViewConfig systemDialogueViewConfig;
        [SerializeField] private UIViewConfig awaitViewConfig;

        [SerializeField] private Button specialGestureButton;
        
        private CoconutCanvas _canvas;
        private UIView _awaitScreenView;

        void Awake()
        {
            _canvas = GetComponent<CoconutCanvas>();
            specialGestureButton.gameObject.SetActive(false);
        }

        void Start()
        {
            // 5초 안에 5회 이상 클릭시
            specialGestureButton.OnClickAsObservable()
                .Buffer(TimeSpan.FromSeconds(5), 5)
                .Where(clicks => clicks.Count >= 5)
                .Subscribe(_ =>
                {
                    _onSpecialGesture.OnNext(Unit.Default);
                    SystemUI.ShowToastMessage("Something Happened...");
                }).AddTo(this);
        }

        public async UniTask<bool> ShowDialogueYesNo(string title, string content, string yes, string no = null)
        {
            specialGestureButton.gameObject.SetActive(true);
            var result = await _canvas.OpenAsync(systemDialogueViewConfig, new SystemDialoguePopup.Args(title, content, yes, no));
            specialGestureButton.gameObject.SetActive(false);
            return ((SystemDialoguePopup.Result) result).isYes;
        }

        public void Show()
        {
            specialGestureButton.gameObject.SetActive(true);
            _awaitScreenView = _canvas.Open(awaitViewConfig);
        }

        public void Hide()
        {
            specialGestureButton.gameObject.SetActive(false);
            _awaitScreenView?.Close();
        }
    }
}