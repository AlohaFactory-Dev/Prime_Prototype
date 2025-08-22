using System;
using System.Collections.Generic;
using Aloha.Coconut;
using Alohacorp.Durian.Model;

namespace Aloha.Durian
{
    public static class LeaderboardEntityFactory
    {
        public static LeaderboardReward CreateReward(LeaderboardRewardConditionDto conditionDto)
        {
            var conditionType = (LeaderboardReward.ConditionType)conditionDto.Condition.ConditionType;
            var minValue = conditionDto.Condition.MinValue != null ? (int)conditionDto.Condition.MinValue.Value : 0;
            var maxValue = conditionDto.Condition.MaxValue != null ? (int)conditionDto.Condition.MaxValue.Value : 0;

            var rewards = new List<Property>();
            foreach (var attachment in conditionDto.RewardMail.Attachments)
            {
                rewards.Add(new Property(attachment.Content, attachment.Quantity.Value));
            }

            return new LeaderboardReward(conditionType, minValue, maxValue, rewards);
        }

        public static Leaderboard CreateLeaderboard(LeaderboardPeriodDto periodDto, string name)
        {
            var endAt = periodDto.EndAt.Value.ToDateTime();
            return new Leaderboard(name, periodDto.LeaderboardId, periodDto.Id, endAt);
        }

        public static LeaderboardEntry CreateEntry(LeaderboardPeriodRecordDto recordDto)
        {
            return new LeaderboardEntry(recordDto.ParticipantId, recordDto.Participant.IsBot,
                recordDto.Participant.Name, (int)recordDto.Score, (int)recordDto.Rank.Value);
        }
    }
}
