using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Order.Domain.Abstractions;
using Order.Infrastructure.Data.Extensions;

namespace Order.Infrastructure.Data.Interceptors;

public sealed class AuditableEntityInterceptor:SaveChangesInterceptor
{

    #region Override Methods

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
    private void UpdateEntities(DbContext? eventDataContext)
    {
        if(eventDataContext==null) return;
        foreach (var item in eventDataContext.ChangeTracker.Entries<IAuditable>())
        {
            if (item.State == EntityState.Added)
            {
                // For new entities, set both created and modified timestamps
                item.Entity.CreatedOnUtc= DateTimeOffset.UtcNow;
                item.Entity.LastModifiedOnUtc = DateTimeOffset.UtcNow;
            }else if (item.State == EntityState.Modified || item.HasChangedOwnedEntities())
            {
                // For existing entities, set modified timestamp only
                item.Entity.LastModifiedOnUtc = DateTimeOffset.UtcNow;
            }
        }
    }

    #endregion
}