using FGC.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FGC.Payments.Infrastructure.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id")
                .IsRequired();

            builder.Property(p => p.UserId)
                .HasColumnName("UserId")
                .IsRequired();

            builder.Property(p => p.GameId)
                .HasColumnName("GameId")
                .IsRequired();

            builder.Property(p => p.Amount)
                .HasColumnName("Amount")
                .HasColumnType("DECIMAL(18,2)")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.Status)
                .HasConversion<int>()
                .HasColumnName("Status")
                .HasColumnType("INT")
                .IsRequired();

            builder.Property(p => p.Method)
                .HasConversion<int>()
                .HasColumnName("Method")
                .HasColumnType("INT")
                .IsRequired();

            builder.Property(p => p.TransactionId)
                .HasColumnName("TransactionId")
                .HasColumnType("NVARCHAR(50)")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .HasColumnName("CreatedAt")
                .HasColumnType("DATETIME2")
                .IsRequired();

            builder.Property(p => p.ProcessedAt)
                .HasColumnName("ProcessedAt")
                .HasColumnType("DATETIME2")
                .IsRequired(false);

            builder.Property(p => p.CompletedAt)
                .HasColumnName("CompletedAt")
                .HasColumnType("DATETIME2")
                .IsRequired(false);

            builder.Property(p => p.FailureReason)
                .HasColumnName("FailureReason")
                .HasColumnType("NVARCHAR(500)")
                .HasMaxLength(500)
                .IsRequired(false);

            // Indexes
            builder.HasIndex(p => p.TransactionId)
                .IsUnique()
                .HasDatabaseName("IX_Payments_TransactionId");

            builder.HasIndex(p => p.UserId)
                .HasDatabaseName("IX_Payments_UserId");

            builder.HasIndex(p => p.GameId)
                .HasDatabaseName("IX_Payments_GameId");

            builder.HasIndex(p => p.Status)
                .HasDatabaseName("IX_Payments_Status");

            builder.HasIndex(p => p.CreatedAt)
                .HasDatabaseName("IX_Payments_CreatedAt");

            builder.Ignore(p => p.DomainEvents);
        }
    }
}
