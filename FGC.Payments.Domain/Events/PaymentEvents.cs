using FGC.Payments.Domain.Common.Events;

namespace FGC.Payments.Domain.Events
{
    public class PaymentCreatedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredAt { get; }
        public Guid PaymentId { get; }
        public Guid UserId { get; }
        public Guid GameId { get; }
        public decimal Amount { get; }
        public string TransactionId { get; }
        public DateTime CreatedAt { get; }

        public PaymentCreatedEvent(Guid paymentId, Guid userId, Guid gameId, decimal amount, string transactionId, DateTime createdAt)
        {
            Id = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            PaymentId = paymentId;
            UserId = userId;
            GameId = gameId;
            Amount = amount;
            TransactionId = transactionId;
            CreatedAt = createdAt;
        }
    }

    public class PaymentProcessingEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredAt { get; }
        public Guid PaymentId { get; }
        public string TransactionId { get; }
        public DateTime ProcessingStartedAt { get; }

        public PaymentProcessingEvent(Guid paymentId, string transactionId, DateTime processingStartedAt)
        {
            Id = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            PaymentId = paymentId;
            TransactionId = transactionId;
            ProcessingStartedAt = processingStartedAt;
        }
    }

    public class PaymentCompletedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredAt { get; }
        public Guid PaymentId { get; }
        public Guid UserId { get; }
        public Guid GameId { get; }
        public decimal Amount { get; }
        public string TransactionId { get; }
        public DateTime CompletedAt { get; }

        public PaymentCompletedEvent(Guid paymentId, Guid userId, Guid gameId, decimal amount, string transactionId, DateTime completedAt)
        {
            Id = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            PaymentId = paymentId;
            UserId = userId;
            GameId = gameId;
            Amount = amount;
            TransactionId = transactionId;
            CompletedAt = completedAt;
        }
    }

    public class PaymentFailedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredAt { get; }
        public Guid PaymentId { get; }
        public string TransactionId { get; }
        public string FailureReason { get; }
        public DateTime FailedAt { get; }

        public PaymentFailedEvent(Guid paymentId, string transactionId, string failureReason, DateTime failedAt)
        {
            Id = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            PaymentId = paymentId;
            TransactionId = transactionId;
            FailureReason = failureReason;
            FailedAt = failedAt;
        }
    }

    public class PaymentRefundedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredAt { get; }
        public Guid PaymentId { get; }
        public Guid UserId { get; }
        public decimal Amount { get; }
        public string TransactionId { get; }
        public DateTime RefundedAt { get; }

        public PaymentRefundedEvent(Guid paymentId, Guid userId, decimal amount, string transactionId, DateTime refundedAt)
        {
            Id = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            PaymentId = paymentId;
            UserId = userId;
            Amount = amount;
            TransactionId = transactionId;
            RefundedAt = refundedAt;
        }
    }

    public class PaymentCancelledEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredAt { get; }
        public Guid PaymentId { get; }
        public string TransactionId { get; }
        public DateTime CancelledAt { get; }

        public PaymentCancelledEvent(Guid paymentId, string transactionId, DateTime cancelledAt)
        {
            Id = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            PaymentId = paymentId;
            TransactionId = transactionId;
            CancelledAt = cancelledAt;
        }
    }
}