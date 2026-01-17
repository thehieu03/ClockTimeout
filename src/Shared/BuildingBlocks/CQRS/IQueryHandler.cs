using MediatR;

namespace BuildingBlocks.CQRS;

public interface IQueryHandler<in TQuery,TResponse>:IRequestHandler<TQuery,TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : notnull
{
}




// Test


public record GetProductQuery(Guid Id) : IQuery<ProductDto>;
public record ProductDto(Guid Id, string Name);

public class GetProductQueryHandler : IQueryHandler<GetProductQuery, ProductDto>
{
    public Task<ProductDto> Handle(GetProductQuery query, CancellationToken ct)
        => Task.FromResult(new ProductDto(query.Id, "Test"));
}