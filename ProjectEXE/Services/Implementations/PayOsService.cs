﻿using Net.payOS;
using Net.payOS.Types;
using ProjectEXE.DTO;
using ProjectEXE.Services.Interfaces;

namespace ProjectEXE.Services.Implementations
{
    public class PayOsService : IPayOsService
    {
        private readonly PayOS _payOs;
        public PayOsService(IConfiguration configuration)
        {
            var clientId = configuration["PayOS:ClientID"];
            var apiKey = configuration["PayOS:APIKey"];
            var checksumKey = configuration["PayOS:ChecksumKey"];

            _payOs = new PayOS(clientId, apiKey, checksumKey);
        }

        public async Task<PaymentLinkInformation> CancelPaymentLink(long orderCode, string cancellationReason)
        {
            return await _payOs.cancelPaymentLink(orderCode, cancellationReason);
        }

        public async Task<CreatePaymentResult> CreatePayOSPaymentUrl(PaymentData paymentData)
        {
            return await _payOs.createPaymentLink(paymentData);
        }

        public async Task<PaymentLinkInformation> GetPaymentLinkInfor(long id)
        {
            return await _payOs.getPaymentLinkInformation(id);
        }


        public PaymentResultModel ProcessReturnUrl(IQueryCollection queryParams)
        {
            var result = new PaymentResultModel
            {
                Code = queryParams.ContainsKey("code") ? queryParams["code"] : string.Empty,
                Id = queryParams.ContainsKey("id") ? queryParams["id"] : string.Empty,
                Cancel = queryParams.ContainsKey("cancel") ? queryParams["cancel"] : string.Empty,
                Status = queryParams.ContainsKey("status") ? queryParams["status"] : string.Empty,
                OrderCode = queryParams.ContainsKey("orderCode") ? queryParams["orderCode"] : string.Empty,
            };
            return result;
        }
    }
}
