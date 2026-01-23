namespace Order.Application.Dtos.AuditableDto;

public interface ICreationAuditDto
{
    #region Fields, Properties and Indexers

    DateTimeOffset CreatedOnUtc { get; set; }

    string? CreatedBy { get; set; }

    #endregion
}
