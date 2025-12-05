using FGC.Payments.Domain.Entities;
using FGC.Payments.Domain.Enums;
using FGC.Payments.Domain.Interfaces;
using FGC.Payments.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FGC.Payments.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentsDbContext _context;

        public PaymentRepository(PaymentsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Payment> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Payment> GetByTransactionIdAsync(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
                return null;

            return await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        }

        public async Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                return Enumerable.Empty<Payment>();

            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByGameIdAsync(Guid gameId)
        {
            if (gameId == Guid.Empty)
                return Enumerable.Empty<Payment>();

            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.GameId == gameId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.Status == PaymentStatus.Pending)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveAsync(Payment payment)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            var existingPayment = await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == payment.Id);

            if (existingPayment == null)
            {
                await _context.Payments.AddAsync(payment);
            }
            else
            {
                _context.Payments.Update(payment);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            if (id == Guid.Empty)
                return false;

            return await _context.Payments
                .AsNoTracking()
                .AnyAsync(p => p.Id == id);
        }
    }
}