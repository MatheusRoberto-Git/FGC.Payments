namespace FGC.Payments.Application.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishPaymentProcessedAsync(PaymentProcessedMessage message);
    }

    public record PaymentProcessedMessage(
        Guid PaymentId,
        Guid UserId,
        Guid GameId,
        decimal Amount,
        string Status,
        DateTime ProcessedAt
    );
}