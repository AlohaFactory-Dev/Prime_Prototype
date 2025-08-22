using System.Collections.Generic;
using Aloha.Coconut;
using Zenject;

public class DailyShopProduct : Product
{
    public new DiscountablePrice Price => (DiscountablePrice) base.Price;
    public int Id => _saveData.id;
    public bool IsPurchased => _saveData.isPurchased;
    public decimal DiscountRate => _saveData.discountRate;
    
    private readonly SaveData _saveData;

    public DailyShopProduct(DiscountablePrice price, Property property, PropertyManager propertyManager, SaveData saveData, string nameKey) 
        : base(price, nameKey, new List<Property> {property}, propertyManager)
    {
        _saveData = saveData;
        price.ApplyDiscount(_saveData.discountRate);
    }

    protected override void OnPurchaseResult(PurchaseResult result)
    {
        if (result.isSuccess)
        {
            _saveData.isPurchased = true;
        }
    }

    public new class Factory
    {
        private readonly DiContainer _diContainer;
        private readonly PropertyManager _propertyManager;

        public Factory(DiContainer diContainer, PropertyManager propertyManager)
        {
            _diContainer = diContainer;
            _propertyManager = propertyManager;
        }

        public DailyShopProduct Create(DiscountablePrice price, Property property, SaveData saveData, string nameKey = null)
        {
            return new DailyShopProduct(price, property, _propertyManager, saveData, nameKey);
        }
    }

    public class SaveData
    {
        public int id;
        public bool isPurchased;
        public decimal discountRate = 0;

        public SaveData() { }

        public SaveData(int id, decimal discountRate)
        {
            this.id = id;
            this.discountRate = discountRate;
        }
    }
}