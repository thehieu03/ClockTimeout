namespace BuildingBlocks.Abstractions;
// define id
public interface IEntityId<T>
{
    T Id { get; set; }
}