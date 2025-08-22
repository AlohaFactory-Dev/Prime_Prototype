using System;
using System.Collections.Generic;
using UniRx;

namespace Aloha.Coconut
{
    public class GrowthLevel : IDisposable
    {
        public struct TableEntry
        {
            [CSVColumn] public int level;
            [CSVColumn] public int req_exp;
        }
        
        public int MaxLevel => _requiredExp.Count;
        
        // 레벨은 1부터 시작
        public int Level => _saveData.level;
        public int CurrentExp => (int)_propertyManager.GetBalance(_expType);
        public int ExpRequirement => IsLevelMax ? 0 : _requiredExp[Level];
        public bool IsLevelMax => Level >= _requiredExp.Count;
        public int CumulatedExp => _saveData.cumulatedExp;

        private readonly PropertyManager _propertyManager;
        private readonly PropertyType _expType;
        private readonly List<int> _requiredExp;

        private SaveData _saveData;
        private IDisposable _propertySubscription;
        
        public IObservable<int> OnLevelUp => _onLevelUp;
        private readonly Subject<int> _onLevelUp = new Subject<int>();

        private GrowthLevel(PropertyManager propertyManager, PropertyType expType, List<int> requiredExp, SaveData saveData)
        {
            _propertyManager = propertyManager;
            _expType = expType;
            
            // 데이터는 1레벨부터 받지만, 코드 가독성을 위해 0레벨을 요구량 0으로 추가
            _requiredExp = new List<int>(requiredExp);
            _requiredExp.Insert(0, 0);

            _saveData = saveData;
            
            _propertySubscription = propertyManager.OnPropertyUpdated.Where(p => p.type == _expType)
                .Where(ev => ev.variance > 0)
                .Subscribe(ev =>
                {
                    _saveData.cumulatedExp += (int)ev.variance;

                    var usedExp = 0;
                    while (!IsLevelMax && ev.balance >= _requiredExp[Level])
                    {
                        usedExp += _requiredExp[Level];
                        ev.balance -= _requiredExp[Level];
                        _saveData.level++;
                        _onLevelUp.OnNext(Level);
                    }

                    if (usedExp > 0)
                    {
                        _propertyManager.Use(new Property(ev.type, usedExp), PlayerAction.UNTRACKED);   
                    }
                });
        }

        public void Dispose()
        {
            _propertySubscription?.Dispose();
        }

        public class SaveData
        {
            public int level = 1;
            public int cumulatedExp;
        }

        public class Factory
        {
            private readonly PropertyManager _propertyManager;

            public Factory(PropertyManager propertyManager)
            {
                _propertyManager = propertyManager;
            }

            public GrowthLevel Create(PropertyType expType, List<int> requiredExp, SaveData saveData = null)
            {
                if(saveData == null) saveData = new SaveData();
                return new GrowthLevel(_propertyManager, expType, requiredExp, saveData);
            }
        }
    }
}
