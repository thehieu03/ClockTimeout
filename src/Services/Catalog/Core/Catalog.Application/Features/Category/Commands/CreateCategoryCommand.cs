using BuildingBlocks.CQRS;
using Catalog.Application.Dtos.Categories;
using Catalog.Domain.Entities;
using Common.Constants;
using Common.Extensions;
using FluentValidation;
using Marten;

namespace Catalog.Application.Features.Category.Commands;

public record class CreateCategoryCommand(CreateCategoryDto Dto, Actor Actor) : ICommand<Guid>;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
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

public class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, Guid>
{
    private readonly IDocumentSession _session;

    public CreateCategoryCommandHandler(IDocumentSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        await _session.BeginTransactionAsync(cancellationToken);

        // Validate parent exists if provided
        if (dto.ParentId.HasValue)
        {
            var parentExists = await _session.Query<CategoryEntity>()
                .AnyAsync(x => x.Id == dto.ParentId.Value, cancellationToken);

            if (!parentExists)
            {
                throw new ClientValidationException(MessageCode.CategoryParentNotFound);
            }
        }

        // Check if slug already exists
        var slug = dto.Name!.Slugify();
        var slugExists = await _session.Query<CategoryEntity>()
            .AnyAsync(x => x.Slug == slug, cancellationToken);

        if (slugExists)
        {
            throw new ClientValidationException(MessageCode.CategorySlugAlreadyExists, slug);
        }

        var entity = CategoryEntity.Create(
            id: Guid.NewGuid(),
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
}
