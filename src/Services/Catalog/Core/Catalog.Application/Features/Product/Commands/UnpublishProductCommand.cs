using Catalog.Domain.Entities;
using Catalog.Domain.Events;
using MediatR;

namespace Catalog.Application.Features.Product.Commands;

public record class UnpublishProductCommand(Guid ProductId, Actor Actor) : ICommand<Guid>;

public class UnpublishProductCommandValidator : AbstractValidator<UnpublishProductCommand>
{
    #region Ctors

    public UnpublishProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage(MessageCode.ProductIdIsRequired);
    }

    #endregion
}

public class UnpublishProductCommandHandler(IDocumentSession session, IMediator mediator) : ICommandHandler<UnpublishProductCommand, Guid>
{
    #region Implementations

    public async Task<Guid> Handle(UnpublishProductCommand command, CancellationToken cancellationToken)
    {
        await session.BeginTransactionAsync(cancellationToken);

        var entity = await session.LoadAsync<ProductEntity>(command.ProductId, cancellationToken)
                     ?? throw new ClientValidationException(MessageCode.ProductIsNotExists, command.ProductId);

        entity.Unpublish(command.Actor.ToString());
        session.Store(entity);

        var @event = new DeletedUnPublishedProductDomainEvent(entity.Id);

        await mediator.Publish(@event, cancellationToken);
        await session.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    #endregion
}
