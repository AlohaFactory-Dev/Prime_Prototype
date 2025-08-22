using System;
using System.Collections.Generic;
using System.Numerics;
using UniRx;

namespace Aloha.Coconut.Missions
{
    public abstract class Mission
    {
        public int Id => _missionData.id;
        public BigInteger Objective => _missionData.objective;
        public List<Property> Rewards => _missionData.rewards;
        public IReadOnlyReactiveProperty<BigInteger> Progress => _saveData.progress;
        public bool IsRewardsClaimed => _saveData.isRewardsClaimed;
        public bool IsCompleted => Progress.Value >= Objective;
        public bool IsRewardsClaimable => IsCompleted && !IsRewardsClaimed;
        public string RedDotPath { get; private set; }
        public IObservable<Unit> OnClaimed => _onClaimed;

        private bool _isDisposed;
        private readonly PropertyManager _propertyManager;
        private readonly SaveData _saveData;
        private readonly MissionData _missionData;
        private readonly string _descriptionKey;
        private readonly Subject<Unit> _onClaimed = new();

        protected Mission(MissionData missionData, PropertyManager propertyManager, SaveData saveData = null)
        {
            _missionData = missionData;
            _propertyManager = propertyManager;
            _saveData = saveData ?? new SaveData();
            _descriptionKey = MissionTypeData.GetDescriptionKey(_missionData.type);
        }

        public virtual void Start() { }

        public virtual string GetDescription()
        {
            return TextTableV2.Get(_descriptionKey, 
                new TextTableV2.Param("obj", Objective.ToString()), 
                new TextTableV2.Param("var", _missionData.var.ToString()));
        }

        public virtual string GetProgressString()
        {
            return $"{Progress.Value}/{Objective}";
        }

        protected void AddProgress(BigInteger progress)
        {
            SetProgress(_saveData.progress.Value + progress);
        }

        protected void SetProgress(BigInteger progress)
        {
            _saveData.progress.Value = progress > Objective ? Objective : progress;
            UpdateRedDot();
        }

        public List<Property> Claim(PlayerAction playerAction)
        {
            List<Property> result = _propertyManager.Obtain(Rewards, playerAction);
            _saveData.isRewardsClaimed = true;
            _onClaimed.OnNext(Unit.Default);
            Dispose();
            
            return result;
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;
            _saveData.progress.Dispose();
            _onClaimed.Dispose();
            if (!string.IsNullOrEmpty(RedDotPath)) RedDot.SetNotified(RedDotPath, false);
            
            OnDispose();
        }

        protected virtual void OnDispose() { }
        
        public T GetExtendedData<T>() where T : IExtendedMissionData
        {
            return _missionData.GetExtendedData<T>();
        }
        
        public void LinkRedDot(string path)
        {
            RedDotPath = path;
            UpdateRedDot();
        }
        
        private void UpdateRedDot()
        {
            if (string.IsNullOrEmpty(RedDotPath)) return;
            RedDot.SetNotified(RedDotPath, IsRewardsClaimable);
        }

        public class SaveData
        {
            public bool isRewardsClaimed;
            public ReactiveProperty<BigInteger> progress = new();
        }
    }
}