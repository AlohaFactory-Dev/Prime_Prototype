using Aloha.Coconut.UI;
using UnityEngine;
using UnityEngine.UI;
using Aloha.Coconut;

namespace Aloha.CoconutMilk
{
    public class InAppReviewPopup : UISlice
    {
        [SerializeField] private Button showReviewButton;
        [SerializeField] private Button skipReviewButton;

        void Awake()
        {
            showReviewButton.onClick.AddListener(() =>
            {
                InAppReviewManager.ShowInAppReview();
                CloseView();
            });
            skipReviewButton.onClick.AddListener(CloseView);
        }
        
        protected override void Open(UIOpenArgs openArgs)
        {
            if (InAppReviewManager.ShouldReviewBlocked)
            {
                Debug.Log("Review was already done. Skip InAppReviewPopup.");
                CloseView();
                return;
            }
        }
    }
}