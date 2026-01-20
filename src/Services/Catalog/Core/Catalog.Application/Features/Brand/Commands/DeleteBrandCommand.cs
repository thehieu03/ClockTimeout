using BuildingBlocks.CQRS;
using Catalog.Domain.Entities;
using Common.Constants;
using FluentValidation;
using Marten;

namespace Catalog.Application.Features.Brand.Commands;

public record class DeleteBrandCommand(Guid BrandId) : ICommand<Unit>;

public class DeleteBrandCommandValidator : AbstractValidator<DeleteBrandCommand>
{
    public DeleteBrandCommandValidator()
    {
        RuleFor(x => x.BrandId)
            .NotEmpty()
            .WithMessage(MessageCode.BrandIdIsRequired);
    }
}

public class DeleteBrandCommandHandler : ICommandHandler<DeleteBrandCommand, Unit>
{
    private readonly IDocumentSession _session;

    public DeleteBrandCommandHandler(IDocumentSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public async Task<Unit> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var entity = await _session.LoadAsync<BrandEntity>(request.BrandId, cancellationToken)
                     ?? throw new ClientValidationException(MessageCode.BrandNotFound);

        // Check if brand is used by any products
        var isUsedByProducts = await _session.Query<ProductEntity>()
            .AnyAsync(x => x.BrandId.HasValue && x.BrandId.Value == request.BrandId, cancellationToken);

        if (isUsedByProducts)
        {
            throw new ClientValidationException(MessageCode.BrandIsUsedByProducts);
        }

        _session.Delete<BrandEntity>(entity.Id);
        await _session.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
