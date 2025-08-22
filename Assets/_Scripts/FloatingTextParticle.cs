using System;
using Aloha.Particle;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class FloatingTextParticle : RecycleParticle
{
    private readonly string show = "show";
    private Animator _animator;
    private Action<RecycleObject> _restoreAction;
    private TextMeshPro textMesh;
    [SerializeField] private float randomYMinRange = 0f;
    [SerializeField] private float randomYMaxRange = 2f;
    [SerializeField] private float randomXMinRange = -0.75f;
    [SerializeField] private float randomXMaxRange = 0.75f;
    public override ParticleType particleType => ParticleType.Floating;

    public override void Play()
    {
        _animator.SetTrigger(show);
    }

    public void Restore_Event()
    {
        _restoreAction(this);
    }

    public override void InitializeByFactory(Action<RecycleObject> releaseAction)
    {
        _restoreAction = releaseAction;
        textMesh = GetComponentInChildren<TextMeshPro>();
        _animator = GetComponent<Animator>();
    }

    public void SetText(string text)
    {
        textMesh.text = text;
    }


    public void Play(Vector2 position)
    {
        position.y += Random.Range(randomYMinRange, randomYMaxRange);
        position.x += Random.Range(randomXMinRange, randomXMaxRange);
        transform.position = position;
        Play();
    }
}