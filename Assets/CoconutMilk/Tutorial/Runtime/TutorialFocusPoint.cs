using Coffee.UIExtensions;
using DG.Tweening;
using UnityEngine;

namespace Aloha.CoconutMilk
{
    public class TutorialFocusPoint : MonoBehaviour
    {
        public Vector2 AnchoredPosition =>
            _isUI ? ((RectTransform)unmask_UI.transform).anchoredPosition : ((RectTransform)transform).anchoredPosition;

        [SerializeField] private Canvas canvas;
        [SerializeField] private Unmask unmask_UI;
        [SerializeField] private Unmask unmask_GameObject;

        private bool _isUI;
        private GameObject _target;
        private Camera _camera;

        public void FocusOn(GameObject target)
        {
            _target = target;
            _isUI = IsUI(_target);

            if (_isUI) FocusOnUI(target);
            else FocusOnGameObject();
        }

        public void FocusOn(Vector2 position)
        {
            _isUI = false;

            var targetRectTransform = (RectTransform)transform;
            targetRectTransform.position = position;

            unmask_UI.fitTarget = targetRectTransform;

            unmask_UI.gameObject.SetActive(true);
            unmask_GameObject.gameObject.SetActive(false);

            TweenUnmask(unmask_UI);
        }
    
        public void SetSize(Vector2 size)
        {
            ((RectTransform)transform).sizeDelta = size;
        }

        private bool IsUI(GameObject target)
        {
            Canvas result = null;
            Transform findTransform = target.transform;

            while (findTransform != null)
            {
                if (findTransform.TryGetComponent<Canvas>(out var c))
                {
                    result = c;
                }

                findTransform = findTransform.parent;
            }

            return result != null && result.renderMode == RenderMode.ScreenSpaceOverlay;
        }

        private void FocusOnUI(GameObject target)
        {
            ((RectTransform)transform).anchoredPosition = Vector2.zero;

            unmask_UI.fitTarget = target.transform as RectTransform;

            unmask_UI.gameObject.SetActive(true);
            unmask_GameObject.gameObject.SetActive(false);

            TweenUnmask(unmask_UI);
        }

        private void TweenUnmask(Unmask unmask)
        {
            unmask.scaleMultiplier = 2;
            unmask.isFilteringRaycast = false;
            DOTween.To(() => unmask.scaleMultiplier,
                    m => unmask.scaleMultiplier = m,
                    1f, 0.5f).SetEase(Ease.InQuart)
                .SetUpdate(true)
                .OnComplete(() => unmask.isFilteringRaycast = true);
        }

        private void FocusOnGameObject()
        {
            unmask_UI.gameObject.SetActive(false);
            unmask_GameObject.gameObject.SetActive(true);

            TweenUnmask(unmask_GameObject);
        }

        public void StopTracking()
        {
            unmask_UI.gameObject.SetActive(false);
            unmask_GameObject.gameObject.SetActive(false);
            _target = null;
        }

        private void LateUpdate()
        {
            if (_target != null && !_isUI)
            {
                ((RectTransform)transform).anchoredPosition = ConvertGameObjectPosition(_target);
            }
        }

        private Vector2 ConvertGameObjectPosition(GameObject target)
        {
            return OverlayCanvasUtility.ConvertGameObjectPosition(target, _camera, canvas);
        }

        public void SetCamera(Camera camera)
        {
            _camera = camera;
        }
    }
}