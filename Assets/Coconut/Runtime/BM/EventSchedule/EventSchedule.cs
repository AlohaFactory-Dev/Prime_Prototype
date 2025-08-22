using UnityEngine.Assertions;

namespace Aloha.Coconut
{
    public class EventSchedule
    {
        public int Id { get; }
        public bool IsCustom { get; }
        public string Type { get; }
        public int Var { get; }
        public GameDate From { get; }

        public GameDate To { get; }
        internal EventSchedule(EventScheduleData data, GameDate firstDate)
        {
            Assert.IsTrue(data.from <= data.to, $"EventScheduleData {data.id} has invalid date range");

            if (data.isCustom == 0)
            {
                Assert.IsTrue(data.from > 2000_01_01, $"EventScheduleData {data.id} has invalid date range");
                Assert.IsTrue(data.to > 2000_01_01, $"EventScheduleData {data.id} has invalid date range");   
            }
            
            Id = data.id;
            IsCustom = data.isCustom == 1;
            Type = data.type;
            Var = data.var;
            From = !IsCustom ? new GameDate(data.from) : firstDate.AddDay(data.from - 1); // 1이 첫번째 날이므로, 1을 빼줌
            To = !IsCustom ? new GameDate(data.to) : firstDate.AddDay(data.to - 1); // 1이 첫번째 날이므로, 1을 빼줌
        }
    }
}
