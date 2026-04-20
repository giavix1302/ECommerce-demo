
namespace Application.Common.Interfaces;

public interface IPaymentService
{
    Task<string> CreatePaymentLinkAsync(long orderId, decimal amount);
    Task<long> VerifyWebhookAsync(PayOSWebhookPayload payload);
}

public record PayOSWebhookPayload(
    string Code,
    string Desc,
    bool Success,
    string Signature,
    PayOSWebhookData Data
);

public record PayOSWebhookData(
    long OrderCode,
    long Amount,
    string Description,
    string AccountNumber,
    string Reference,
    string TransactionDateTime,
    string Currency,
    string PaymentLinkId,
    string Code,
    string Desc,
    string CounterAccountBankId,
    string CounterAccountBankName,
    string CounterAccountName,
    string CounterAccountNumber,
    string VirtualAccountName,
    string VirtualAccountNumber
);