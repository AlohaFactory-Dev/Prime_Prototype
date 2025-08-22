using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Aloha.Coconut
{
    public class BeginnerPackage
    {
        internal int Id { get; }
        public string NameKey { get; private set; }
        public string LocalizedPriceString { get; private set; }
        public int Efficiency { get; private set; }
        public List<BeginnerPackageComponent> Components { get; } = new();
        public bool IsPurchased => _saveData.isPurchased;

        internal IObservable<int> OnBeginnerPackageRewardClaimed => _onBeginnerPackageRewardClaimed;
        private readonly Subject<int> _onBeginnerPackageRewardClaimed = new();
        
        private readonly CompositeDisposable _disposables = new();

        private readonly SaveData _saveData;
        private readonly IAPProduct _product;

        private BeginnerPackage(IAPProduct iapProduct, BeginnerPackageData packageData,
            SaveData saveData)
        {
            Id = packageData.id;
            NameKey = iapProduct.NameKey;
            Efficiency = iapProduct.Efficiency;

            _product = iapProduct;
            LocalizedPriceString = _product.Price.GetPriceString();

            _saveData = saveData;

            if (IsPurchased)
            {
                InitializeComponents();
            }
        }

        private void InitializeComponents()
        {
            foreach (var component in Components)
            {
                component.UpdateClaimable(_saveData.purchasedDate);
                component.OnBeginnerPackageRewardClaimed
                    .Subscribe(_onBeginnerPackageRewardClaimed.OnNext).AddTo(_disposables);
            }
        }

        internal void UpdateComponentsClaimable()
        {
            foreach (var component in Components)
            {
                component.UpdateClaimable(_saveData.purchasedDate);
            }
        }

        internal void SetPurchased()
        {
            _saveData.isPurchased = true;
            _saveData.purchasedDate = Clock.GameDateNow;
            InitializeComponents();
        }

        public async UniTask<bool> Purchase()
        {
            var result = await _product.Purchase(PlayerAction.UNTRACKED);
            return result.isSuccess;
        }

        internal void OnDispose()
        {
            _disposables.Dispose();
        }

        internal class SaveData
        {
            public bool isPurchased;
            public GameDate purchasedDate;

            public Dictionary<int, BeginnerPackageComponent.SaveData> componentSaveDatas = new();
        }

        internal class Factory
        {
            private readonly IIAPManager _iapManager;
            private readonly BeginnerPackageComponent.Factory _componentFactory;
            private readonly IBeginnerPackageDatabase _database;

            public Factory(IIAPManager iapManager, BeginnerPackageComponent.Factory componentFactory,
                IBeginnerPackageDatabase database)
            {
                _iapManager = iapManager;
                _componentFactory = componentFactory;
                _database = database;
            }

            public BeginnerPackage Create(BeginnerPackageData data, SaveData saveData)
            {
                var newPackage = new BeginnerPackage(_iapManager.GetProduct(data.iapId), data, saveData);

                var componentDataList = data.componentDataList;
                foreach (var componentData in componentDataList)
                {
                    var componentSaveData = saveData.componentSaveDatas.TryGetValue(componentData.day, out var value)
                        ? value
                        : new BeginnerPackageComponent.SaveData();

                    var component = 
                        _componentFactory.Create(componentData.day, componentData.rewards, componentSaveData);

                    component.LinkRedDot($"{_database.GetRedDotPath()}/Package/{data.id}/{componentData.day}");

                    newPackage.Components.Add(component);
                    saveData.componentSaveDatas.TryAdd(componentData.day, componentSaveData);
                }

                return newPackage;
            }
        }
    }
}