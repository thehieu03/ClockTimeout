namespace Common.Models.Reponses;

public sealed class ApiPerformedResponse<T>
{
    public T Result { get; set; } = default!;
    public ApiPerformedResponse(){
    }
    public ApiPerformedResponse(T result) => Result = result;
}