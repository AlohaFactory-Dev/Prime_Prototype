using System.Collections.Generic;

namespace Aloha.Coconut
{
    public class NewUserPackageGroup
    {
        public int Day { get; private set; }
        public List<LimitedProduct> ProductList { get; private set; }

        private NewUserPackageGroup(int day, List<LimitedProduct> productList)
        {
            Day = day;
            ProductList = productList;
        }

        internal class Factory
        {
            private readonly INewUserEventDatabase _newUserEventDatabase;
            private readonly LimitedProduct.Factory _limitedProductFactory;

            public Factory(INewUserEventDatabase newUserEventDatabase, LimitedProduct.Factory limitedProductFactory)
            {
                _newUserEventDatabase = newUserEventDatabase;
                _limitedProductFactory = limitedProductFactory;
            }

            public NewUserPackageGroup Create(NewUserPackageGroupData data, SaveData saveData)
            {
                var productList = new List<LimitedProduct>();
                foreach (var packageData in data.packageDataList)
                {
                    var packageId = packageData.iapId;
                    var productSaveData = saveData.productSaveDatas.TryGetValue(packageId, out var value)
                        ? value
                        : new LimitedProduct.SaveData();
                    saveData.productSaveDatas.TryAdd(packageId, productSaveData);

                    LimitedProduct limitedProduct;
                    if (packageData.isFree)
                    {
                        limitedProduct = _limitedProductFactory.CreateFreeProduct(packageId, packageData.limit, productSaveData);
                        limitedProduct.LinkRedDot($"{_newUserEventDatabase.GetRedDotPath()}/Package/{data.day}/{packageId}");
                    }
                    else
                    {
                        limitedProduct = _limitedProductFactory.CreateIAPProduct(packageId, packageData.limit, productSaveData);
                    }
                    
                    productList.Add(limitedProduct);
                }

                return new NewUserPackageGroup(data.day, productList);
            }
        }

        internal class SaveData
        {
            public Dictionary<string, LimitedProduct.SaveData> productSaveDatas = new();
        }
    }
}