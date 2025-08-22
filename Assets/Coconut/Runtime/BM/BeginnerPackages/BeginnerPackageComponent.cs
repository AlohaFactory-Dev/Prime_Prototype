using System;
using System.Collections.Generic;
using Aloha.Coconut;
using UniRx;

public class BeginnerPackageComponent
{
    public int Day { get; }
    public List<Property> Rewards { get; }
    public bool IsClaimable { get; private set; }
    public bool IsClaimed => _saveData.isClaimed;
    public string RedDotPath { get; private set; }

    internal IObservable<int> OnBeginnerPackageRewardClaimed => _onBeginnerPackageRewardClaimed;
    private Subject<int> _onBeginnerPackageRewardClaimed = new();

    private readonly SaveData _saveData;
    private readonly PropertyManager _propertyManager;

    private BeginnerPackageComponent(int day, List<Property> rewards, SaveData saveData,
        PropertyManager propertyManager)
    {
        Day = day;
        Rewards = rewards;

        _saveData = saveData;
        _propertyManager = propertyManager;
    }

    public List<Property> Claim(PlayerAction action)
    {
        var obtainedRewards = _propertyManager.Obtain(Rewards, action);
        _saveData.isClaimed = true;

        UpdateRedDot();

        _onBeginnerPackageRewardClaimed.OnNext(Day);
        return obtainedRewards;
    }

    internal void UpdateClaimable(GameDate purchasedDate)
    {
        var elapsedTime = Clock.GameDateNow.Date - purchasedDate.Date;
        IsClaimable = elapsedTime.Days >= Day - 1;

        UpdateRedDot();
    }

    public void LinkRedDot(string path)
    {
        RedDotPath = path;
        UpdateRedDot();
    }

    private void UpdateRedDot()
    {
        if (string.IsNullOrEmpty(RedDotPath)) return;
        RedDot.SetNotified(RedDotPath, IsClaimable && !IsClaimed);
    }

    internal class SaveData
    {
        public bool isClaimed;
    }

    internal class Factory
    {
        private readonly PropertyManager _propertyManager;

        public Factory(PropertyManager propertyManager)
        {
            _propertyManager = propertyManager;
        }

        public BeginnerPackageComponent Create(int day, List<Property> rewards, SaveData saveData)
        {
            return new BeginnerPackageComponent(day, rewards, saveData, _propertyManager);
        }
    }
}