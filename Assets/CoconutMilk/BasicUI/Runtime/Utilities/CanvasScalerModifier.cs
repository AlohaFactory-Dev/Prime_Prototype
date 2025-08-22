using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class CanvasScalerModifier : MonoBehaviour
{
    void Start()
    {
        GetComponent<CanvasScaler>().matchWidthOrHeight = (float)Screen.width / Screen.height > 9f / 16 ? 1 : 0;
    }

#if UNITY_EDITOR
    void Update()
    {
        GetComponent<CanvasScaler>().matchWidthOrHeight = (float)Screen.width / Screen.height > 9f / 16 ? 1 : 0;
    }
#endif
}