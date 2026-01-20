using BuildingBlocks.CQRS;
using Catalog.Domain.Entities;
using Common.Constants;
using FluentValidation;
using Marten;

namespace Catalog.Application.Features.Category.Commands;

public record class DeleteCategoryCommand(Guid CategoryId) : ICommand<Unit>;

public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage(MessageCode.CategoryIdIsRequired);
    }
}

public class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand, Unit>
{
    private readonly IDocumentSession _session;

    public DeleteCategoryCommandHandler(IDocumentSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _session.LoadAsync<CategoryEntity>(request.CategoryId, cancellationToken)
                     ?? throw new ClientValidationException(MessageCode.CategoryNotFound);

        // Check if category has children
        var hasChildren = await _session.Query<CategoryEntity>()
            .AnyAsync(x => x.ParentId == request.CategoryId, cancellationToken);

        if (hasChildren)
        {
            throw new ClientValidationException(MessageCode.CategoryHasChildren);
        }

        // Check if category is used by any products
        var isUsedByProducts = await _session.Query<ProductEntity>()
            .AnyAsync(x => x.CategoryIds != null && x.CategoryIds.Contains(request.CategoryId), cancellationToken);

        if (isUsedByProducts)
        {
            throw new ClientValidationException(MessageCode.CategoryIsUsedByProducts);
        }

        _session.Delete<CategoryEntity>(entity.Id);
        await _session.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
