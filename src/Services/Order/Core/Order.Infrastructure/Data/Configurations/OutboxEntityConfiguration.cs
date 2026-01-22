using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;

namespace Order.Infrastructure.Data.Configurations;

public class OutboxEntityConfiguration:IEntityTypeConfiguration<OutBoxMessageEntity>
{

    public void Configure(EntityTypeBuilder<OutBoxMessageEntity> builder)
    {
        throw new NotImplementedException();
    }
}