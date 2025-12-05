using FGC.Payments.Domain.Common.Entities;
using FGC.Payments.Domain.Enums;
using FGC.Payments.Domain.Events;

namespace FGC.Payments.Domain.Entities
{
    public class Payment : AggregateRoot
    {
        #region [Properties]

        public Guid UserId { get; private set; }
        public Guid GameId { get; private set; }
        public decimal Amount { get; private set; }
        public PaymentStatus Status { get; private set; }
        public PaymentMethod Method { get; private set; }
        public string TransactionId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string FailureReason { get; private set; }

        #endregion

        #region [Constructor]

        private Payment() : base() { }

        private Payment(Guid userId, Guid gameId, decimal amount, PaymentMethod method) : base()
        {
            UserId = ValidateUserId(userId);
            GameId = ValidateGameId(gameId);
            Amount = ValidateAmount(amount);
            Method = method;
            Status = PaymentStatus.Pending;
            TransactionId = GenerateTransactionId();
            CreatedAt = DateTime.UtcNow;
        }

        #endregion

        #region [Factory Methods]

        public static Payment Create(Guid userId, Guid gameId, decimal amount, PaymentMethod method)
        {
            var payment = new Payment(userId, gameId, amount, method);

            payment.AddDomainEvent(new PaymentCreatedEvent(
                payment.Id,
                payment.UserId,
                payment.GameId,
                payment.Amount,
                payment.TransactionId,
                payment.CreatedAt
            ));

            return payment;
        }

        #endregion

        #region [Business Methods]

        public void StartProcessing()
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidOperationException($"Não é possível processar pagamento com status {Status}");

            Status = PaymentStatus.Processing;
            ProcessedAt = DateTime.UtcNow;

            AddDomainEvent(new PaymentProcessingEvent(Id, TransactionId, ProcessedAt.Value));
        }

        public void Complete()
        {
            if (Status != PaymentStatus.Processing)
                throw new InvalidOperationException($"Não é possível completar pagamento com status {Status}");

            Status = PaymentStatus.Completed;
            CompletedAt = DateTime.UtcNow;

            AddDomainEvent(new PaymentCompletedEvent(
                Id,
                UserId,
                GameId,
                Amount,
                TransactionId,
                CompletedAt.Value
            ));
        }

        public void Fail(string reason)
        {
            if (Status != PaymentStatus.Processing)
                throw new InvalidOperationException($"Não é possível falhar pagamento com status {Status}");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Motivo da falha é obrigatório", nameof(reason));

            Status = PaymentStatus.Failed;
            FailureReason = reason;

            AddDomainEvent(new PaymentFailedEvent(Id, TransactionId, reason, DateTime.UtcNow));
        }

        public void Refund()
        {
            if (Status != PaymentStatus.Completed)
                throw new InvalidOperationException($"Não é possível reembolsar pagamento com status {Status}");

            Status = PaymentStatus.Refunded;

            AddDomainEvent(new PaymentRefundedEvent(
                Id,
                UserId,
                Amount,
                TransactionId,
                DateTime.UtcNow
            ));
        }

        public void Cancel()
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidOperationException($"Não é possível cancelar pagamento com status {Status}");

            Status = PaymentStatus.Cancelled;

            AddDomainEvent(new PaymentCancelledEvent(Id, TransactionId, DateTime.UtcNow));
        }

        #endregion

        #region [Validações]

        private static Guid ValidateUserId(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId é obrigatório", nameof(userId));

            return userId;
        }

        private static Guid ValidateGameId(Guid gameId)
        {
            if (gameId == Guid.Empty)
                throw new ArgumentException("GameId é obrigatório", nameof(gameId));

            return gameId;
        }

        private static decimal ValidateAmount(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Valor deve ser maior que zero", nameof(amount));

            if (amount > 999999.99m)
                throw new ArgumentException("Valor excede o limite máximo", nameof(amount));

            return amount;
        }

        private static string GenerateTransactionId()
        {
            return $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        #endregion

        #region [Helpers]

        public bool IsPending => Status == PaymentStatus.Pending;
        public bool IsProcessing => Status == PaymentStatus.Processing;
        public bool IsCompleted => Status == PaymentStatus.Completed;
        public bool IsFailed => Status == PaymentStatus.Failed;
        public bool IsRefunded => Status == PaymentStatus.Refunded;
        public bool IsCancelled => Status == PaymentStatus.Cancelled;

        #endregion
    }
}