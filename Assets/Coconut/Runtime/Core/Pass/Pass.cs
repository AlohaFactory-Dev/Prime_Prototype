using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Aloha.Coconut
{
    public class Pass
    {
        public IReadOnlyList<PassNode> Nodes => _nodes;
        public int CurrentLevel  => (_saveData.exp / _passLevelUnit) + 1;
        public int MaxLevel => _nodes[^1].PassLevel;
        public int MaxExp => (MaxLevel - 1) * _passLevelUnit;
        public int CurrentLevelExp => _saveData.exp % _passLevelUnit;
        public int ExpRequirement => _passLevelUnit;
        public string RedDotPath { get; private set; }
            
        public bool IsAdvancedActivated => _saveData.isAdvancedActivated;
        public bool IsPremiumActivated => _saveData.isPremiumActivated;
        
        public IObservable<Unit> OnExpSet => _onExpSet;
        private readonly Subject<Unit> _onExpSet = new();
        
        public IObservable<Unit> OnPassUpdated => _onPassUpdated;
        private readonly Subject<Unit> _onPassUpdated = new();
        
        private readonly int _passLevelUnit;
        private readonly List<PassNode> _nodes;
        private readonly SaveData _saveData;
        private readonly PropertyManager _propertyManager;

        private Pass(int passLevelUnit, List<PassNode> nodes, SaveData saveData, PropertyManager propertyManager)
        {
            _passLevelUnit = passLevelUnit;
            _nodes = nodes;
            _nodes.Sort((a, b) => a.PassLevel.CompareTo(b.PassLevel));
            _saveData = saveData;
            _propertyManager = propertyManager;
            
            RefreshNodes();
        }

        private void RefreshNodes()
        {
            foreach (var node in _nodes)
            {
                node.IsAdvancedActivated = _saveData.isAdvancedActivated;
                node.IsPremiumActivated = _saveData.isPremiumActivated;
                
                if (CurrentLevel < node.PassLevel)
                {
                    node.FreeRewardState = PassNode.State.Locked;
                    node.AdvancedRewardState = PassNode.State.Locked;
                    node.PremiumRewardState = PassNode.State.Locked;
                }
                else
                {
                    node.FreeRewardState = node.PassLevel > _saveData.claimedFreeRewardLevel ? PassNode.State.Claimable : PassNode.State.Claimed;
                    node.AdvancedRewardState = node.PassLevel > _saveData.claimedAdvancedRewardLevel ? PassNode.State.Claimable : PassNode.State.Claimed;
                    node.PremiumRewardState = node.PassLevel > _saveData.claimedPremiumRewardLevel ? PassNode.State.Claimable : PassNode.State.Claimed;
                }
            }

            if (!string.IsNullOrEmpty(RedDotPath))
            {
                UpdateRedDot();
            }
            
            _onPassUpdated.OnNext(Unit.Default);
        }
        
        public List<Property> ClaimFreeRewards(PlayerAction playerAction)
        {
            return ClaimRewards(CurrentLevel, ref _saveData.claimedFreeRewardLevel, n => n.FreeReward, playerAction);
        }
        
        public List<Property> ClaimFreeRewards(PassNode node, PlayerAction playerAction)
        {
            return ClaimRewards(node.PassLevel,ref _saveData.claimedFreeRewardLevel, n => n.FreeReward, playerAction);
        }

        private List<Property> ClaimRewards(int maxLevel, ref int savedLevel, Func<PassNode, Property> rewardGetter, PlayerAction playerAction)
        {
            var rewards = new List<Property>();
            for (var i = 0; i < _nodes.Count; i++)
            {
                if(_nodes[i].PassLevel <= savedLevel) continue;
                if(_nodes[i].PassLevel > maxLevel) break;
                
                rewards.Add(rewardGetter(_nodes[i]));
            }
            rewards = _propertyManager.Obtain(rewards, playerAction);
            savedLevel = maxLevel;
            
            RefreshNodes();
            return rewards;
        }
        
        public List<Property> ClaimAdvancedRewards(PlayerAction playerAction)
        {
            if(!_saveData.isAdvancedActivated) return new List<Property>();
            return ClaimRewards(CurrentLevel, ref _saveData.claimedAdvancedRewardLevel, n => n.AdvancedReward, playerAction);
        }

        public List<Property> ClaimAdvancedRewards(PassNode passNode, PlayerAction playerAction)
        {
            if(!_saveData.isAdvancedActivated) return new List<Property>();
            return ClaimRewards(passNode.PassLevel, ref _saveData.claimedAdvancedRewardLevel, n => n.AdvancedReward, playerAction);
        }
        
        public List<Property> ClaimPremiumRewards(PlayerAction playerAction)
        {
            if(!_saveData.isPremiumActivated) return new List<Property>();
            return ClaimRewards(CurrentLevel, ref _saveData.claimedPremiumRewardLevel, n => n.PremiumReward, playerAction);
        }

        public List<Property> ClaimPremiumRewards(PassNode passNode, PlayerAction playerAction)
        {
            if(!_saveData.isPremiumActivated) return new List<Property>();
            return ClaimRewards(passNode.PassLevel, ref _saveData.claimedPremiumRewardLevel, n => n.PremiumReward, playerAction);
        }
        
        public void ActivateAdvanced()
        {
            _saveData.isAdvancedActivated = true;
            RefreshNodes();
        }
        
        public void ActivatePremium()
        {
            _saveData.isPremiumActivated = true;
            RefreshNodes();
        }
        
        public void SetLevel(int level)
        {
            SetExp((level - 1) * _passLevelUnit);
        }

        public void AddExp(int exp)
        {
            SetExp(_saveData.exp + exp);
        }

        public void SetExp(int exp)
        {
            var lastLevel = CurrentLevel;
            _saveData.exp = exp;
            if (_saveData.exp >= MaxExp) _saveData.exp = MaxExp;
            _onExpSet.OnNext(Unit.Default);
            if (CurrentLevel != lastLevel) RefreshNodes();
        }

        public void LinkRedDot(string path)
        {
            RedDotPath = path;

            foreach (var node in _nodes)
            {
                node.LinkRedDot($"{path}/{node.PassLevel}");
            }

            UpdateRedDot();
        }

        private void UpdateRedDot()
        {
            foreach (var node in _nodes)
            {
                node.UpdateRedDot();
            }
        }

        public class SaveData
        {
            public int exp;
            public int claimedFreeRewardLevel;
            public int claimedAdvancedRewardLevel;
            public int claimedPremiumRewardLevel;
            
            public bool isAdvancedActivated;
            public bool isPremiumActivated;

            public void Reset(bool startFromLevelZero)
            {
                exp = startFromLevelZero ? -1 : 0; // exp 0을 lv 1로 처리하는 게 기본, lv을 0으로 하기 위해선 exp를 -1로 해야 함
                claimedFreeRewardLevel = 0;
                claimedAdvancedRewardLevel = 0;
                claimedPremiumRewardLevel = 0;
                isAdvancedActivated = false;
                isPremiumActivated = false;
            }
        }

        public class Factory
        {
            private readonly PropertyManager _propertyManager;
            
            public Factory(PropertyManager propertyManager)
            {
                _propertyManager = propertyManager;
            }
            
            public Pass Create(int passLevelUnit, List<PassNode> nodes, SaveData saveData)
            {
                return new Pass(passLevelUnit, nodes, saveData, _propertyManager);
            }
        }
    }
}
