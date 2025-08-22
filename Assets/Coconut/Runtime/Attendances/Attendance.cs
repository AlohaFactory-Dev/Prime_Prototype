using System;
using System.Collections.Generic;

namespace Aloha.Coconut.Attendances
{
    public class Attendance : IDisposable
    {
        public int DayCount => _saveData.dayCount;
        public int LastClaimedDay => _saveData.lastClaimedDay;
        public List<AttendanceNode> Nodes => _nodes;
        public bool IsCompleted => LastClaimedDay >= Nodes[^1].day;
        
        private readonly string _id;
        private readonly List<AttendanceNode> _nodes;
        private readonly SaveData _saveData;
        private readonly PeriodicResetHandler _periodicResetHandler;
        private readonly PropertyManager _propertyManager;

        public Attendance(string id, List<AttendanceNode> nodes, SaveData saveData, 
            PeriodicResetHandler periodicResetHandler, PropertyManager propertyManager)
        {
            _id = id;
            _nodes = nodes;
            _nodes.Sort((a, b) => a.day.CompareTo(b.day));
            _saveData = saveData;
            _periodicResetHandler = periodicResetHandler;
            _propertyManager = propertyManager;

            _periodicResetHandler.AddResetCallback(ResetPeriod.Daily, $"attendance_{_id}", OnDailyReset);

            foreach (var node in _nodes)
            {
                node.IsClaimed = node.day <= LastClaimedDay;
                node.IsClaimable = node.day <= DayCount;
            }
        }

        private void OnDailyReset()
        {
            _saveData.dayCount++;
            foreach (var node in _nodes)
            {
                node.IsClaimable = node.day <= DayCount;
            }
        }

        public List<Property> Claim(PlayerAction playerAction)
        {
            var result = new List<Property>();
            var lastClaimedDay = 0;
            for (var i = 0; i < _nodes.Count; i++)
            {
                if (_nodes[i].day > LastClaimedDay && _nodes[i].day <= DayCount)
                {
                    _nodes[i].IsClaimed = true;
                    _nodes[i].IsClaimable = false;
                    result.AddRange(_nodes[i].rewards);
                    lastClaimedDay = _nodes[i].day;
                }

                if (_nodes[i].day > DayCount) break;
            }

            if (result.Count > 0)
            {
                result = _propertyManager.Obtain(result, playerAction);
                if(lastClaimedDay > _saveData.lastClaimedDay) _saveData.lastClaimedDay = lastClaimedDay;   
            }
            
            return result;
        }

        public void Dispose()
        {
            _periodicResetHandler.RemoveResetCallback(ResetPeriod.Daily, $"attendance_{_id}");
        }

        public class SaveData
        {
            public int dayCount;
            public int lastClaimedDay;
        }
    }
}
