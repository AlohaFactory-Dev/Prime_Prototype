using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteInEditMode]
public class RelativeRectSetter : MonoBehaviour
{
    [SerializeField] private RectTransform targetTransform;
    [SerializeField] private Vector2 relativeSize;

    [SerializeField] private bool useRelativePosition;
    [SerializeField, ShowIf("useRelativePosition")] private Vector2 relativePosition;

    void LateUpdate()
    {
        if (targetTransform == null) return;
        ((RectTransform)transform).sizeDelta = new Vector2(
            relativeSize.x * targetTransform.rect.width,
            relativeSize.y * targetTransform.rect.height
        );

        if (useRelativePosition)
        {
            ((RectTransform)transform).anchoredPosition = new Vector2(
                relativePosition.x * targetTransform.rect.width,
                relativePosition.y * targetTransform.rect.height
            );
        }
    }
}
