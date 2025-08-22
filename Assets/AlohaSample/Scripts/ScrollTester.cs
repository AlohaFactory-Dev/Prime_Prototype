using Aloha.Coconut.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class ScrollTester : UISlice
{
    [SerializeField] private RectTransform content;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private float normalizedOffset = 0.5f;
    [SerializeField] private float scrollTime = 0.5f;

    void Start()
    {
        var buttons = content.GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
            button.GetComponentInChildren<TMP_Text>()?.SetText(button.name);
            button.onClick.AddListener(() =>
            {
                scrollRect.ScrollTo(button.GetComponent<RectTransform>(), scrollTime, normalizedOffset);        
            });
        }
    }
}
