namespace Order.Domain.Abstractions;

public interface IEntityId<T>
{

    #region Methods
    public T Id { get; }
    

    #endregion
}