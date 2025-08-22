using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Aloha.CoconutMilk
{
    public class TutorialFinger : MonoBehaviour
    {
        private enum Mode
        {
            Pointing,
            Moving
        }

        private TutorialFocusPoint _targetPoint;

        [SerializeField] private Image fingerImage;
        [SerializeField] private RectTransform fingerTransform;

        private Tween _fingerSequence;
        private Mode _mode;

        public void StartPointing(TutorialFocusPoint point, bool xFlipped = false, bool yFlipped = false)
        {
            _mode = Mode.Pointing;

            fingerImage.color = new Color(1, 1, 1, 1);
            if (xFlipped)
            {
                fingerTransform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                fingerTransform.localScale = Vector3.one;
            }

            if (yFlipped)
            {
                fingerTransform.localScale =
                    new Vector3(fingerTransform.localScale.x, -1, fingerTransform.localScale.z);
            }

            _targetPoint = point;
            TweenPointing(xFlipped, yFlipped);
        }

        private void TweenPointing(bool fingerXFlipped, bool fingerYFlipped)
        {
            fingerTransform.anchoredPosition = Vector2.zero;

            var outPosition = fingerXFlipped ? new Vector2(-100, -100) : new Vector2(100, -100);
            outPosition = fingerYFlipped ? new Vector2(outPosition.x, 100) : outPosition;
            _fingerSequence?.Kill();

            var sequence = DOTween.Sequence();
            sequence.Append(fingerTransform.DOAnchorPos(outPosition, .5f).SetEase(Ease.InOutQuad))
                .Append(fingerTransform.DOAnchorPos(Vector2.zero, .5f).SetEase(Ease.InOutQuad))
                .SetUpdate(true)
                .OnComplete(() => TweenPointing(fingerXFlipped, fingerYFlipped));

            _fingerSequence = sequence;
        }

        private void LateUpdate()
        {
            if (_mode == Mode.Pointing && _targetPoint != null)
                ((RectTransform)transform).anchoredPosition = _targetPoint.AnchoredPosition;
        }

        public void Cancel()
        {
            _fingerSequence?.Kill();
            _fingerSequence = null;
        }

        public void StartMoving(TutorialFocusPoint pointSrc, TutorialFocusPoint pointDst)
        {
            _mode = Mode.Moving;
            TweenFingerPath(pointSrc, pointDst);
        }

        private void TweenFingerPath(TutorialFocusPoint pointSrc, TutorialFocusPoint pointDst)
        {
            _fingerSequence?.Kill();

            fingerImage.color = new Color(1, 1, 1, 0);
            fingerTransform.anchoredPosition = Vector2.zero;
            fingerTransform.localScale = Vector3.one * 1.5f;

            var sequence = DOTween.Sequence();
            sequence.Append(fingerTransform.DOScale(1, .35f))
                .Join(fingerImage.DOFade(1, .35f)
                    .OnUpdate(() => ((RectTransform)transform).anchoredPosition = pointSrc.AnchoredPosition))
                .Append(DOLerp(pointSrc, pointDst, .7f))
                .Append(fingerTransform.DOScale(1.5f, .35f))
                .Join(fingerImage.DOFade(0, .35f))
                .AppendInterval(.5f)
                .AppendCallback(() => TweenFingerPath(pointSrc, pointDst));

            _fingerSequence = sequence;
        }

        private Tween DOLerp(TutorialFocusPoint pointSrc, TutorialFocusPoint pointDst, float duration)
        {
            var t = 0f;
            return DOTween.To(() => t, v =>
            {
                t = v;
                ((RectTransform)transform).anchoredPosition =
                    Vector2.Lerp(pointSrc.AnchoredPosition, pointDst.AnchoredPosition, t);
            }, 1f, duration);
        }
    }
}