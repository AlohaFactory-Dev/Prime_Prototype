using System;
using System.Collections.Generic;
using System.Linq;
using Aloha.Coconut;
using UniRx;
using Random = UnityEngine.Random;

public class DailyShop : IDisposable
{
    public Product FreeGemProduct => _freeGem;
    public List<DailyShopProduct> LoadedProducts => _loadedProducts;
    public TimeSpan TimeToReset => _periodicResetHandler.GetRemainingTime(ResetPeriod.Daily);
    
    public IObservable<Unit> OnReset => _onReset;
    private Subject<Unit> _onReset = new();

    private readonly SaveData _saveData;
    private readonly PeriodicResetHandler _periodicResetHandler;
    private readonly IDailyShopEntryProvider _dailyShopEntryProvider;
    private readonly DailyShopProduct.Factory _dailyShopProductFactory;
    private readonly DiscountablePrice.Factory _discountablePriceFactory;

    private bool _wasResetOnce;

    private readonly List<DailyShopProduct> _loadedProducts = new();
    private readonly Product _freeGem;

    public DailyShop(SaveDataManager saveDataManager, PeriodicResetHandler periodicResetHandler,
        IDailyShopEntryProvider dailyShopEntryProvider, 
         DiscountablePrice.Factory discountablePriceFactory, DailyShopProduct.Factory dailyShopProductFactory, 
        DailyLimitedRvPrice.Factory dailyLimitedRvPriceFactory, Product.Factory propertyProductFactory)
    {
        _periodicResetHandler = periodicResetHandler;
        _dailyShopEntryProvider = dailyShopEntryProvider;
        _dailyShopProductFactory = dailyShopProductFactory;
        _discountablePriceFactory = discountablePriceFactory;
        _saveData = saveDataManager.Get<SaveData>("daily_shop");

        var freeGemPrice = dailyLimitedRvPriceFactory.Create(3004, "free_gem", 2);
        _freeGem = propertyProductFactory.Create(freeGemPrice, new Property("Gem", 100));
        
        _periodicResetHandler.AddResetCallback(ResetPeriod.Daily, "daily_shop", OnDailyReset);
        
        // 첫 PeriodicResetEventHandler 등록 과정에서 상품이 로드되지 않았다면, 세이브데이터로부터 로드
        if (!_wasResetOnce)
        {
            var entries = _dailyShopEntryProvider.GetDailyShopRawEntries();
            foreach (var productSaveData in _saveData.loadedProducts)
            {
                var rawEntry = entries.FirstOrDefault(e => e.id == productSaveData.id);
                if (rawEntry.id == 0) rawEntry = entries[0];
                var price = _discountablePriceFactory.Create(new Property(rawEntry.priceType, rawEntry.priceAmount));
                var product = dailyShopProductFactory.Create(price, new Property(rawEntry.goodsType, rawEntry.goodsAmount), productSaveData);
                _loadedProducts.Add(product);
            }
        }
    }

    private void OnDailyReset()
    {
        _loadedProducts.Clear();
        _saveData.loadedProducts.Clear();

        var entries = _dailyShopEntryProvider.GetDailyShopRawEntries();
        var selectedEntries = new List<DailyShopEntry>();
        var weightSum = entries.Sum(entry => entry.weight);
        // get 5 entries
        for (var i = 0; i < 5; i++)
        {
            var random = Random.Range(0, weightSum);
            var sum = 0;
            for (var j = 0; j < entries.Count; j++)
            {
                var entry = entries[j];
                sum += entry.weight;
                if (sum >= random)
                {
                    selectedEntries.Add(entry);
                    weightSum -= entry.weight;
                    entries.RemoveAt(j);
                    break;
                }
            }
        }
        
        foreach (var entry in selectedEntries)
        {
            var newSaveData = new DailyShopProduct.SaveData (entry.id, GetRandomDiscountRate());
            var price = _discountablePriceFactory.Create(new Property(entry.priceType, entry.priceAmount));
            var product = _dailyShopProductFactory.Create(price, new Property(entry.goodsType, entry.goodsAmount), newSaveData);
            _loadedProducts.Add(product);
            _saveData.loadedProducts.Add(newSaveData);
        }

        _wasResetOnce = true;
        _onReset.OnNext(Unit.Default);
    }

    private decimal GetRandomDiscountRate()
    {
        var rand = Random.Range(0, 100);
        if (rand <= 40) return 0;
        if (rand <= 60) return 0.1m;
        if (rand <= 80) return 0.2m;
        if (rand <= 90) return 0.3m;
        return 0.5m;
    }

    public void Dispose()
    {
        _periodicResetHandler.RemoveResetCallback(ResetPeriod.Daily, "daily_shop");
    }

    private class SaveData
    {
        public readonly List<DailyShopProduct.SaveData> loadedProducts = new();
    }
}
