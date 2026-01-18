namespace BuildingBlocks.Extensions;

public sealed class ClientValidationException:Exception
{

    #region Fields, Properties and Indexers
        public object? Details { get; }

    #endregion

    #region Constructors

    public ClientValidationException(string message):base(message)
    {
    }
    public ClientValidationException(string message, object? details):base(message)
    {
        
    }

    #endregion
}