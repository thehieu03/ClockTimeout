namespace Common.Models;

public sealed class ErrorResult
{
    public string? ErrorMessage { get; set; }
    public object? Details { get; set; }
    public ErrorResult()
    {

    }
    public ErrorResult(string errorMessage, object? details = null)
    {
        ErrorMessage = errorMessage;
        Details = details;
    }
}
