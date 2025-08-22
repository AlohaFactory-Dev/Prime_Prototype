using Aloha.Coconut;
using Aloha.Coconut.UI;
using Aloha.Durian;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class SampleUI : UISlice
{
    [Inject] private SampleGameData _sampleGameData;
    [Inject] private SaveDataManager _saveDataManager;
    [Inject] private NotificationManager _notifactionManager;
    [Inject] private AuthManager _authManager;

    [SerializeField] private TMP_Text uidText;
    [SerializeField] private Button uidCopyButton;
    [SerializeField] private Button kickMySelfButton;
    [SerializeField] private TMP_Text localCounterText;
    [SerializeField] private TMP_Text remoteCounterText;
    [SerializeField] private TMP_Text remoteCounter2Text;

    void Start()
    {
        uidText.text = $"UID: {_authManager.UID}";

        uidCopyButton.onClick.AddListener(() =>
        {
            GUIUtility.systemCopyBuffer = _authManager.UID;
            SystemUI.ShowToastMessage("UID Copied");
        });
        
        kickMySelfButton.onClick.AddListener(async () =>
        {
            await _authManager.BanMySelf("Test");
            await SystemUI.ShowDialogue("Ban", "You have been banned", "OK");
            Application.Quit();
        });

        localCounterText.text = $"Local: {_sampleGameData.LocalCounter}";
        remoteCounterText.text = $"Remote: {_sampleGameData.RemoteCounter}";
        remoteCounter2Text.text = $"Remote2: {_sampleGameData.RemoteCounter2}";
    }

    public void AddLocalCounter()
    {
        _sampleGameData.AddLocalCounter(1);
        localCounterText.text = $"Local: {_sampleGameData.LocalCounter}";
        _saveDataManager.Save();
    }

    public void MinusLocalCounter()
    {
        _sampleGameData.AddLocalCounter(-1);
        localCounterText.text = $"Local: {_sampleGameData.LocalCounter}";
        _saveDataManager.Save();
    }

    public void AddRemoteCounter()
    {
        _sampleGameData.AddRemoteCounter(1);
        remoteCounterText.text = $"Remote: {_sampleGameData.RemoteCounter}";
        _saveDataManager.Save();
    }

    public void MinusRemoteCounter()
    {
        _sampleGameData.AddRemoteCounter(-1);
        remoteCounterText.text = $"Remote: {_sampleGameData.RemoteCounter}";
        _saveDataManager.Save();
    }

    public void AddRemoteCounter2()
    {
        _sampleGameData.AddRemoteCounter2(1);
        remoteCounter2Text.text = $"Remote2: {_sampleGameData.RemoteCounter2}";
        _saveDataManager.Save();
    }

    public void MinusRemoteCounter2()
    {
        _sampleGameData.AddRemoteCounter2(-1);
        remoteCounter2Text.text = $"Remote2: {_sampleGameData.RemoteCounter2}";
        _saveDataManager.Save();
    }

    public void SendNotification10Sec()
    {
        _notifactionManager.Send("10Sec", "10Sec", "10Sec", System.DateTime.Now.AddSeconds(10));
    }

    public void SendNotification30Sec()
    {
        _notifactionManager.Send("30Sec", "30Sec", "30Sec", System.DateTime.Now.AddSeconds(30));
    }

    public void CancelNotifications10Sec()
    {
        _notifactionManager.Cancel("10Sec");
    }

    public void CancelNotifications30Sec()
    {
        _notifactionManager.Cancel("30Sec");
    }

    public void Delete()
    {
        _saveDataManager.DeleteAll();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
