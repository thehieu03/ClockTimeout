using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Data.Configurations;

public class PaymentWebhookLogConfiguration: IEntityTypeConfiguration<PaymentWebhookLog>
{

    public void Configure(EntityTypeBuilder<PaymentWebhookLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x=>x.Gateway).IsRequired().HasMaxLength(50);
        builder.Property(x=>x.RequestId).HasMaxLength(100);
        builder.HasIndex(x => new { x.Gateway, x.RequestId });
    }
}