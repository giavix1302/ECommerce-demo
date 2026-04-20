namespace Application.Common.Exceptions;

public class InvalidWebhookSignatureException : Exception
{
    public InvalidWebhookSignatureException()
        : base("Invalid webhook signature.")
    {
    }
}
