using UnityEngine;

#if UNITY_ANDROID
using Google.Play.Review;
#elif UNITY_IOS
using UnityEngine.iOS;
#endif

namespace Aloha.Coconut
{
    public static class InAppReviewManager
    {
        // Google, iOS 모두 인앱리뷰 할당량이 있어, Native 코드로 리뷰를 호출했다면 가까운 시일 내에는 호출해도 아무 반응도 나타나지 않음
        // 이러한 경우를 방지하기 위해 게임 내에서 자체적으로 리뷰가 호출되었음을 트래킹하여, 리뷰가 한 번 이상 호출되었다면 리뷰 요청을 다시 띄우지 않도록 조치
        // ex)
        // 1. 리뷰 요청 조건 만족시 ShouldReviewBlocked false면 [게임이 마음에 드시나요?] 팝업 노출, true면 아무 일도 일어나지 않음
        // 2. "예"선택시 ShowInAppReview 호출되어 ShouldReviewBlocked가 true로 설정됨, 이후 리뷰 요청 조건을 만족해도 팝업이 노출되지 않음
        // 3. "아니오" 선택시 ShouldReviewBlocked는 false로 유지되어 다음 리뷰 요청 조건을 만족할 때 다시 팝업이 노출됨
        // 단, DebugConfig.UseDebug가 true일 경우에는 WasReviewed를 무시하고 ShowInAppReview를 호출함
        public static bool ShouldReviewBlocked => PlayerPrefs.GetInt(REVIEWED_KEY, 0) == 1 && !DebugConfig.UseDebug;
        private const string REVIEWED_KEY = "coconut.wasReviewed";

        public static void ShowInAppReview()
        {
            if (ShouldReviewBlocked)
            {
                Debug.Log("Already reviewed.");
                return;
            }
            
#if UNITY_ANDROID
            var reviewManager = new ReviewManager();

            // start preloading the review prompt in the background
            var playReviewInfoAsyncOperation = reviewManager.RequestReviewFlow();
            playReviewInfoAsyncOperation.Completed += playReviewInfoAsync =>
            {
                if (playReviewInfoAsync.Error == ReviewErrorCode.NoError)
                {
                    // display the review prompt
                    var playReviewInfo = playReviewInfoAsync.GetResult();
                    var reviewFlow = reviewManager.LaunchReviewFlow(playReviewInfo);

                    reviewFlow.Completed += reviewFlowAsync =>
                    {
                        // The flow has finished. The API does not indicate whether the user
                        // reviewed or not, or even whether the review dialog was shown. Thus, no
                        // matter the result, we continue our app flow.
                        if (reviewFlowAsync.Error != ReviewErrorCode.NoError)
                        {
                            Debug.Log("Error launching review prompt (review flow): " + reviewFlowAsync.Error);
                        }
                        
                        PlayerPrefs.SetInt(REVIEWED_KEY, 1);
                    };
                }
                else
                {
                    // handle error when loading review prompt
                    Debug.Log("Error launching review prompt (review info): " + playReviewInfoAsync.Error);
                }
            };
#elif UNITY_IOS
            Device.RequestStoreReview();
            PlayerPrefs.SetInt(REVIEWED_KEY, 1);
#endif
        }
    }
}