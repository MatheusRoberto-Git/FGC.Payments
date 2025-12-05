using FGC.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FGC.Payments.Infrastructure.Data.Context
{
    public class PaymentsDbContext : DbContext
    {
        public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

        public PaymentsDbContext() : base() { }

        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentsDbContext).Assembly);
        }
    }
}