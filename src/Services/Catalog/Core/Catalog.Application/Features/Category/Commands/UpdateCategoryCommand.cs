using BuildingBlocks.CQRS;
using Catalog.Application.Dtos.Categories;
using Catalog.Domain.Entities;
using Common.Constants;
using Common.Extensions;
using FluentValidation;
using Marten;

namespace Catalog.Application.Features.Category.Commands;

public record class UpdateCategoryCommand(Guid CategoryId, UpdateCategoryDto Dto, Actor Actor) : ICommand<Guid>;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage(MessageCode.CategoryIdIsRequired);

        RuleFor(x => x.Dto)
            .NotNull()
            .WithMessage(MessageCode.BadRequest)
            .DependentRules(() =>
            {
                RuleFor(x => x.Dto.Name)
                    .NotEmpty()
                    .WithMessage(MessageCode.CategoryNameIsRequired)
                    .MaximumLength(200)
                    .WithMessage(MessageCode.CategoryNameMaxLength);

                RuleFor(x => x.Dto.Description)
                    .MaximumLength(1000)
                    .When(x => !string.IsNullOrWhiteSpace(x.Dto.Description))
                    .WithMessage(MessageCode.CategoryDescriptionMaxLength);

                RuleFor(x => x.Actor)
                    .NotNull()
                    .WithMessage(MessageCode.ActorIsRequired);
            });
    }
}

public class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand, Guid>
{
    private readonly IDocumentSession _session;

    public UpdateCategoryCommandHandler(IDocumentSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public async Task<Guid> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        await _session.BeginTransactionAsync(cancellationToken);

        var entity = await _session.LoadAsync<CategoryEntity>(request.CategoryId, cancellationToken)
                     ?? throw new ClientValidationException(MessageCode.CategoryNotFound);

        var dto = request.Dto;

        // Validate parent exists if provided and is not self
        if (dto.ParentId.HasValue)
        {
            if (dto.ParentId.Value == request.CategoryId)
            {
                throw new ClientValidationException(MessageCode.CategoryCannotBeParentOfItself);
            }

            var parentExists = await _session.Query<CategoryEntity>()
                .AnyAsync(x => x.Id == dto.ParentId.Value, cancellationToken);

            if (!parentExists)
            {
                throw new ClientValidationException(MessageCode.CategoryParentNotFound, dto.ParentId.Value.ToString());
            }

            // Check for circular reference: parent cannot be a descendant of this category
            var isDescendant = await IsDescendantAsync(dto.ParentId.Value, request.CategoryId, cancellationToken);
            if (isDescendant)
            {
                throw new ClientValidationException(MessageCode.CategoryCircularReference);
            }
        }

        // Check if slug already exists (excluding current category)
        var slug = dto.Name!.Slugify();
        if (slug != entity.Slug)
        {
            var slugExists = await _session.Query<CategoryEntity>()
                .AnyAsync(x => x.Slug == slug && x.Id != request.CategoryId, cancellationToken);

            if (slugExists)
            {
                throw new ClientValidationException(MessageCode.CategorySlugAlreadyExists, slug);
            }
        }

        entity.Update(
            name: dto.Name!,
            description: dto.Description ?? string.Empty,
            slug: slug,
            performedBy: request.Actor.ToString(),
            parentId: dto.ParentId
        );

        _session.Store(entity);
        await _session.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    private async Task<bool> IsDescendantAsync(Guid potentialParentId, Guid categoryId, CancellationToken cancellationToken)
    {
        var current = await _session.LoadAsync<CategoryEntity>(potentialParentId, cancellationToken);
        if (current == null) return false;

        while (current.ParentId.HasValue)
        {
            if (current.ParentId.Value == categoryId)
                return true;

            current = await _session.LoadAsync<CategoryEntity>(current.ParentId.Value, cancellationToken);
            if (current == null) break;
        }

        return false;
    }
}
