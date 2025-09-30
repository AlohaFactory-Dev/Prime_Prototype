using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public enum FireType
{
    Normal,
    Howitzer
}

public enum ProjectileSpeedType
{
    Speed, //속도
    Time //시간
}

[RequireComponent(typeof(Rigidbody2D), typeof(RecycleObject))]
public class Projectile : MonoBehaviour
{
    [InfoBox("이 필드는 곡사(FireType.Howitzer)일 때만 사용됩니다.")]
    [BoxGroup("Howitzer Settings")]
    [SerializeField]
    private AnimationCurve howitzerCurve;

    [BoxGroup("Howitzer Settings")]
    [SerializeField]
    private float howitzerHeight;

    [BoxGroup("Normal")] [SerializeField] private AnimationCurve normalCurve;

    [SerializeField] private Transform mainObject;
    private RecycleObject _recycleObject;
    private ProjectileTable _table;
    private Action _onHit;
    private Sequence _sequence;
    private FireType FireType => _table.fireType;
    private TrailRenderer _trailRenderer;
    private ParticleSystem _particleSystem;
    private Vector3 _previousPosition;
    private bool _isInitialized;
    private float _releaseDelay = 3f; // 자동 해제 딜레이 타임

    public void Init(Vector2 position, Action onHit, ProjectileTable table, Vector2 targetPosition)
    {
        GetComponents();
        transform.position = position; 

        if (_trailRenderer)
        {
            _trailRenderer.Clear();
            _trailRenderer.enabled = false;
            _trailRenderer.enabled = true;
        }
        else if (_particleSystem)
        {
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _particleSystem.Play();
        }

        mainObject.gameObject.SetActive(true);
        mainObject.transform.localPosition = Vector3.zero;
        _table = table;
        mainObject.localScale = Vector3.one * _table.scale;
        _onHit = onHit;
        if (table.delayRandomMin == 0)
        {
            Fire(targetPosition);
        }
        else
        {
            mainObject.gameObject.SetActive(false);
            StartCoroutine(FireDelay(Random.Range(table.delayRandomMin, table.delayRandomMax), targetPosition));
        }
    }

    private void GetComponents()
    {
        if (_isInitialized) return;
        _recycleObject = GetComponent<RecycleObject>();
        _trailRenderer = GetComponentInChildren<TrailRenderer>();
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        _isInitialized = true;
    }


    private IEnumerator FireDelay(float delay, Vector2 targetPosition)
    {
        yield return new WaitForSeconds(delay);
        mainObject.gameObject.SetActive(true);
        Fire(targetPosition);
    }


    private void Fire(Vector2 targetPosition)
    {
        if (_table.onLookat)
        {
            var dir = targetPosition - (Vector2)mainObject.position;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            mainObject.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        _previousPosition = mainObject.position;

        _sequence = DOTween.Sequence();
        if (_table.speedType == ProjectileSpeedType.Speed)
        {
            var speedValue = Vector2.Distance(transform.position, targetPosition) / Random.Range(_table.speedRandomMin, _table.speedRandomMax);
            _sequence.Append(transform.DOMove(targetPosition, speedValue).SetEase(normalCurve));
            if (FireType == FireType.Howitzer)
            {
                var howitzerTween = mainObject.DOLocalMoveY(howitzerHeight, speedValue).SetEase(howitzerCurve);
                if (_table.onLookat)
                {
                    howitzerTween.OnUpdate(UpdateHowitzerRotation);
                }

                _sequence.Join(howitzerTween);
            }
        }
        else
        {
            var speedValue = Random.Range(_table.speedRandomMin, _table.speedRandomMax);
            _sequence.Append(transform.DOMove(targetPosition, speedValue).SetEase(normalCurve));
            if (FireType == FireType.Howitzer)
            {
                var howitzerTween = mainObject.DOLocalMoveY(howitzerHeight, speedValue).SetEase(howitzerCurve);
                if (_table.onLookat)
                {
                    howitzerTween.OnUpdate(UpdateHowitzerRotation);
                }

                _sequence.Join(howitzerTween);
            }
        }

        _sequence.OnComplete(() =>
        {
            _onHit?.Invoke();
            StartCoroutine(ReleaseAfterDelay());
        });
        _sequence.Play();
    }
    private IEnumerator ReleaseAfterDelay()
    {
        yield return new WaitForSeconds(_releaseDelay);
        _recycleObject.Release();
    }
    private void UpdateHowitzerRotation()
    {
        // 현재 위치와 이전 위치의 차이로 실제 이동 방향 계산
        var currentPosition = mainObject.position;
        var velocity = currentPosition - _previousPosition;

        if (velocity.magnitude > 0.001f)
        {
            var angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            mainObject.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        _previousPosition = currentPosition;
    }
    
  
}