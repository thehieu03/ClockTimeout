namespace Common.Models.Reponses;

public sealed class ApiDeletedResponse<T>
{
    public T Value { get; set; } = default!;
    public ApiDeletedResponse() { }
    public ApiDeletedResponse(T value) => Value = value;
    
}