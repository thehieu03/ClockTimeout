using BuildingBlocks.CQRS;
using Catalog.Application.Dtos.Brands;
using Catalog.Domain.Entities;
using Common.Constants;
using Common.Extensions;
using FluentValidation;
using Marten;

namespace Catalog.Application.Features.Brand.Commands;

public record class CreateBrandCommand(CreateBrandDto Dto, Actor Actor) : ICommand<Guid>;

public class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator()
    {
        RuleFor(x => x.Dto)
            .NotNull()
            .WithMessage(MessageCode.BadRequest)
            .DependentRules(() =>
            {
                RuleFor(x => x.Dto.Name)
                    .NotEmpty()
                    .WithMessage(MessageCode.BrandNameIsRequired)
                    .MaximumLength(200)
                    .WithMessage(MessageCode.BrandNameMaxLength);

                RuleFor(x => x.Actor)
                    .NotNull()
                    .WithMessage(MessageCode.ActorIsRequired);
            });
    }
}

public class CreateBrandCommandHandler : ICommandHandler<CreateBrandCommand, Guid>
{
    private readonly IDocumentSession _session;

    public CreateBrandCommandHandler(IDocumentSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public async Task<Guid> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        await _session.BeginTransactionAsync(cancellationToken);

        // Check if slug already exists
        var slug = dto.Name!.Slugify();
        var slugExists = await _session.Query<BrandEntity>()
            .AnyAsync(x => x.Slug == slug, cancellationToken);

        if (slugExists)
        {
            throw new ClientValidationException(MessageCode.BrandSlugAlreadyExists, slug);
        }

        var entity = BrandEntity.Create(
            id: Guid.NewGuid(),
            name: dto.Name!,
            slug: slug,
            performedBy: request.Actor.ToString()
        );

        _session.Store(entity);
        await _session.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
