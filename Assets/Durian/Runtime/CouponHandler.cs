using Alohacorp.Durian.Client;
using Alohacorp.Durian.Model;
using Cysharp.Threading.Tasks;

namespace Aloha.Durian
{
    public class CouponHandler
    {
        private readonly MailManager _mailManager;

        public enum HandleResult
        {
            Success,
            Failed_UsedCoupon,
            Failed_NotFound,
        }

        public CouponHandler(MailManager mailManager)
        {
            _mailManager = mailManager;
        }
        
        private async UniTask<HandleResult> Handle(string couponCode)
        {
            try
            {
                var couponApi = await DurianApis.CouponApi();
                UseCouponReqDto useCouponReqDto = new UseCouponReqDto(couponCode.ToUpper());
                await couponApi.UseCouponAsync(useCouponReqDto);

                // 쿠폰은 결과는 메일로 옴
                _mailManager.FetchMails().Forget();
                return HandleResult.Success;
            }
            catch (ApiException e)
            {
                if (e.ErrorCode == 400)
                {
                    return HandleResult.Failed_UsedCoupon;
                }
                else
                {
                    return HandleResult.Failed_NotFound;
                }
            }
        }
    }
}
