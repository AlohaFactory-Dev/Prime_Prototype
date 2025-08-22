using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class Factory
{
    private RecycleObject _prefab;
    private int _defaultPoolSize;
    private Transform _parent;
    private bool _useDynamicSize;

    private Queue<RecycleObject> _pool = new Queue<RecycleObject>();
    private Dictionary<RecycleObject, bool> _poolFindContainer = new Dictionary<RecycleObject, bool>();
    private List<RecycleObject> _usingPool = new List<RecycleObject>();
    private Dictionary<RecycleObject, int> _cachedUsingPoolIndexes = new Dictionary<RecycleObject, int>();
    private RecycleObject _tempObject;
    private DiContainer _container;
    private int _tempIndex;

    public Factory(DiContainer container, RecycleObject prefab, int defaultPoolSize, Transform parent, bool useDynamicSize = true)
    {
        _container = container;
        defaultPoolSize = System.Math.Max(defaultPoolSize, 1);

        _prefab = prefab;
        _defaultPoolSize = defaultPoolSize;
        _parent = parent;
        _useDynamicSize = useDynamicSize;
        CreatPool();
    }

    void CreatPool()
    {
        for (int i = 0; i < _defaultPoolSize; i++)
        {
            // _tempObject = GameObject.Instantiate(_prefab, _parent);
            _tempObject = _container.InstantiatePrefab(_prefab, _parent).GetComponent<RecycleObject>();
            _tempObject.InitializeByFactory(Release);
            InitializeObjectTransform(_tempObject);
            _pool.Enqueue(_tempObject);
            _poolFindContainer[_tempObject] = true;
        }
    }

    private void InitializeObjectTransform(RecycleObject recycleObject)
    {
        recycleObject.transform.localRotation = _prefab.transform.localRotation;
        recycleObject.transform.localScale = _prefab.transform.localScale;
        DisableObject(recycleObject);
    }

    protected virtual void DisableObject(RecycleObject recycleObject)
    {
        recycleObject.gameObject.SetActive(false);
    }

    public RecycleObject Get()
    {
        if (_pool.Count <= 0)
        {
            if (_useDynamicSize) CreatPool();
            else _usingPool[^1].Release();
        }

        _tempObject = _pool.Dequeue();
        _poolFindContainer.Remove(_tempObject);
        if (_useDynamicSize == false)
        {
            _usingPool.Add(_tempObject);
            _cachedUsingPoolIndexes[_tempObject] = _usingPool.Count - 1;
        }

        _tempObject.gameObject.SetActive(true);

        return _tempObject;
    }

    public void Release(RecycleObject recycleObject)
    {
        if (_poolFindContainer.ContainsKey(recycleObject)) return;

        InitializeObjectTransform(recycleObject);
        _pool.Enqueue(recycleObject);
        _poolFindContainer[recycleObject] = true;
        if (_useDynamicSize == false)
        {
            _tempObject = _usingPool[^1];
            _tempIndex = _cachedUsingPoolIndexes[recycleObject];

            _usingPool.RemoveAt(_tempIndex);
            _cachedUsingPoolIndexes.Remove(recycleObject);
            if (_tempObject == recycleObject) return;
            _cachedUsingPoolIndexes[_tempObject] = _tempIndex;
        }
    }
}