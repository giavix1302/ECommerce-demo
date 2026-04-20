using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Exceptions;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace Infrastructure.Services;

public class PayOSService : IPaymentService
{
    private readonly PayOSClient _client;
    private readonly PayOSSettings _settings;

    public PayOSService(PayOSClient client, IOptions<PayOSSettings> options)
    {
        _client = client;
        _settings = options.Value;
    }


    public async Task<string> CreatePaymentLinkAsync(long orderId, decimal amount)
    {
        var request = new CreatePaymentLinkRequest
        {
            OrderCode = orderId,
            Amount = (long)amount,
            Description = $"Order #{orderId}",
            ReturnUrl = _settings.ReturnUrl,
            CancelUrl = _settings.CancelUrl
        };

        var paymentLink = await _client.PaymentRequests.CreateAsync(request);
        return paymentLink.CheckoutUrl;
    }

    public async Task<long> VerifyWebhookAsync(PayOSWebhookPayload payload)
    {
        var webhook = new Webhook
        {
            Code = payload.Code,
            Description = payload.Desc,
            Success = payload.Success,
            Signature = payload.Signature,
            Data = new WebhookData
            {
                OrderCode = payload.Data.OrderCode,
                Amount = payload.Data.Amount,
                Description = payload.Data.Description,
                AccountNumber = payload.Data.AccountNumber,
                Reference = payload.Data.Reference,
                TransactionDateTime = payload.Data.TransactionDateTime,
                Currency = payload.Data.Currency,
                PaymentLinkId = payload.Data.PaymentLinkId,
                Code = payload.Data.Code,
                Description2 = payload.Data.Desc,
                CounterAccountBankId = payload.Data.CounterAccountBankId,
                CounterAccountBankName = payload.Data.CounterAccountBankName,
                CounterAccountName = payload.Data.CounterAccountName,
                CounterAccountNumber = payload.Data.CounterAccountNumber,
                VirtualAccountName = payload.Data.VirtualAccountName,
                VirtualAccountNumber = payload.Data.VirtualAccountNumber,
            }
        };

        try
        {
            var data = await _client.Webhooks.VerifyAsync(webhook);
            return data.OrderCode;
        }
        catch (Exception ex) when (ex is InvalidSignatureException or WebhookException)
        {
            throw new InvalidWebhookSignatureException();
        }
    }
}
