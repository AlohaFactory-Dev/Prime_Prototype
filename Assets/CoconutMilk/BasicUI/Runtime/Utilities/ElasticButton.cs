using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnityEngine.UI.Selectable))]
public class ElasticButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Transform _targetTransform;

    private Vector3 _originalScale;
    private UnityEngine.UI.Selectable _selectable;
    private Tween _tween;

    void Awake()
    {
        _selectable = GetComponent<UnityEngine.UI.Selectable>();
        if (_targetTransform == null) _targetTransform = transform;
        _originalScale = _targetTransform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_selectable.interactable)
        {
            _targetTransform.localScale = _originalScale;
            _tween?.Kill();
            _tween = _targetTransform.DOScale(0.9f * _originalScale, 0.15f).SetUpdate(UpdateType.Late);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_selectable.interactable)
        {
            _targetTransform.localScale = 0.9f * _originalScale;
            _tween?.Kill();
            _tween = _targetTransform.DOScale(_originalScale, 0.5f).SetEase(Ease.OutElastic).SetUpdate(UpdateType.Late);
        }

        ;
    }

    public void PlayPointerUp()
    {
        _targetTransform.localScale = 0.9f * _originalScale;
        _tween?.Kill();
        _tween = _targetTransform.DOScale(_originalScale, 0.5f).SetEase(Ease.OutElastic).SetUpdate(UpdateType.Late);
    }

    public void PlayPointerClick()
    {
        _targetTransform.localScale = _originalScale;
        _tween?.Kill();
        _tween = _targetTransform.DOScale(0.9f * _originalScale, 0.15f).SetUpdate(UpdateType.Late);
        _tween.OnComplete(() =>
        {
            _targetTransform.localScale = 0.9f * _originalScale;
            _tween = _targetTransform.DOScale(_originalScale, 0.5f).SetEase(Ease.OutElastic).SetUpdate(UpdateType.Late);
        });
    }

    private void Reset()
    {
        _targetTransform = transform;
    }
}