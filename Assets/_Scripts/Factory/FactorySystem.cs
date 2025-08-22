using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Scripts;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public abstract class FactorySystem<PrefabType, ID> where PrefabType : MonoBehaviour
{
    private Factory _defaultFactory;
    private HashSet<ID> _keys;
    private Factory _tempFactory;
    protected Dictionary<ID, Factory> factories = new();
    protected Transform _parent;
    private bool isInitialized;
    protected Dictionary<ID, PrefabType> prefabContainer = new();
    protected PrefabType tempObject;
    protected bool useFactory = true;
    protected abstract string LabelId { get; }

    protected virtual bool IsLazy => false;
    private DiContainer _diContainer;

    public async Task Initialize(DiContainer diContainer, Transform parent)
    {
        _diContainer = diContainer;
        // Debug.Log($"팩토리 세부 생성 시작 {startTime}, {this}");
        _parent = parent;
        await OnInitialize();
        // Debug.Log($"팩토리 세부 생성 종료 {Time.time}, 소요시간 : {Time.time - startTime} 초, {this}");
    }

    protected abstract ID TranslateStringKeyToID(string primaryKey);

    protected async Task OnInitialize()
    {
        if (!useFactory) return;

        if (IsLazy) _keys = new HashSet<ID>();

        var loadResourceHandle = Addressables.LoadResourceLocationsAsync(LabelId, typeof(GameObject));
        await loadResourceHandle.Task;

        if (IsLazy)
        {
            foreach (var t in loadResourceHandle.Result)
            {
                _keys.Add(TranslateStringKeyToID(t.PrimaryKey));
            }

            Addressables.Release(loadResourceHandle);
            return;
        }

        var opList = new List<AsyncOperationHandle<GameObject>>();
        var keyList = new List<string>();
        foreach (var t in loadResourceHandle.Result)
        {
            var handler = Addressables.LoadAssetAsync<GameObject>(t.PrimaryKey);
            var key = t.PrimaryKey;
            opList.Add(handler);
            keyList.Add(key);
        }

        await Task.WhenAll(opList.Select(op => op.Task));

        for (var i = 0; i < opList.Count; i++)
        {
            OnHandlerCompleted(opList[i], keyList[i]);
        }

        if (prefabContainer.Count == 0)
        {
            Debug.LogError($"{this}: {nameof(prefabContainer)} 내부에 값이 존재해야만 합니다");
        }
        else
        {
            _defaultFactory = factories.ElementAt(0).Value;
        }

        Addressables.Release(loadResourceHandle);
    }

    private void OnHandlerCompleted(AsyncOperationHandle<GameObject> obj, string key)
    {
        // Debug.Log($"OnHandlerCompleted called for key: {key}");
        HandlerCallback(TranslateStringKeyToID(key), obj.Result.GetComponent<PrefabType>());
    }

    protected void HandlerCallback(ID id, PrefabType prefabType)
    {
        if (prefabType == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"{this} : {id}에 해당하는 프리팹에 컴포넌트가 존재하지 않습니다");
#endif
        }
        else
        {
            prefabContainer[id] = prefabType;
            InitializeFactory(id);
        }
    }

    private void InitializeFactory(ID id, bool useDynamicPool = true)
    {
        var recycleObject = prefabContainer[id].GetComponent<RecycleObject>();
        factories[id] = new Factory(_diContainer, recycleObject, recycleObject.PoolSize, _parent, useDynamicPool);
    }

    private Factory GetFactory(ID id, int index = 0)
    {
        if (IsLazy && index >= _keys.Count)
        {
            Debug.LogError($"{this} : 모든 key들을 순회했지만 올바르게 세팅된 프리팹이 팩토리내에 존재하지 않습니다.");
            return null;
        }

        if (factories.TryGetValue(id, out _tempFactory))
        {
            return _tempFactory;
        }

        if (IsLazy && _keys.Contains(id))
        {
            var key = id.ToString();
            var prefab = Addressables.LoadAssetAsync<GameObject>(key).WaitForCompletion().GetComponent<PrefabType>();
            if (prefab == null)
            {
                Debug.LogError($"{this}: factoryType = {id} 프리팹에 해당 타입의 컴포넌트가 존재하지 않습니다");
                var count = _keys.Count;
                if (factories.Count == 0)
                {
                    index++;
                    var factory = GetFactory(_keys.ElementAt(Mathf.Min(index, count - 1)), Mathf.Min(index, count));
                    if (factory == null)
                    {
                        return null;
                    }
                }

                return factories.ElementAt(0).Value;
            }

            HandlerCallback(id, prefab);
            return factories[id];
        }

        Debug.LogError($"{this}: factoryType = {id}, 해당 팩토리는 존재하지 않습니다");
        return null;
    }

    public PrefabType GetObject(ID id)
    {
        _tempFactory = GetFactory(id);
        if (_tempFactory == null)
        {
            return _defaultFactory.Get().GetComponent<PrefabType>();
        }

        return _tempFactory.Get().GetComponent<PrefabType>();
    }
}