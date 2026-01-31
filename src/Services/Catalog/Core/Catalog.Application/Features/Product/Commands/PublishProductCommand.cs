namespace Catalog.Application.Features.Product.Commands;

public record PublishProductCommand(Guid ProductId, Actor Actor) : ICommand<Guid>;

public class PublishProductCommandValidator : AbstractValidator<PublishProductCommand>
{
    public PublishProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage(MessageCode.ProductIdIsRequired);
    }
}

public class PublishProductCommandHandler(IDocumentSession session, IMediator mediator) : ICommandHandler<PublishProductCommand, Guid>
{
    public async Task<Guid> Handle(PublishProductCommand command, CancellationToken cancellationToken)
    {
        await session.BeginTransactionAsync(cancellationToken);

        var entity = await session.LoadAsync<ProductEntity>(command.ProductId, cancellationToken)
                     ?? throw new ClientValidationException(MessageCode.ProductIsNotExists, command.ProductId);

        entity.Publish(command.Actor.ToString());
        session.Store(entity);

        var @event = new UpsertedProductDomainEvent(
            entity.Id,
            entity.Name!,
            entity.Sku!,
            entity.Slug!,
            entity.Price,
            entity.SalePrice,
            entity.CategoryIds?.Select(id => id.ToString()).ToList(),
            entity.Images?.Select(img => img.PublicURL).Where(url => !string.IsNullOrWhiteSpace(url)).Cast<string>().ToList(),
            entity.Thumbnail?.PublicURL!,
            entity.Status,
            entity.CreatedOnUtc,
            entity.CreatedBy!,
            entity.LastModifiedOnUtc,
            entity.LastModifiedBy);

        await mediator.Publish(@event, cancellationToken);
        await session.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}