using System;
using System.Collections.Generic;
using System.Numerics;
using Aloha.Coconut.IAP;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace Aloha.Coconut
{
    public class BeginnerPackagesManager : IPropertyHandler, IDisposable
    {
        public ReactiveProperty<bool> IsActive { get; } = new(false);
        public List<BeginnerPackage> BeginnerPackageList { get; } = new();
        public string RedDotPath => _beginnerPackageDatabase.GetRedDotPath();

        private readonly SaveData _saveData;
        private readonly IBeginnerPackageDatabase _beginnerPackageDatabase;
        private readonly PeriodicResetHandler _periodicResetHandler;
        private readonly IIAPManager _iapManager;

        private readonly CompositeDisposable _disposables = new();

        public BeginnerPackagesManager(SaveDataManager saveDataManager, PeriodicResetHandler periodicResetHandler,
            IIAPManager iapManager, IBeginnerPackageDatabase beginnerPackageDatabase)
        {
            _saveData = saveDataManager.Get<SaveData>("beginner_packages_manager");
            _beginnerPackageDatabase = beginnerPackageDatabase;
            _periodicResetHandler = periodicResetHandler;
            _iapManager = iapManager;

            HandlingGroups = new List<PropertyTypeGroup> { _beginnerPackageDatabase.GetBeginnerPackageTypeGroup() };
        }

        // Factory class들을 internal로 유지하기 위한 internal Inject method
        [Inject]
        internal void Inject(BeginnerPackage.Factory beginnerPackageFactory)
        {
            _iapManager.AddOnInitializedListener(result =>
            {
                if (!result) 
                {
                    Debug.LogError("BeginnerPackagesManager :: IAPManager is not initialized");
                    return;
                }
                
                var packageDataList = _beginnerPackageDatabase.GetBeginnerPackageDataList();
                bool isAllPackagesClaimed = true;
                foreach (var packageData in packageDataList)
                {
                    var saveData = _saveData.packageSaveDatas.TryGetValue(packageData.id, out var value)
                        ? value
                        : new BeginnerPackage.SaveData();
                    _saveData.packageSaveDatas.TryAdd(packageData.id, saveData);

                    var beginnerPackage = beginnerPackageFactory.Create(packageData, saveData);
                    beginnerPackage.OnBeginnerPackageRewardClaimed.Subscribe(_ =>
                    {
                        bool isAllClaimed = beginnerPackage.Components.TrueForAll(component => component.IsClaimed);
                        IsActive.Value = !isAllClaimed;
                    }).AddTo(_disposables);
                
                    BeginnerPackageList.Add(beginnerPackage);

                    bool allComponentsClaimed = beginnerPackage.Components.TrueForAll(component => component.IsClaimed);
                    if (!allComponentsClaimed) isAllPackagesClaimed = false;
                }

                IsActive.Value = !isAllPackagesClaimed;
                UpdatePackageComponents();

                _periodicResetHandler.AddResetCallback(ResetPeriod.Daily, "beginner_packages_manager",
                    UpdatePackageComponents);
            });
        }

        private void UpdatePackageComponents()
        {
            foreach (var beginnerPackage in BeginnerPackageList)
            {
                if (beginnerPackage.IsPurchased == false) continue;
                beginnerPackage.UpdateComponentsClaimable();
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
            foreach (var beginnerPackage in BeginnerPackageList)
            {
                beginnerPackage.OnDispose();
            }
            
            _periodicResetHandler.RemoveResetCallback(ResetPeriod.Daily, "beginner_packages_manager");
        }

        private class SaveData
        {
            public Dictionary<int, BeginnerPackage.SaveData> packageSaveDatas = new();
        }

        #region IPropertyHandler
        public List<PropertyTypeGroup> HandlingGroups { get; }

        void IPropertyHandler.Obtain(Property property)
        {
            foreach (BeginnerPackage beginnerPackage in BeginnerPackageList)
            {
                if (beginnerPackage.Id == property.type.id)
                {
                    beginnerPackage.SetPurchased();
                    break;
                }
            }
        }

        void IPropertyHandler.Use(Property property)
        {
            throw new NotImplementedException();
        }

        void IPropertyHandler.Set(Property property)
        {
            throw new NotImplementedException();
        }

        BigInteger IPropertyHandler.GetBalance(PropertyType property)
        {
            return 0;
        }
        # endregion
    }
}