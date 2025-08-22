using Aloha.Coconut.Launcher;
using Aloha.Durian;
using TMPro;
using UnityEngine;
using Zenject;

public class TitleScreen : MonoBehaviour, ITitleScreen
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text uidText;

    // [Inject] private AuthManager _authManager;

    public void Report(float value)
    {
        Debug.Log($"Report: {value}");
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }

    // void Update()
    // {
    //     if (_authManager != null && _authManager.IsSignedIn.Value)
    //     {
    //         uidText.text = $"UID: {_authManager.UID}";
    //     }
    // }
}