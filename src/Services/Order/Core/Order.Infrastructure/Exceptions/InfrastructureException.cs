namespace Order.Infrastructure.Exceptions;

public sealed class InfrastructureException:Exception
{
    # region Constructors
    public InfrastructureException(string message):base(message)
    {
        
    }
    #endregion
}