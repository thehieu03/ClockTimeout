namespace Common.Models.Reponses;

public sealed class ApiGetResponse<T>
{

    #region Fields, Properties and Indexers

    public T Result { get; set; } = default!;

    #endregion

    #region Contructors

    public ApiGetResponse() { }
    public ApiGetResponse(T result) => Result = result;

    #endregion
}
