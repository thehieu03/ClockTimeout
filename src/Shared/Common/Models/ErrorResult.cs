namespace Common.Models;

public sealed class ErrorResult
{

    #region Fields, Properties and Indexers

    public string? ErrorMessage { get; set; }
    public object? Details { get; set; }
    #endregion
    #region Constructors
    public ErrorResult()
    {

    }
    public ErrorResult(string errorMessage, object? details = null)
    {
        ErrorMessage = errorMessage;
        Details = details;
    }

    #endregion
}
