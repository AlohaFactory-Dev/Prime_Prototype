using System;
using Aloha.Particle;
using UnityEngine;

public class BasicParticle : RecycleParticle
{
    private Action<RecycleObject> _restoreAction;

    // public override RecycleObjectType recycleObjectType => RecycleObjectType.GetParticle;
    private ParticleSystem _particleSystem;
    public override ParticleType particleType => ParticleType.Basic;

    private void OnParticleSystemStopped()
    {
        _restoreAction.Invoke(this);
    }

    public override void InitializeByFactory(Action<RecycleObject> restoreAction)
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _restoreAction = restoreAction;
        var main = _particleSystem.main;
        main.stopAction = ParticleSystemStopAction.Callback;
        base.InitializeByFactory(restoreAction);
    }

    public override void Play()
    {
        _particleSystem.Play();
    }
}