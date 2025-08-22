using System;
using Aloha.Coconut;
using Cysharp.Threading.Tasks;

public class DailyLimitedRvPrice : IPrice, IDisposable
{
    public bool IsOver => WatchedCount >= DailyLimit;
    public int DailyLimit => _dailyLimit;

    public int WatchedCount
    {
        get => _simpleValues.GetInt($"rv_{_placementId}", 0);
        set => _simpleValues.SetInt($"rv_{_placementId}", value);
    }

    private readonly IRVAdapter _rvAdapter;
    private readonly int _placementId;
    private readonly string _placementName;
    private readonly int _dailyLimit;
    private readonly SimpleValues _simpleValues;
    private readonly PeriodicResetHandler _periodicResetHandler;

    private DailyLimitedRvPrice(IRVAdapter rvAdapter, int placementId, string placementName,
        int dailyLimit, SimpleValues simpleValues, PeriodicResetHandler periodicResetHandler)
    {
        _rvAdapter = rvAdapter;
        _placementId = placementId;
        _placementName = placementName;
        _dailyLimit = dailyLimit;
        _simpleValues = simpleValues;
        _periodicResetHandler = periodicResetHandler;

        _periodicResetHandler.AddResetCallback(ResetPeriod.Daily,
            $"rv_{placementId}", () =>
            {
                WatchedCount = 0;
            });
    }

    public async UniTask<bool> Pay(PlayerAction playerAction)
    {
        if (WatchedCount >= DailyLimit) return false; 
        
        var result = await _rvAdapter.ShowRewardedAdAsync(_placementId, _placementName);
        if(result) WatchedCount++;
        return result;
    }
    
    public string GetPriceString()
    {
        return $"<sprite name=\"Ads\"> {DailyLimit - WatchedCount}/{DailyLimit}";
    }

    public bool IsPayable()
    {
        return !IsOver;
    }

    public void Dispose()
    {
        _periodicResetHandler.RemoveResetCallback(ResetPeriod.Daily, $"rv_{_placementId}");
    }
    
    public class Factory
    {
        private readonly IRVAdapter _rvAdapter;
        private readonly SimpleValues _simpleValues;
        private readonly PeriodicResetHandler _periodicResetHandler;

        public Factory(IRVAdapter rvAdapter, SimpleValues simpleValues, PeriodicResetHandler periodicResetHandler)
        {
            _rvAdapter = rvAdapter;
            _simpleValues = simpleValues;
            _periodicResetHandler = periodicResetHandler;
        }
        
        public DailyLimitedRvPrice Create(int placementId, string placementName, int dailyLimit)
        {
            return new DailyLimitedRvPrice(_rvAdapter, placementId, placementName, dailyLimit, _simpleValues, _periodicResetHandler);
        }
    }
}
