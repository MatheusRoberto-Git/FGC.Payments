using FGC.Payments.Domain.Entities;
using FGC.Payments.Domain.Enums;

namespace FGC.Payments.Domain.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment> GetByIdAsync(Guid id);
        Task<Payment> GetByTransactionIdAsync(string transactionId);
        Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Payment>> GetByGameIdAsync(Guid gameId);
        Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status);
        Task<IEnumerable<Payment>> GetPendingPaymentsAsync();
        Task SaveAsync(Payment payment);
        Task<bool> ExistsAsync(Guid id);
    }
}