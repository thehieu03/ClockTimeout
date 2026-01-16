namespace BuildingBlocks.Abstractions;
// check account create entity
public interface ICreationAuditable
{
    DateTimeOffset CreatedOnUtc { get; set; }
    string? CreatedBy { get; set; }
}