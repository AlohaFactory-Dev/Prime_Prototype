using System;
using Newtonsoft.Json;

namespace Aloha.Coconut
{
    // 게임 내의 "하루"를 판단하는 기준은 자정이 아닌 경우가 많음 (예: 5시에 리셋)
    // 게임 내에서 날짜를 나타낼 때 혼동을 방지하기 위해 GameDate 클래스를 사용해서 비교
    // 5시에 리셋일 때 11월 27일 3시라면, GameDate은 26일로 취급, 5시 이후부터 27일로 취급
    [JsonObject(MemberSerialization.OptIn)]
    public struct GameDate : IComparable<GameDate>
    {
        public DateTime Date => new DateTime(Year, Month, Day);
        public DateTime StartDateTime => new DateTime(Year, Month, Day, Clock.RESET_TIME, 0, 0);
        public DateTime EndDateTime => new DateTime(Year, Month, Day, Clock.RESET_TIME, 0, 0).AddDays(1);
        
        public int Year { get; }
        public int Month { get; }
        public int Day { get; }
        
        [JsonProperty] private int yyyymmdd => Year * 10000 + Month * 100 + Day;

        [JsonConstructor]
        public GameDate(int yyyymmdd)
        {
            Year = yyyymmdd / 10000;
            Month = (yyyymmdd % 10000) / 100;
            Day = yyyymmdd % 100;
        }

        public GameDate(int yyyy, int mm, int dd)
        {
            Year = yyyy;
            Month = mm;
            Day = dd;
        }

        public GameDate(DateTime dateTime)
        {
            if (dateTime.Hour < Clock.RESET_TIME)
            {
                dateTime = dateTime.AddDays(-1);
            }
            
            Year = dateTime.Year;
            Month = dateTime.Month;
            Day = dateTime.Day;
        }

        public GameDate AddDay(int day)
        {
            var dateTime = StartDateTime.AddDays(day);
            return new GameDate(dateTime);
        }
        
        public GameDate AddMonth(int month)
        {
            var dateTime = StartDateTime.AddMonths(month);
            return new GameDate(dateTime);
        }
        
        public GameDate AddYear(int year)
        {
            var dateTime = StartDateTime.AddYears(year);
            return new GameDate(dateTime);
        }
        
        public static bool operator ==(GameDate a, GameDate b)
        {
            return a.Year == b.Year && a.Month == b.Month && a.Day == b.Day;
        }
        
        public static bool operator !=(GameDate a, GameDate b)
        {
            return a.Year != b.Year || a.Month != b.Month || a.Day != b.Day;
        }
        
        public static bool operator >(GameDate a, GameDate b)
        {
            if (a.Year > b.Year)
            {
                return true;
            }
            if (a.Year < b.Year)
            {
                return false;
            }
            if (a.Month > b.Month)
            {
                return true;
            }
            if (a.Month < b.Month)
            {
                return false;
            }
            return a.Day > b.Day;
        }

        public static bool operator <(GameDate a, GameDate b)
        {
            if (a.Year < b.Year)
            {
                return true;
            }
            if (a.Year > b.Year)
            {
                return false;
            }
            if (a.Month < b.Month)
            {
                return true;
            }
            if (a.Month > b.Month)
            {
                return false;
            }
            return a.Day < b.Day;
        }
        
        public static bool operator <=(GameDate a, GameDate b)
        {
            return a == b || a < b;
        }
        
        public static bool operator >=(GameDate a, GameDate b)
        {
            return a == b || a > b;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is GameDate date)
            {
                return this == date;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return yyyymmdd;
        }

        public int CompareTo(GameDate other)
        {
            int yearComparison = Year.CompareTo(other.Year);
            if (yearComparison != 0) return yearComparison;
            int monthComparison = Month.CompareTo(other.Month);
            if (monthComparison != 0) return monthComparison;
            return Day.CompareTo(other.Day);
        }
    }
}
