using FGC.Payments.Domain.Enums;

namespace FGC.Payments.Application.DTOs
{
    public class CreatePaymentDTO
    {
        public Guid UserId { get; set; }
        public Guid GameId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }

    public class ProcessPaymentDTO
    {
        public Guid PaymentId { get; set; }
    }

    public class RefundPaymentDTO
    {
        public Guid PaymentId { get; set; }
    }

    public class PaymentResponseDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid GameId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string FailureReason { get; set; }
    }

    public class PaymentStatusDTO
    {
        public Guid PaymentId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? ProcessedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string FailureReason { get; set; }
    }
}