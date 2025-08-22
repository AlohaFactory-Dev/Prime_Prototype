using System.Collections.Generic;
using UnityEngine;

namespace Aloha.Coconut.UI
{
    [CreateAssetMenu(fileName = "UIViewConfig", menuName = "Coconut/UI/UIViewConfig")]
    public class UIViewConfig : ScriptableObject
    {
        public bool isOverlay = true;
        public List<UISlice> slices;

#if UNITY_EDITOR
        public void Preview()
        {
            var canvas = FindCanvas();
            if (canvas != null)
            {
                canvas.previewViewConfig = this;
                canvas.Preview();
            }
        }

        public bool PreviewingThis()
        {
            return FindCanvas()?.previewViewConfig == this;
        }

        public void ClearPreview()
        {
            var canvas = FindCanvas();
            if (canvas != null)
            {
                canvas.previewViewConfig = null;
                canvas.ClearPreview();   
            }
        }

        private CoconutCanvas FindCanvas()
        {
            var coconutRootCanvas = FindObjectOfType<CoconutCanvas>();
            return coconutRootCanvas;
        }
#endif
    }
}
