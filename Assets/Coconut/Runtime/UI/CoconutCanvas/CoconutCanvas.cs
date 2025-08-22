using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using Zenject;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Aloha.Coconut.UI
{
    public class CoconutCanvas : MonoBehaviour
    {
        public IObservable<(UIView, UICloseResult)> OnViewClosed => _onViewClosed;
        public bool IsOnRootView => _viewStack.Count == 1 && _viewStack[^1].Config == rootViewConfig;

        private Subject<(UIView, UICloseResult)> _onViewClosed = new();

        [Inject] private DiContainer _diContainer;

        [SerializeField] private UIViewConfig rootViewConfig;
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private UIView viewPrefab;
        [SerializeField] private RectTransform viewParent;
        [SerializeField] private RectTransform uiPoolParent;

#if UNITY_EDITOR
        [Header("Preview")] [ShowInInspector, NonSerialized, OnValueChanged("Preview")]
        public UIViewConfig previewViewConfig;

        private UIView previewingView;
#endif

        private List<UIViewConfig> _viewConfigs = new();

        private List<UIView> _viewStack = new(); // 대부분의 로직에서 Stack의 구조로 동작하지만, Top이 아닌 View도 닫을 수 있도록 하기 위해 List로 구현함. 구조상 이름은 Stack으로 존치
        private Queue<UIView> _viewPool = new();

        private Dictionary<UISlice, UISlice> _sliceInstances = new();

        void Start()
        {
            uiPoolParent.gameObject.SetActive(false);
            if (initializeOnStart) Initialize();
        }

        public void Initialize()
        {
            if (rootViewConfig != null) Open(rootViewConfig);
        }

        public UIView Open(string viewConfigName, UIOpenArgs openArgs = null)
        {
            var viewConfig = _viewConfigs.FirstOrDefault(config => config.name == viewConfigName);
            if (viewConfig == null)
            {
                viewConfig = Addressables.LoadAssetAsync<UIViewConfig>(viewConfigName).WaitForCompletion();
                _viewConfigs.Add(viewConfig);
            }

            return Open(viewConfig, openArgs);
        }

        public UIView Open(UIViewConfig viewConfig, UIOpenArgs openArgs = null)
        {
            if (!viewConfig.isOverlay && _viewStack.Count > 0)
            {
                Assert.IsFalse(_viewStack[^1].Config.isOverlay,
                    "Trying to open an non-overlay view on top of a overlay view.");
                Close(_viewStack[^1], null);
            }

            if (_viewPool.Count == 0)
            {
                _viewPool.Enqueue(Instantiate(viewPrefab, viewParent));
            }

            var newView = _viewPool.Dequeue();
            newView.transform.SetParent(viewParent);

            var slices = new List<UISlice>();
            foreach (var slicePrefab in viewConfig.slices)
            {
                if (!_sliceInstances.ContainsKey(slicePrefab))
                {
                    _sliceInstances[slicePrefab] = _diContainer.InstantiatePrefab(slicePrefab, uiPoolParent)
                        .GetComponent<UISlice>();
                }

                slices.Add(_sliceInstances[slicePrefab]);
            }

            _viewStack.Add(newView);
            newView.Open(this, viewConfig, slices, openArgs);

            return newView;
        }
        
        public async UniTask<UICloseResult> OpenAsync(UIViewConfig viewConfig, UIOpenArgs openArgs = null)
        {
            var view = Open(viewConfig, openArgs);
            return await _onViewClosed.Where(x => x.Item1 == view)
                .Select(x => x.Item2).First().ToUniTask();
        }

        public async UniTask<UICloseResult> OpenAsync(string viewConfigName, UIOpenArgs openArgs = null)
        {
            var view = Open(viewConfigName, openArgs);
            return await _onViewClosed.Where(x => x.Item1 == view)
                .Select(x => x.Item2).First().ToUniTask();
        }

        public void Preload(UIViewConfig viewConfig)
        {
            foreach (var slicePrefab in viewConfig.slices)
            {
                if (!_sliceInstances.ContainsKey(slicePrefab))
                {
                    _sliceInstances[slicePrefab] = _diContainer.InstantiatePrefab(slicePrefab, uiPoolParent)
                        .GetComponent<UISlice>();
                }
            }
        }

        internal void Close(UIView uiView, UICloseResult closeResult)
        {
            bool wasTopView = _viewStack[^1] == uiView;

            foreach (var slice in uiView.Slices)
            {
                if (slice.CurrentView != uiView) continue;
                slice.OnClose();
                slice.CurrentView = null;
                
                // UISlice가 재사용되는 경우, OnDisable/OnEnable 호출이 일어나면 안됨
                // uiPoolParent는 비활성화되어 있으므로, 재사용되는 경우는 사용하고 있는 다른 UIView로 옮겨 OnDisable/OnEnable 호출을 막음
                UIView otherViewContaining = _viewStack.FirstOrDefault(view => view != uiView && view.Slices.Contains(slice));
                if (otherViewContaining != null)
                {
                    slice.transform.SetParent(otherViewContaining.transform, false);
                }
                else
                {
                    slice.transform.SetParent(uiPoolParent, false);   
                }
            }

            UIViewConfig closedViewConfig = uiView.Config;
            uiView.Clear();
            uiView.transform.SetParent(uiPoolParent);
            _viewPool.Enqueue(uiView);
            _viewStack.Remove(uiView);
            
            if (wasTopView && _viewStack.Count > 0) _viewStack[^1].RefreshSlices();
            _onViewClosed.OnNext((uiView, closeResult));
        }

        public void CloseTop()
        {
            if (_viewStack.Count == 0) return;
            if (_viewStack.Count == 1 && !IsOnRootView && rootViewConfig != null)
            {
                Open(rootViewConfig);
            }
            else
            {
                Close(_viewStack[^1], null);
            }
        }
        
        public T GetSlice<T>() where T : UISlice
        {
            foreach (var view in _viewStack)
            {
                var slice = view.GetSlice<T>();
                if (slice != null) return slice;
            }

            Debug.LogError($"Cannot find slice of type {typeof(T).Name}.");
            return null;
        }

#if UNITY_EDITOR

        [Button]
        public void Preview()
        {
            if (Application.isPlaying)
            {
                Debug.LogError("Preview is only available in editor mode.");
                return;
            }

            ClearPreview();
            if (previewViewConfig == null) return;

            previewingView = (UIView)PrefabUtility.InstantiatePrefab(viewPrefab, viewParent);
            previewingView.gameObject.hideFlags = HideFlags.HideAndDontSave;
            previewingView.transform.SetParent(viewParent);

            foreach (var slicePrefab in previewViewConfig.slices)
            {
                var slice = (UISlice)PrefabUtility.InstantiatePrefab(slicePrefab, previewingView.transform);
                slice.gameObject.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        [Button]
        public void ClearPreview()
        {
            if (previewingView) DestroyImmediate(previewingView.gameObject);
        }
#endif
    }
}