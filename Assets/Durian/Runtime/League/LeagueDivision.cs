using System;

namespace Aloha.Durian
{
    public class LeagueDivision
    {
        public readonly string leagueGroupId;
        public readonly string leagueSeasonId;
        public readonly string leagueId;
        public readonly string divisionId;
        public readonly DateTime startTime;
        public readonly DateTime endTime;
        public readonly LeagueEnum league;
        public readonly Leaderboard leaderboard;

        public LeagueDivision(string leagueGroupId, string leagueSeasonId, string leagueId, string divisionId,
            DateTime startTime, DateTime endTime, LeagueEnum league, Leaderboard leaderboard)
        {
            this.leagueSeasonId = leagueSeasonId;
            this.leagueGroupId = leagueGroupId;
            this.leagueId = leagueId;
            this.divisionId = divisionId;
            this.startTime = startTime;
            this.endTime = endTime;
            this.league = league;
            this.leaderboard = leaderboard;
        }

        public static bool operator ==(LeagueDivision a, LeagueDivision b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.leagueSeasonId == b.leagueSeasonId && a.league == b.league && a.leagueGroupId == b.leagueGroupId
                   && a.leagueId == b.leagueId && a.divisionId == b.divisionId;
        }

        public static bool operator !=(LeagueDivision a, LeagueDivision b)
        {
            return !(a == b);
        }
    }
}