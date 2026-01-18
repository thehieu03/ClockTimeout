namespace Common.Models.Reponses;

public sealed class ApiUpdatedResponse<T>
{

    #region Fields, Properties and Indexers

    public T Value { get; set; } = default!;

    #endregion

    #region Constructors

    public ApiUpdatedResponse()
    {
    }
    public ApiUpdatedResponse(T value) => Value = value;

    #endregion

}