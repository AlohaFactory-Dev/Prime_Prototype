using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Aloha.Coconut.UI
{
    public class UISlice : MonoBehaviour
    {
        public UIView CurrentView { get; internal set; }
        public CoconutCanvas Canvas => CurrentView.Canvas;

        protected internal virtual void Open(UIOpenArgs openArgs)
        {
            IsOpened = true;
        }
        protected bool IsOpened;

        // Inspector에서 호출 가능하도록 closeResult 없이 호출하는 CloseView를 따로 둬야 함
        public void CloseView()
        {
            CloseView(null);
        }

        public void CloseView(UICloseResult closeResult)
        {
            CurrentView.Close(closeResult);
        }

        protected internal virtual void OnClose()
        {
            IsOpened = false;
        }

#if UNITY_EDITOR
        [NonSerialized, ShowInInspector, PropertyOrder(int.MaxValue - 1)]
        private List<UIViewConfigEntry> _usingViews = new();

        [NonSerialized, ShowInInspector, PropertyOrder(int.MaxValue)] [OnValueChanged("OnUsingViewAdded")]
        private UIViewConfig _addUsingView;

        [Serializable]
        private struct UIViewConfigEntry
        {
            [HideLabel]
            [InlineButton("Preview", ShowIf = "IsNotShowingPreview")]
            [InlineButton("Clear", ShowIf = "IsShowingPreview")]
            public UIViewConfig uiViewConfig;

            private bool IsShowingPreview => uiViewConfig.PreviewingThis();
            private bool IsNotShowingPreview => !uiViewConfig.PreviewingThis();

            public void Preview()
            {
                uiViewConfig.Preview();
            }

            public void Clear()
            {
                uiViewConfig.ClearPreview();
            }
        }

        [OnInspectorInit]
        private void FetchUsingViews()
        {
            // check if prefab
            if (!PrefabUtility.IsPartOfPrefabAsset(this) &&
                PrefabUtility.GetPrefabInstanceStatus(this) != PrefabInstanceStatus.NotAPrefab)
            {
                return;
            }

            _usingViews.Clear();

            var prefab = GetPrefab();
            var assets = AssetDatabase.FindAssets($"t:{nameof(UIViewConfig)}");
            foreach (var guid in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var view = AssetDatabase.LoadAssetAtPath<UIViewConfig>(path);
                foreach (var slice in view.slices)
                {
                    if (slice == prefab)
                    {
                        _usingViews.Add(new UIViewConfigEntry { uiViewConfig = view });
                        break;
                    }
                }
            }
        }

        private UISlice GetPrefab()
        {
            UISlice prefab;
            // if prefab stage
            if (PrefabStageUtility.GetCurrentPrefabStage() != null && !AssetDatabase.IsMainAsset(gameObject))
            {
                prefab = AssetDatabase
                    .LoadAssetAtPath<GameObject>(PrefabStageUtility.GetPrefabStage(this.gameObject).assetPath)
                    .GetComponent<UISlice>();
            }
            else
            {
                prefab = PrefabUtility.GetCorrespondingObjectFromSource(this) ?? this;
            }

            return prefab;
        }

        private void OnUsingViewAdded()
        {
            if (_addUsingView == null) return;
            var prefab = GetPrefab();
            if (!_addUsingView.slices.Contains(prefab))
            {
                _addUsingView.slices.Add(prefab);
                EditorUtility.SetDirty(_addUsingView);
                FetchUsingViews();
            }

            _addUsingView = null;
        }
#endif
    }
}