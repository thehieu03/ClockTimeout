namespace Order.Application.Dtos.AuditableDto;

public interface IModificationAuditDto
{
    #region Fields, Properties and Indexers

    DateTimeOffset? LastModifiedOnUtc { get; set; }

    string? LastModifiedBy { get; set; }

    #endregion
}
