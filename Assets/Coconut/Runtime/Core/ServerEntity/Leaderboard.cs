using System;
using System.Collections.Generic;
using Aloha.Coconut;
using UniRx;
using UnityEngine;

namespace Aloha.Durian
{
    public class Leaderboard
    {
        public IReadOnlyReactiveProperty<bool> IsUpdating => _isUpdating.Select(value => value > 0).ToReactiveProperty();
        private readonly ReactiveProperty<int> _isUpdating = new ReactiveProperty<int>(0);
        
        public string Name { get; }
        public string LeaderboardId { get; }
        public string PeriodId { get; }

        public bool IsValid => Clock.NoDebugNow < _endAt;
        public DateTime LastUpdateAt => _lastUpdateAt;

        public List<LeaderboardEntry> Entries { get; } = new List<LeaderboardEntry>();
        public LeaderboardEntry MyEntry { get; set; }

        public DateTime EndAt => _endAt;
        private DateTime _endAt;
        private DateTime _lastUpdateAt;

        public Leaderboard(string name, string leaderboardId, string periodId, DateTime endAt)
        {
            Name = name;
            LeaderboardId = leaderboardId;
            PeriodId = periodId;
            _endAt = endAt;
            _lastUpdateAt = Clock.NoDebugNow;
        }

        // Mock purpose
        public Leaderboard(string name)
        {
            Name = name;
            LeaderboardId = $"mock_{name}";
            PeriodId = $"mock_period_{name}";
        }
        
        public void SetIsUpdating(bool isUpdating)
        {
            if (isUpdating)
            {
                _isUpdating.Value++;
            }
            else
            {
                _isUpdating.Value = Math.Max(0, _isUpdating.Value - 1);
            }
        }

        public void ResetLastUpdateAt() => _lastUpdateAt = Clock.NoDebugNow;
    }

    public class LeaderboardEntry
    {
        public string UID { get; }
        public string Nickname { get; private set; }
        public int Score { get; set; }
        public int Rank { get; set; }
        public bool IsBot { get; private set; }

        public LeaderboardEntry(string uid, bool isBot, string nickname, int score, int rank)
        {
            UID = uid;
            IsBot = isBot;
            if (string.IsNullOrEmpty(nickname))
            {
                if (isBot)
                {
                    Nickname = $"Player_{uid.Substring(6, 4)}"; // 봇은 봇임을 나타내는 문자열 6자리가 prefix로 들어옴
                }
                else
                {
                    Nickname = $"Player_{uid.Substring(0, 4)}";
                }
            }
            else
            {
                Nickname = nickname;
            }

            Score = score;
            Rank = rank;
        }

        public void OverrideNickname(string nickname)
        {
            Nickname = nickname;
        }

        // Record가 없을 경우, UID만 받아서 설정 후 Score는 0처리
        public LeaderboardEntry(string uid)
        {
            UID = uid;
            Nickname = $"Player_{uid.Substring(0, Mathf.Max(0, 4))}";
            Score = 0;
            Rank = int.MaxValue;
        }

        public LeaderboardEntry(string uid, int score, int rank)
        {
            UID = uid;
            Score = score;
            Rank = rank;
        }
    }
}
