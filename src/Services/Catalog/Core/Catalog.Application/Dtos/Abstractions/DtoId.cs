namespace Catalog.Application.Dtos.Abstractions;

public class DtoId<T> : IDtoId<T>
{
    public T Id { get; init; } = default!;

}
public interface IDtoId<T>
{
    T Id { get; init; }
}
