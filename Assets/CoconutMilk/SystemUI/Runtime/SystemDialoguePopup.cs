using Aloha.Coconut.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Aloha.CoconutMilk
{
    public class SystemDialoguePopup : UISlice
    {
        public class Args : UIOpenArgs
        {
            public string title;
            public string content;
            public string yes;
            public string no;
            
            public Args(string title, string content, string yes, string no)
            {
                this.title = title;
                this.content = content;
                this.yes = yes;
                this.no = no;
            }
        }
        
        public class Result : UICloseResult
        {
            public bool isYes;
            
            public Result(bool isYes)
            {
                this.isYes = isYes;
            }
        }
        
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text contentText;
        [SerializeField] private TMP_Text yesText;
        [SerializeField] private TMP_Text noText;
        
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        void Start()
        {
            yesButton.onClick.AddListener(() => CloseView(new Result(true)));
            noButton.onClick.AddListener(() => CloseView(new Result(false)));
        }
        
        protected override void Open(UIOpenArgs openArgs)
        {
            var args = (Args) openArgs;
            
            noButton.gameObject.SetActive(args.no != null);
            titleText.text = args.title;
            contentText.text = args.content;
            yesText.text = args.yes;
            noText.text = args.no;
        }
    }
}
