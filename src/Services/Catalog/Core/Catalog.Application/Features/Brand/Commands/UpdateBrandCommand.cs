using BuildingBlocks.CQRS;
using Catalog.Application.Dtos.Brands;
using Catalog.Domain.Entities;
using Common.Constants;
using Common.Extensions;
using FluentValidation;
using Marten;

namespace Catalog.Application.Features.Brand.Commands;

public record class UpdateBrandCommand(Guid BrandId, UpdateBrandDto Dto, Actor Actor) : ICommand<Guid>;

public class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandValidator()
    {
        RuleFor(x => x.BrandId)
            .NotEmpty()
            .WithMessage(MessageCode.BrandIdIsRequired);

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

public class UpdateBrandCommandHandler : ICommandHandler<UpdateBrandCommand, Guid>
{
    private readonly IDocumentSession _session;

    public UpdateBrandCommandHandler(IDocumentSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public async Task<Guid> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        await _session.BeginTransactionAsync(cancellationToken);

        var entity = await _session.LoadAsync<BrandEntity>(request.BrandId, cancellationToken)
                     ?? throw new ClientValidationException(MessageCode.BrandNotFound);

        var dto = request.Dto;

        // Check if slug already exists (excluding current brand)
        var slug = dto.Name!.Slugify();
        if (slug != entity.Slug)
        {
            var slugExists = await _session.Query<BrandEntity>()
                .AnyAsync(x => x.Slug == slug && x.Id != request.BrandId, cancellationToken);

            if (slugExists)
            {
                throw new ClientValidationException(MessageCode.BrandSlugAlreadyExists, slug);
            }
        }

        entity.Update(
            name: dto.Name!,
            slug: slug,
            performedBy: request.Actor.ToString()
        );

        _session.Store(entity);
        await _session.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
