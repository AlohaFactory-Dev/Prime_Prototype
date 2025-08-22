using System;
using Aloha.Coconut;
using Cysharp.Threading.Tasks;
using UniRx;

public class CooldownRvPrice : IPrice, IDisposable
{
    public bool IsDuringCooldown => _simpleValues.HaveDateTime(Key);
    public TimeSpan CooldownLeft => _simpleValues.GetDateTime(Key, Clock.Now) + TimeSpan.FromHours(_cooldownHours) - Clock.Now;
    public IObservable<Unit> OnCooldownEnd => _onCooldownEnd;
    
    private Subject<Unit> _onCooldownEnd = new Subject<Unit>();

    private string Key => $"rv_{_placementId}_shown";

    private readonly IRVAdapter _rvAdapter;
    private readonly int _placementId;
    private readonly string _placementName;
    private readonly int _cooldownHours;
    private readonly SimpleValues _simpleValues;

    private IDisposable _cooldownSubscription;

    public CooldownRvPrice(IRVAdapter rvAdapter, int placementId, string placementName, int cooldownHours, SimpleValues simpleValues)
    {
        _rvAdapter = rvAdapter;
        _placementId = placementId;
        _placementName = placementName;
        _cooldownHours = cooldownHours;
        _simpleValues = simpleValues;

        _cooldownSubscription = Clock.OnMinuteTick.Subscribe(_ =>
        {
            if (IsDuringCooldown && CooldownLeft <= TimeSpan.Zero)
            {
                _simpleValues.DeleteDateTime(Key);
                _onCooldownEnd.OnNext(Unit.Default);
            }
        });
    }

    public async UniTask<bool> Pay(PlayerAction playerAction)
    {
        if (IsDuringCooldown) return false; 
        
        var result = await _rvAdapter.ShowRewardedAdAsync(_placementId, _placementName);
        if (result)
        {
            _simpleValues.SetDateTime(Key, Clock.Now);
        }
        return result;
    }
    
    public string GetPriceString()
    {
        return $"<sprite name=\"Ads\">";
    }

    public bool IsPayable()
    {
        return !IsDuringCooldown;
    }

    public void Dispose()
    {
        _cooldownSubscription?.Dispose();
    }
}
