using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine.Assertions;

namespace Aloha.Coconut
{
    public class SpecialPackageManager: IDisposable
    {
        public IReadOnlyList<SpecialPackage> SpecialPackages => _specialPackages;
        
        private readonly LimitedProduct.Factory _limitedProductFactory;
        private readonly List<SpecialPackage> _specialPackages = new List<SpecialPackage>();
        private readonly SaveData _saveData;
        
        private readonly List<SpecialPackageData> _specialPackageDatas;

        public SpecialPackageManager(SaveDataManager saveDataManager, ISpecialPackageDatabase specialPackageDatabase, LimitedProduct.Factory limitedProductFactory)
        {
            _saveData = saveDataManager.Get<SaveData>("specialPackage");
            _limitedProductFactory = limitedProductFactory;
            _specialPackageDatas = specialPackageDatabase.GetSpecialPackageDatas();
            
            // 기본 스페셜 패키지 활성화
            foreach (SpecialPackageData data in _specialPackageDatas)
            {
                if (data.condition != 0) continue;
                AddPackage(data);
            }

            // 릴레이 스페셜 패키지 활성화
            foreach (SpecialPackageData data in _specialPackageDatas)
            {
                if (data.condition == 0) continue;
                
                Assert.IsTrue(_specialPackages.Exists(package => package.Id == data.condition));
                SpecialPackage conditionPackage = _specialPackages.Find(package => package.Id == data.condition);
                if (conditionPackage.IsSoldOut)
                {
                    AddPackage(data);
                }
            }
        }

        private void AddPackage(SpecialPackageData data)
        {
            if (!_saveData.productSaveDatas.ContainsKey(data.id))
            {
                _saveData.productSaveDatas.Add(data.id, new LimitedProduct.SaveData());
            }
            
            LimitedProduct product = _limitedProductFactory.CreateIAPProduct(data.iapProductId, data.limit, _saveData.productSaveDatas[data.id]);
            SpecialPackage specialPackage = new SpecialPackage(data.id, product);
            _specialPackages.Add(specialPackage);

            specialPackage.OnPurchased
                .Subscribe(_ => OnPackagePurchased(specialPackage));
        }

        private void OnPackagePurchased(SpecialPackage specialPackage)
        {
            if (!specialPackage.IsSoldOut) return;
            
            foreach (SpecialPackageData data in _specialPackageDatas)
            {
                if (data.condition == 0) continue;
                if (data.condition == specialPackage.Id)
                {
                    AddPackage(data);   
                }
            }
        }

        public void Dispose()
        {
            foreach (SpecialPackage specialPackage in _specialPackages)
            {
                specialPackage.LimitedProduct.Dispose();
            }
        }

        private class SaveData
        {
            public Dictionary<int, LimitedProduct.SaveData> productSaveDatas = new Dictionary<int, LimitedProduct.SaveData>();
        }
    }
}
