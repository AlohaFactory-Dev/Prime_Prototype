using System.Threading.Tasks;
using UnityEngine;

namespace Aloha.Sdk
{
    public partial class AlohaSdk
    {
        public static class PrivacyPolicy
        {
            private const string KEY_CONFIRMED = "aloha-pp_confirmed";
            public static bool IsConfirmed => PlayerPrefs.HasKey(KEY_CONFIRMED);

            public static async Task ShowPopup()
            {
                if (IsConfirmed) return;

                var taskCompletionSource = new TaskCompletionSource<bool>();
                var popUpPrefab = Resources.Load<AlohaPrivacyPolicyPopUp>("AlohaPrivacyPolicyPopup");
                var popUp = Instantiate(popUpPrefab);

                popUp.OnStartGame += () =>
                {
                    PlayerPrefs.SetInt(KEY_CONFIRMED, 1);
                    taskCompletionSource.SetResult(true);
                };
                popUp.Show();

                await taskCompletionSource.Task;
                await Task.Delay(100);
            }

            public static string GetTermsLink(SystemLanguage language)
            {
                switch (language)
                {
                    case SystemLanguage.Korean:
                        return "https://www.aloha-corp.com/privacy";
                    case SystemLanguage.English:
                        return "https://www.aloha-corp.com/en/privacy";
                    default:
                        return "https://www.aloha-corp.com/en/privacy";
                }
            }

            public static string GetTermsLink(string language)
            {
                switch (language)
                {
                    case "Korean":
                        return "https://www.aloha-corp.com/privacy";
                    case "English":
                        return "https://www.aloha-corp.com/en/privacy";
                    default:
                        return "https://www.aloha-corp.com/en/privacy";
                }
            }
        }
    }
}