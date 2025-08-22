using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Aloha.Durian
{
    public class APITester : MonoBehaviour
    {
        [Inject] private LeaderboardManager _leaderboardManager;
        [Inject] private AuthManager _authManager;
        [Inject] private OtherPlayerDataManager _otherPlayerDataManager;

        [Button]
        public void LoadTest()
        {
            _leaderboardManager.GetLeaderboard("Test").Forget();
        }

        [SerializeField] private int score;
        [Button]
        public void SubmitTest()
        {
            _leaderboardManager.UpdateMyScore("Test", score).Forget();
        }

        [Button]
        public void GetPlayerRankTest()
        {
            _leaderboardManager.FetchMyRank("Test").Forget();
        }

        [Button]
        public void RefreshTest()
        {
            _leaderboardManager.RefreshLeaderboard("Test").Forget();
        }

        [SerializeField] private string playerUID;
        [Button]
        public void GetPlayerDataTest()
        {
            if (string.IsNullOrEmpty(playerUID)) playerUID = _authManager.UID;

            _otherPlayerDataManager.Get<SampleGameData.RemoteSaveData>(playerUID, "sample_remote")
                .ContinueWith(data =>
                {
                    Debug.Log($"sample_remote : {data.counter}");
                });

            _otherPlayerDataManager.Get<SampleGameData.RemoteSaveData>(playerUID, "sample_remote2")
                .ContinueWith(data =>
                {
                    Debug.Log($"sample_remote2 : {data.counter}");
                });
        }

        [SerializeField] private string targetLeaderboardName;
        [Button]
        public void GetLeaderboardTest()
        {
            GetLeaderboardTestAsync(targetLeaderboardName).Forget();
        }

        private async UniTask GetLeaderboardTestAsync(string leaderboardName)
        {
            Leaderboard leaderboard = await _leaderboardManager.GetLeaderboard(leaderboardName);
            Debug.Log($"Leaderboard {leaderboardName} loaded");
            Debug.Log($"End at : {leaderboard.EndAt}");
        }

        public static DateTime ToDateTime(long unixTimeMilliseconds)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddMilliseconds(unixTimeMilliseconds).ToLocalTime();
            return dt;
        }
    }
}
