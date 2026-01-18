namespace Common.Models.Reponses;

public sealed class ResultShareResponse<T> where T:class
{

    #region Fields, Properties and Indexers

    public T Data { get; set; } = default!;
    public string? Message { get; set; }
    public int StatusCode { get; set; }
    public string? Instance { get; set; }
    public List<ErrorResult>? Errors { get; set; }
    #endregion
    #region Constructors

    public ResultShareResponse()
    {
        
    }
    public ResultShareResponse(
        T data,
        string message,
        int statusCode,
        string? instance,
        List<ErrorResult>? errors)
    {
        Data = data;
        Message = message;
        StatusCode = statusCode;
        Instance = instance;
        Errors = errors;
    }
    public ResultShareResponse(int statusCode, string? instance, List<ErrorResult>? errors, string? message)
    {
        StatusCode = statusCode;
        Instance = instance;
        Errors = errors;
        Message = message;
    }
    #endregion
    public static ResultShareResponse<T> Failure(
        int statusCode =400,
        string? instance=null,
        List<ErrorResult>? errors=null,
        string? message=null
        )
    {
        return new ResultShareResponse<T>(statusCode, instance, errors, message);
    }
    public static ResultShareResponse<T> Success(T data, string message = "Success",string? instance=null)
    {
        return new ResultShareResponse<T>(data, message, 200, instance, null);
    }
}