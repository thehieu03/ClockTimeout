namespace Catalog.Domain.Abstractions;

public interface IEntityId<T>
{
    public T Id { get; set; }
}