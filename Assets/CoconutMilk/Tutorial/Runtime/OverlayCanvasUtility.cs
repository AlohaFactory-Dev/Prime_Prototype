using UnityEngine;

namespace Aloha.CoconutMilk
{
    public static class OverlayCanvasUtility
    {
        public static Vector2 ConvertGameObjectPosition(GameObject target, Camera camera, Canvas targetCanvas)
        {
            var viewportPoint = camera.WorldToViewportPoint(target.transform.position);
            var positionFromScreenCenter = ((Vector2)viewportPoint - new Vector2(.5f, .5f)) * GetCameraSize(camera);
            return positionFromScreenCenter /
                   (targetCanvas.pixelRect.width / ((RectTransform)targetCanvas.transform).sizeDelta.x);
        }

        public static Vector2 ConvertWorldToUIPosition(Vector3 position, Camera camera, Canvas targetCanvas)
        {
            var viewportPoint = camera.WorldToViewportPoint(position);
            var positionFromScreenCenter = ((Vector2)viewportPoint - new Vector2(.5f, .5f)) * GetCameraSize(camera);
            return positionFromScreenCenter /
                   (targetCanvas.pixelRect.width / ((RectTransform)targetCanvas.transform).sizeDelta.x);
        }

        private static Vector2 GetCameraSize(Camera camera)
        {
            return new Vector2(camera.pixelWidth, camera.pixelHeight);
        }

        public static Vector2 ConvertOverlayUIPosition(RectTransform target, Canvas targetCanvas)
        {
            var rootCanvas = FindRootCanvas(target);
            if (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.LogError(
                    "Root canvas of the targetObject is not in Overlay mode! Use ConvertGameObjectPosition instead.");
                return Vector2.zero;
            }

            var targetCenterWorldPosition =
                (Vector2)target.position + target.sizeDelta * (new Vector2(.5f, .5f) - target.pivot);
            var targetCenterLocalPosition = rootCanvas.transform.InverseTransformPoint(targetCenterWorldPosition);
            var positionFromScreenCenter = (Vector3)targetCenterLocalPosition * rootCanvas.transform.localScale.x;
            return positionFromScreenCenter / targetCanvas.transform.localScale.x;
        }

        private static Canvas FindRootCanvas(RectTransform target)
        {
            Canvas result = null;
            Transform findTransform = target;

            while (findTransform != null)
            {
                if (findTransform.TryGetComponent<Canvas>(out var c))
                {
                    result = c;
                }

                findTransform = findTransform.parent;
            }

            return result;
        }

        public static Vector2 ConvertScreenPosition(Vector3 screenPosition, Canvas targetCanvas)
        {
            var canvasScale = targetCanvas.transform.localScale.x;
            var screenSize = targetCanvas.pixelRect.size;
            var anchoredPositionNotScaled = (Vector2)screenPosition - screenSize / 2f;
            return anchoredPositionNotScaled / canvasScale;
        }
    }
}