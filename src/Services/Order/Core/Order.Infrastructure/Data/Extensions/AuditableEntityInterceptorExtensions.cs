using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Order.Infrastructure.Data.Extensions;

public static class AuditableEntityInterceptorExtensions
{

    #region Methods

    public static bool HasChangedComplexProperties(this EntityEntry entry) =>
        entry.ComplexProperties.Any(x => x.IsModified);

    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (
                r.TargetEntry.State == EntityState.Added ||
                r.TargetEntry.State == EntityState.Modified ||
                r.TargetEntry.ComplexProperties.Any(x => x.IsModified)
            )
        );

    #endregion
}