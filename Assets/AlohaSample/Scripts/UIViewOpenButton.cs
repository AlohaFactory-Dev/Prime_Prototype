using Aloha.Coconut.UI;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIViewOpenButton : MonoBehaviour
{
    [SerializeField] private UIViewConfig viewConfig;
    
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            GetComponentInParent<CoconutCanvas>().Open(viewConfig);
        });
    }
}
