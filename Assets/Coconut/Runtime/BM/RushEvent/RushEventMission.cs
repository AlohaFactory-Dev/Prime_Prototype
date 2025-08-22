using System.Collections.Generic;
using UniRx;

namespace Aloha.Coconut
{
    public struct RushEventMissionData
    {
        public int objective;
        public List<Property> rewards;
    }
    
    public class RushEventMission
    {
        internal Subject<Unit> OnClaimed { get; } = new Subject<Unit>();
        
        public int Objective { get; }
        public List<Property> Rewards { get; }

        public int Progress
        {
            get => _progress;
            internal set
            {
                _progress = value;
                UpdateRedDot();
            }
        }
        public bool IsClaimed { get; internal set; }
        public string RedDotPath { get; private set; }
        
        private readonly PropertyManager _propertyManager;

        private int _progress;

        internal RushEventMission(RushEventMissionData data, PropertyManager propertyManager)
        {
            _propertyManager = propertyManager;
            Objective = data.objective;
            Progress = 0;
            Rewards = data.rewards;
        }

        public List<Property> Claim(PlayerAction playerAction)
        {
            if (Progress < Objective) return null;

            OnClaimed.OnNext(Unit.Default);
            return _propertyManager.Obtain(Rewards, playerAction);
        }
        
        internal void LinkRedDot(string path)
        {
            RedDotPath = path;
            UpdateRedDot();
        }
        
        internal void UpdateRedDot()
        {
            if (string.IsNullOrEmpty(RedDotPath)) return;
            RedDot.SetNotified(RedDotPath, !IsClaimed && Progress >= Objective);
        }
    }
}
