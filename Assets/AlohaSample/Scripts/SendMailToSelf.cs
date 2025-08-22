using System.Collections.Generic;
using Aloha.Coconut;
using Aloha.Durian;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class SendMailToSelf : MonoBehaviour
{
    [Inject] private MailManager _mailManager;
    
    [Button]
    public void Send()
    {
        SendAndReceive().Forget();
    }

    private async UniTaskVoid SendAndReceive()
    {
        await _mailManager.SendMailToSelf("aa", "dfkldfldf", new List<Property>(), 10);
        await _mailManager.FetchMails();
    }
}
