using System;
using System.Collections.Generic;
using System.Linq;
using Aloha.Coconut;
using Alohacorp.Durian.Api;
using Alohacorp.Durian.Client;
using Alohacorp.Durian.Model;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;

namespace Aloha.Durian
{
    public class AnnouncementManager : IDisposable
    {
        public IReadOnlyList<Announcement> Announcements => _announcements;
        private List<Announcement> _announcements;

        public IObservable<Unit> OnAnnouncementsUpdated => _onAnnouncementsUpdated;
        private Subject<Unit> _onAnnouncementsUpdated = new Subject<Unit>();

        public bool IsInitialized => _isInitialized;
        private bool _isInitialized;

        private readonly DurianConfig _durianConfig;
        private readonly DurianDaemon _durianDaemon;

        public AnnouncementManager(DurianConfig durianConfig, DurianDaemon durianDaemon)
        {
            _durianConfig = durianConfig;
            _durianDaemon = durianDaemon;

            FetchAnnouncements().ContinueWith(() =>
            {
                _durianDaemon.Register("durian.announcement_manager", FetchAnnouncements, durianConfig.announcementPollingInterval);
                _isInitialized = true;
            });
        }

        public async UniTask FetchAnnouncements()
        {
            var announcementApi = DurianApis.AnnouncementApi();

            if (_announcements != null)
            {
                foreach (var announcement in _announcements)
                {
                    RedDot.SetNotified(announcement.RedDotPath, false);
                }
            }

            _announcements = (await RequestHandler.Request(announcementApi.GetAnnouncementsAsync(),
                    resp =>
                    {
                        var deserialized = JsonConvert.DeserializeObject<List<AnnouncementDto>>(resp.Data.Content.ToString());
                        return deserialized;
                    }))
                .Select(a => new Announcement(a, GetRedDotPath(a.Id.ToString())))
                .Where(a => a.IsActive && a.StartsAt <= Clock.NoDebugNow && a.EndsAt > Clock.NoDebugNow).ToList();

            _announcements.Sort((a, b) => b.StartsAt.CompareTo(a.StartsAt));

            foreach (var announcement in _announcements)
            {
                RedDot.SetNotified(announcement.RedDotPath, !IsAnnouncementRead(announcement.Id));
            }

            _onAnnouncementsUpdated.OnNext(Unit.Default);
        }

        public string GetRedDotPath(string announcementId)
        {
            return $"{_durianConfig.announcementRedDotPath}/{announcementId}";
        }

        public bool IsAnnouncementRead(string announcementId)
        {
            return PlayerPrefs.GetInt($"ann_{announcementId}_read", 0) == 1;
        }

        public void MarkAnnouncementRead(string announcementId)
        {
            PlayerPrefs.SetInt($"ann_{announcementId}_read", 1);
            RedDot.SetNotified(GetRedDotPath(announcementId), false);
        }

        public void Dispose()
        {
            _onAnnouncementsUpdated?.Dispose();
            _durianDaemon.Remove("durian.announcement_manager");
        }
    }
}
