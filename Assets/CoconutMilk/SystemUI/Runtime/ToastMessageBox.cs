using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;

namespace Aloha.CoconutMilk
{
    public class ToastMessageBox : MonoBehaviour
    {
        public Subject<Unit> OnComplete = new Subject<Unit>();

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI textMesh;
        [SerializeField] private float offset = 80;

        private float _offset;
        private Tween _offsetTween;
    
        public void Show(string message, float duration)
        {
            _offset = 0;
            canvasGroup.alpha = 1;
            textMesh.text = message;
            transform.localScale = Vector3.zero;
        
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(1, .2f))
                .AppendInterval(duration)
                .Append(canvasGroup.DOFade(0, .2f))
                .AppendCallback(() =>
                {
                    _offsetTween?.Kill();
                    OnComplete.OnNext(Unit.Default);
                });
        }

        public void IncreaseOffset()
        {
            _offset += offset;
            _offsetTween?.Kill();
            _offsetTween = transform.DOLocalMoveY(_offset, .2f);
        }
    }
}