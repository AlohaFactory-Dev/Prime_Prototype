using System;
using System.Collections.Generic;
using System.Linq;
using Aloha.Coconut;
using Alohacorp.Durian.Model;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UniRx;
using UnityEngine.Assertions;

namespace Aloha.Durian
{
    public class MailManager : IPropertyManagerRequirer, IDisposable
    {
        public bool IsInitialized { get; private set; }
        public IReadOnlyList<Mail> Mails => _mails;
        public PropertyManager PropertyManager { private get; set; }

        public IObservable<Unit> OnMailBoxUpdated => _onMailBoxUpdated;
        private Subject<Unit> _onMailBoxUpdated = new();

        private readonly AuthManager _authManager;
        private readonly DurianConfig _durianConfig;
        private readonly DurianDaemon _durianDaemon;
        private readonly IDisposable _authManagerDisposable;
        private List<Mail> _mails;
        private object _mailsLock = new object();

        public MailManager(AuthManager authManager, DurianConfig durianConfig, DurianDaemon durianDaemon)
        {
            _authManager = authManager;
            _durianConfig = durianConfig;
            _durianDaemon = durianDaemon;

            _authManagerDisposable = _authManager.IsSignedIn.Where(isSignedIn => isSignedIn)
                .First()
                .Subscribe(_ =>
                {
                    FetchMails().ContinueWith(() =>
                    {
                        _durianDaemon.Register("durian.mail_manager", FetchMails, _durianConfig.mailPollingInterval);
                        IsInitialized = true;
                    });
                });
        }

        public async UniTask FetchMails()
        {
            var mailApi = await DurianApis.MailApi();

            if (_mails != null)
            {
                foreach (var mail in _mails) SetRedDot(mail, false);
            }

            _mails = (await RequestHandler.Request(mailApi.GetMailsAsync(),
                    resp =>
                    {
                        var deserialized = JsonConvert.DeserializeObject<List<PlayerMailDto>>(resp.Data.Content.ToString());
                        return deserialized;
                    }))
                .Select(mailDto => new Mail(mailDto)).ToList();
            foreach (var mail in _mails) SetRedDot(mail, mail.ReceivedAt == null && mail.Attachments.Count > 0);

            _onMailBoxUpdated.OnNext(Unit.Default);
        }

        private void SetRedDot(Mail mail, bool value)
        {
            RedDot.SetNotified(GetRedDotPath(mail.Id), value);
        }

        public string GetRedDotPath(string mailId)
        {
            return $"{_durianConfig.mailRedDotPath}/{mailId}";
        }

        public async UniTask<List<Property>> ReceiveAllAttachments()
        {
            var mails = _mails.Where(m => m.State != Mail.StateEnum.RECEIVED);
            var tasks = mails.Select(mail => ReceiveAttachments(mail.Id));
            var results = await UniTask.WhenAll(tasks);

            return results.SelectMany(result => result).ToList();
        }

        public async UniTask<List<Property>> ReceiveAttachments(string mailId)
        {
            Assert.IsNotNull(PropertyManager);

            var mailApi = await DurianApis.MailApi();

            var result = new List<Property>();
            var targetMail = _mails.FirstOrDefault(mail => mail.Id == mailId);

            if (targetMail != null && targetMail.ReceivedAt == null)
            {
                await RequestHandler.Request(mailApi.ReceiveAttachmentAsync(mailId));
                var updatedMailDto = await RequestHandler.Request(mailApi.GetMailAsync(mailId), resp => resp.Data);

                if (updatedMailDto != null)
                {
                    var updatedMail = new Mail(updatedMailDto);
                    var attachments = targetMail.Attachments;
                    foreach (var attachment in attachments)
                    {
                        result.Add(attachment);
                    }

                    lock (_mailsLock)
                    {
                        result = PropertyManager.Obtain(result, PlayerAction.Get(_durianConfig.mailPlayerActionId));
                        _mails[_mails.IndexOf(targetMail)] = updatedMail;
                        SetRedDot(updatedMail, false);
                        _onMailBoxUpdated.OnNext(Unit.Default);
                    }
                }
            }

            return result;
        }

        public Mail FindMail(string mailId)
        {
            return _mails.FirstOrDefault(mail => mail.Id == mailId);
        }

        public async UniTask SendMailToSelf(string title, string content, List<Property> attachments, int expireDaysAfter)
        {
            var playerApi = await DurianApis.PlayerApi();

            var attachmentsDto = new List<MailAttachmentDto>();
            foreach (var attachment in attachments)
            {
                attachmentsDto.Add(new MailAttachmentDto(attachment.type.alias, (int)attachment.amount));
            }

            var mailDto = new MailDto(title, content, null, attachmentsDto);
            await RequestHandler.Request(playerApi.SendMailAsync(_authManager.UID, new PlayerMailSendReqDto(mailDto, expireDaysAfter)), dto => dto.Data);
        }

        public void Dispose()
        {
            _onMailBoxUpdated?.Dispose();
            _authManagerDisposable?.Dispose();
            _durianDaemon.Remove("durian.mail_manager");
        }
    }
}