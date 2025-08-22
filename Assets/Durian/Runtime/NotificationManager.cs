using Firebase.Messaging;
using System;
using Aloha.Sdk;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Alohacorp.Durian.Model;
using UniRx;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif

namespace Aloha.Durian
{
    public class NotificationManager : IDisposable
    {
        private readonly AuthManager _authManager;
        private readonly IDisposable _authManagerDisposable;

        private string ChannelId => $"{Application.identifier}.default";
        private bool _isInitialized;
        private string _fcmToken;

        public bool IsNotificationOff
        {
            get => PlayerPrefs.GetInt("notification_off") == 1;
            set
            {
                if (_isInitialized)
                {
#if UNITY_ANDROID
                    if (value) AndroidNotificationCenter.CancelAllNotifications();
#elif UNITY_IOS
                    if (value) iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
                }

                var prevValue = IsNotificationOff;
                if (value && !prevValue)
                {
                    // Off됐을 때
                    if (!string.IsNullOrEmpty(_fcmToken))
                    {
                        UnregisterNotificationTokenAsync(_fcmToken).Forget();
                    }
                }
                else
                {
                    // On됐을 때
                    if (!string.IsNullOrEmpty(_fcmToken))
                    {
                        RegisterNotificationTokenAsync(_fcmToken).Forget();
                    }
                }

                PlayerPrefs.SetInt("notification_off", value ? 1 : 0);
            }
        }

        public NotificationManager(AuthManager authManager)
        {
            _authManager = authManager;
            _authManagerDisposable = _authManager.IsSignedIn.Where(isSignedIn => isSignedIn)
                .First()
                .Subscribe(_ =>
                {
                    Debug.Log("AuthManager signed in");
                    Initialize();
                });
        }

        private void Initialize()
        {
            Debug.Log("NotificationManager Initialize");
            // 서버 Api에서 처리, 자체 Registration은 항상 false
            FirebaseMessaging.TokenRegistrationOnInitEnabled = false;

            if (!PlayerPrefs.HasKey("notification_off"))
            {
                IsNotificationOff = AlohaSdk.RemoteConfig.Predefined.SHOW_PRIVACY_POLICY_POPUP_ON_START && !AlohaSdk.Context.NotificationAgreed;
            }

            Debug.Log($"IsNotificationOff: {IsNotificationOff}");

#if UNITY_ANDROID
            var channel = new AndroidNotificationChannel()
            {
                Id = ChannelId,
                Name = ChannelId,
                Importance = Importance.Default,
                Description = "Generic notifications",
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
            AndroidNotificationCenter.CancelAllDisplayedNotifications();
#endif

            Debug.Log("NotificationManager Messaging Subscribe");
            Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
            Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
            Debug.Log("NotificationManager Messaging Subscribe complete");

            if (IsNotificationOff)
            {
#if UNITY_ANDROID
                AndroidNotificationCenter.CancelAllNotifications();
#elif UNITY_IOS
                iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
            }

            _isInitialized = true;
            Debug.Log("NotificationManager Initialized");
        }

        private async UniTaskVoid RegisterNotificationTokenAsync(string token)
        {
            var notificationApi = await DurianApis.NotificationApi();
            var requestDto = new PlayerFCMTokenRegisterReqDto(token);
            await RequestHandler.Request(notificationApi.RegisterDeviceAsync(requestDto), false, 3);
        }

        private async UniTaskVoid UnregisterNotificationTokenAsync(string token)
        {
            var notificationApi = await DurianApis.NotificationApi();
            var requestDto = new PlayerFCMTokenRegisterReqDto(token);
            await RequestHandler.Request(notificationApi.UnregisterDeviceAsync(requestDto), false, 3);
        }

        private void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
        {
            Debug.Log("Received Registration Token: " + token.Token);
            _fcmToken = token.Token;

            if (!IsNotificationOff)
            {
                RegisterNotificationTokenAsync(_fcmToken).Forget();
            }
        }

        private void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
        {
            Debug.Log("Received a new message from: " + e.Message.From);
        }

        public void Send(string id, string title, string text, DateTime time)
        {
            Debug.Log($"Send Notification: {id} {time: yyyy-MM-dd HH:mm:ss}");
            if (IsNotificationOff) return;

#if UNITY_ANDROID
            var notification = new AndroidNotification()
            {
                Title = title,
                Text = text,
                FireTime = time,
                SmallIcon = "icon_0",
                LargeIcon = "icon_1"
            };
            int notificationId = AndroidNotificationCenter.SendNotification(notification, ChannelId);
            PlayerPrefs.SetInt(id, notificationId);
            Debug.Log($"Notification ID: {notificationId} sent");
#elif UNITY_IOS
            var calenderTrigger = new iOSNotificationCalendarTrigger()
            {
                Day = time.Day,
                Hour = time.Hour,
                Minute = time.Minute,
                Second = time.Second,
                Repeats = false
            };

            var notification = new iOSNotification()
            {
                // You can specify a custom identifier which can be used to manage the notification later.
                // If you don't provide one, a unique string will be generated automatically.
                Title = title,
                Body = text,
                Subtitle = "",
                ShowInForeground = true,
                ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
                CategoryIdentifier = ChannelId,
                ThreadIdentifier = "CrownRumble - Default Channel",
                Trigger = calenderTrigger,
            };

            iOSNotificationCenter.ScheduleNotification(notification);
            PlayerPrefs.SetString(id, notification.Identifier);
#endif
        }

        public void Cancel(string id)
        {
#if UNITY_ANDROID
            if (PlayerPrefs.HasKey(id))
            {
                AndroidNotificationCenter.CancelNotification(PlayerPrefs.GetInt(id));
            }
#elif UNITY_IOS            
            if (PlayerPrefs.HasKey(id))
            {
                iOSNotificationCenter.RemoveScheduledNotification(PlayerPrefs.GetString(id));
            }
#endif
        }

        public void Dispose()
        {
            _authManagerDisposable?.Dispose();
        }
    }
}
