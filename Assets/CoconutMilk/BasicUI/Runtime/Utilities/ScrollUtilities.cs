using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public static class ScrollUtilities
{
    public static void Scroll(ScrollRect scrollRect, RectTransform target, float duration = 0f, float normalizedOffset = 0.5f)
    {
        Assert.IsTrue(target.IsChildOf(scrollRect.content.transform), "target이 scrollRect.content의 자식이어야 합니다."); 
        
        var content = scrollRect.content;
        Assert.IsTrue(content.anchorMax.y == 1, "scrollRect.content의 anchorMax.y가 1이어야 합니다.");
        Assert.IsTrue(content.anchorMin.y == 1, "scrollRect.content의 anchorMin.y가 1이어야 합니다.");
        
        normalizedOffset = Mathf.Clamp01(normalizedOffset);
        
        var viewportHeight = scrollRect.viewport.rect.height * scrollRect.transform.lossyScale.y;
        var contentHeight = content.rect.height * content.lossyScale.y;
        var relativePosition = content.position.y - target.position.y - normalizedOffset * viewportHeight;
        var targetNormalizedPosition = Mathf.Clamp01(1 - (relativePosition / (contentHeight - viewportHeight)));

        if (duration == 0)
        {
            scrollRect.verticalNormalizedPosition = targetNormalizedPosition;   
        }
        else
        {
            scrollRect.DOVerticalNormalizedPos(targetNormalizedPosition, duration).SetEase(Ease.OutCubic);
        }
    }

    public static void ScrollTo(this ScrollRect scrollRect, RectTransform target, float duration = 0f, float normalizedOffset = 0.5f)
    {
        Scroll(scrollRect, target, duration, normalizedOffset);
    }
}
