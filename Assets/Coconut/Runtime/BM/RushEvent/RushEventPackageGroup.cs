using System.Collections.Generic;

namespace Aloha.Coconut
{
    public struct RushEventPackageGroupData
    {
        public List<RushEventPackageData> packageDatas;
    }

    public struct RushEventPackageData
    {
        public int id;
        public string packageId;
        public bool isFree;
        public int limit;
    }
    
    public class RushEventPackageGroup
    {
        public List<LimitedProduct> LimitedProducts { get; }
        private readonly SaveData _saveData;

        private RushEventPackageGroup(List<LimitedProduct> limitedProducts, SaveData saveData)
        {
            _saveData = saveData;
            LimitedProducts = limitedProducts;
        }
        
        public class SaveData
        {
            public Dictionary<int, LimitedProduct.SaveData> productSaveDatas;
        }
        
        internal class Factory
        {
            private readonly LimitedProduct.Factory _limitedProductFactory;
            private readonly IRushEventDatabase _rushEventDatabase;

            public Factory(LimitedProduct.Factory limitedProductFactory, IRushEventDatabase rushEventDatabase)
            {
                _limitedProductFactory = limitedProductFactory;
                _rushEventDatabase = rushEventDatabase;
            }

            public RushEventPackageGroup Create(RushEventPackageGroupData data, SaveData saveData)
            {
                List<LimitedProduct> products = new List<LimitedProduct>();
                foreach (var packageData in data.packageDatas)
                {
                    saveData.productSaveDatas ??= new Dictionary<int, LimitedProduct.SaveData>();
                    if (!saveData.productSaveDatas.ContainsKey(packageData.id))
                    {
                        saveData.productSaveDatas[packageData.id] = new LimitedProduct.SaveData();
                    }

                    if (packageData.isFree)
                    {
                        var product = _limitedProductFactory.CreateFreeProduct(packageData.packageId, 1,
                            saveData.productSaveDatas[packageData.id]);
                        product.LinkRedDot($"{_rushEventDatabase.GetRedDotPath()}/Package/{packageData.id}");
                        products.Add(product);
                    }
                    else
                    {
                        products.Add(_limitedProductFactory.CreateIAPProduct(packageData.packageId, packageData.limit, saveData.productSaveDatas[packageData.id]));
                    }
                }
                
                return new RushEventPackageGroup(products, saveData);
            }
        }
    }
}
