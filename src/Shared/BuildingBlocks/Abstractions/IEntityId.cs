namespace BuildingBlocks.Abstractions;

public interface IEntityId<T>
{
    T Id { get; set; }    
}