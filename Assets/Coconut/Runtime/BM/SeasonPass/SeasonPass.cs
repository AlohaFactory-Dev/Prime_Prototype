using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UniRx;
using UnityEngine.Assertions;
using Zenject;

namespace Aloha.Coconut
{
    public class SeasonPass : IDisposable, IPropertyHandler
    {
        public IObservable<Unit> OnSeasonChanged => _onSeasonChanged;
        private readonly Subject<Unit> _onSeasonChanged = new();

        public GameDate EndDate { get; private set; }
        public Pass CurrentPass { get; private set; }
        public string RedDotPath => $"{_database.GetRedDotPath()}/{_saveData.passId}";

        private readonly ISeasonPassDatabase _database;
        private readonly Pass.Factory _passFactory;
        private readonly SaveData _saveData;

        private readonly CompositeDisposable _compositeDisposable = new();
        private IDisposable _currentPassDisposable;

        public SeasonPass([InjectOptional] ISeasonPassDatabase database, SaveDataManager saveDataManager,
            Pass.Factory passFactory, ISeasonPassExpAdder expAdder)
        {
            _database = database ?? new SeasonPassDatabase();
            _passFactory = passFactory;

            HandlingGroups = new List<PropertyTypeGroup> { _database.GetSeasonPassTypeGroup() };
            _saveData = saveDataManager.Get<SaveData>("SeasonPass");

            CreateSeasonPass();
            Clock.OnGameDatePassed.Subscribe(_ =>
            {
                if (Clock.GameDateNow > EndDate) CreateSeasonPass();
            }).AddTo(_compositeDisposable);

            expAdder.OnGetSeasonPassExp
                .Subscribe(exp => CurrentPass.AddExp(exp))
                .AddTo(_compositeDisposable);
        }

        private void CreateSeasonPass()
        {
            _currentPassDisposable?.Dispose();

            var currentSeasonPassData = _database.GetCurrentSeasonPassData(Clock.GameDateNow);
            if (currentSeasonPassData == null)
            {
                _onSeasonChanged.OnNext(Unit.Default);
                return;
            }

            if (_saveData.passId != currentSeasonPassData.id)
            {
                _saveData.passId = currentSeasonPassData.id;
                _saveData.passSaveData = new Pass.SaveData();
            }

            EndDate = new GameDate(currentSeasonPassData.endDate);
            var nodes = currentSeasonPassData.nodeDatas.Select(nodeData => new PassNode(nodeData)).ToList();
            CurrentPass = _passFactory.Create(currentSeasonPassData.expPerLevel, nodes, _saveData.passSaveData);
            CurrentPass.LinkRedDot(RedDotPath);

            _onSeasonChanged.OnNext(Unit.Default);
        }

        public List<Property> ClaimFreeRewards(PassNode passNode, PlayerAction playerAction)
        {
            var result = CurrentPass.ClaimFreeRewards(passNode, playerAction);
            return result;
        }

        public List<Property> ClaimAdvancedRewards(PassNode passNode, PlayerAction playerAction)
        {
            var result = CurrentPass.ClaimAdvancedRewards(passNode, playerAction);
            return result;
        }

        public List<Property> ClaimPremiumRewards(PassNode passNode, PlayerAction playerAction)
        {
            var result = CurrentPass.ClaimPremiumRewards(passNode, playerAction);
            return result;
        }

        public List<Property> ClaimAll(PlayerAction freePlayerAction, PlayerAction premiumPlayerAction)
        {
            var result = CurrentPass.ClaimFreeRewards(freePlayerAction);
            if (CurrentPass.IsAdvancedActivated) result.AddRange(CurrentPass.ClaimAdvancedRewards(premiumPlayerAction));
            if (CurrentPass.IsPremiumActivated) result.AddRange(CurrentPass.ClaimPremiumRewards(premiumPlayerAction));
            return result;
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
            _currentPassDisposable?.Dispose();
        }

        private class SaveData
        {
            public int passId;
            public Pass.SaveData passSaveData;
        }

        #region IPropertyHandler

        public List<PropertyTypeGroup> HandlingGroups { get; }

        void IPropertyHandler.Obtain(Property property)
        {
            if (property.type.id == 1) CurrentPass.ActivateAdvanced();
            else CurrentPass.ActivatePremium();
        }

        void IPropertyHandler.Use(Property property) { }

        void IPropertyHandler.Set(Property property) { }

        BigInteger IPropertyHandler.GetBalance(PropertyType property)
        {
            return 0;
        }

        #endregion
    }
}