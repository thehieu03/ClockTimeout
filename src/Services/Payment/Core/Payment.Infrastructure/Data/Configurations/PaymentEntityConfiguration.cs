using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Data.Configurations;

public sealed class PaymentEntityConfiguration : IEntityTypeConfiguration<PaymentEntity>
{
    public void Configure(EntityTypeBuilder<PaymentEntity> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderId)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Method)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedOnUtc)
            .IsRequired();

        builder.Property(x => x.TransactionId)
            .HasMaxLength(100);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(500);

        builder.Property(x => x.RefundReason)
            .HasMaxLength(500);

        builder.Property(x => x.RefundTransactionId)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(255);

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(255);

        // Index for faster order lookup
        builder.HasIndex(x => x.OrderId);

        // Index for status filtering
        builder.HasIndex(x => x.Status);
    }
}
