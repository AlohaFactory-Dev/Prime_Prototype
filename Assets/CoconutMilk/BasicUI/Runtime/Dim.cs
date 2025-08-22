using Aloha.Coconut.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Aloha.CoconutMilk
{
    public interface IDimClosable
    {
        void CloseByDim();
    }
    
    [RequireComponent(typeof(Button))]
    public class Dim : UISlice
    {
        private IDimClosable _dimClosable;

        void Start()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                _dimClosable?.CloseByDim();
            });
        }
        
        protected override void Open(UIOpenArgs openArgs)
        {
            base.Open(openArgs);
            _dimClosable = CurrentView.GetSlice<IDimClosable>();
        }
    }
}
