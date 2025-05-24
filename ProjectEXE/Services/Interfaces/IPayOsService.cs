using Net.payOS.Types;
using ProjectEXE.DTO;

namespace ProjectEXE.Services.Interfaces
{
    public interface IPayOsService
    {
        Task<CreatePaymentResult> CreatePayOSPaymentUrl(PaymentData paymentData);
        Task<PaymentLinkInformation> GetPaymentLinkInfor(long id);
        Task<PaymentLinkInformation> CancelPaymentLink(long orderCode, string cancellationReason);
        PaymentResultModel ProcessReturnUrl(IQueryCollection queryParams);
    }
}
