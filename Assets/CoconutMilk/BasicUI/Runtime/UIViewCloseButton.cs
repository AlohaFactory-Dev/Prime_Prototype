using Aloha.Coconut.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Aloha.CoconutMilk
{
    [RequireComponent(typeof(Button))]
    public class UIViewCloseButton : MonoBehaviour
    {
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                GetComponentInParent<UIView>().Close();
            });
        }

        private void OnEnable()
        {
            Assert.IsNotNull(GetComponentInParent<UIView>(), "UIViewCloseButton must be a child of UIView");
        }
    }
}