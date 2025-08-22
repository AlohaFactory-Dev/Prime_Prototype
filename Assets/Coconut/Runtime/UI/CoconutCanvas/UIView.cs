using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Aloha.Coconut.UI
{
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
    public class UIView : MonoBehaviour
    {
        private UIViewConfig _config;
        private CoconutCanvas _canvas;
        private UIOpenArgs _openArgs;
        
        public string Name => _config.name;
        public UIViewConfig Config => _config;
        public CoconutCanvas Canvas => _canvas;
        
        internal List<UISlice> Slices { get; private set; }
        
        public void Open(CoconutCanvas canvas, UIViewConfig config, List<UISlice> slices, UIOpenArgs openArgs)
        {
            _config = config;
            _canvas = canvas;
            _openArgs = openArgs;
            Slices = slices;
         
            gameObject.name = _config.name;   
            RefreshSlices();
        }
        
        internal void RefreshSlices()
        {
            int index = 0;
            foreach (var slice in Slices)
            {
                slice.transform.SetParent(transform, false);
                slice.transform.SetSiblingIndex(index++);
                
                slice.CurrentView = this;
            }

            // Slice들 간의 상호작용 이슈를 없애기 위해 slice들의 GameObject parenting이 전부 끝난 후에 Open 호출
            foreach (var slice in Slices)
            {
                slice.Open(_openArgs);
            }
        }

        internal void Clear()
        {
            _config = null;
            _openArgs = null;
            Slices = null;
            
            gameObject.name = "UIView";
        }
        
        public T GetSlice<T>() where T : class
        {
            foreach (var slice in Slices)
            {
                if (slice is T t) return t;
            }
            return null;
        }

        public void Close(UICloseResult closeResult = null)
        {
            _canvas.Close(this, closeResult);
        }
    }
}