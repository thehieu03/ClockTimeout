using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;

namespace Order.Infrastructure.Data.Configurations;

public class OutboxEntityConfiguration : IEntityTypeConfiguration<OutBoxMessageEntity>
{
    public void Configure(EntityTypeBuilder<OutBoxMessageEntity> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .IsRequired();

        builder.Property(x => x.Content)
            .HasColumnName("content")
            .IsRequired();

        builder.Property(x => x.OccurredOnUtc)
            .HasColumnName("occurred_on_utc")
            .IsRequired();

        builder.Property(x => x.ProcessedOnUtc)
            .HasColumnName("processed_on_utc");

        builder.Property(x => x.LastErrorMessage)
            .HasColumnName("last_error_message");

        builder.Property(x => x.AttemptCount)
            .HasColumnName("attempt_count")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.MaxAttempts)
            .HasColumnName("max_attempts")
            .HasDefaultValue(3)
            .IsRequired();

        builder.Property(x => x.NextAttemptOnUtc)
            .HasColumnName("next_attempt_on_utc");

        builder.HasIndex(x => x.ProcessedOnUtc);
        
        builder.HasIndex(x => new
        {
            x.ProcessedOnUtc,
            x.AttemptCount,
            x.MaxAttempts
        });
    }
}