using AutoMapper;
using Catalog.Application.Dtos.Products;
using Catalog.Domain.Entities;
using Catalog.Domain.Events;
using Common.Extensions;
using MediatR;

namespace Catalog.Application.Features.Product.Commands;

public record class UpdateProductCommand(Guid ProductId, UpdateProductDto Dto, Actor Actor) : ICommand<Guid>;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    #region Ctors

    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage(MessageCode.ProductIdIsRequired);

        RuleFor(x => x.Dto)
            .NotNull()
            .WithMessage(MessageCode.BadRequest)
            .DependentRules(() =>
            {
                RuleFor(x => x.Dto.Name)
                    .NotEmpty()
                    .WithMessage(MessageCode.ProductNameIsRequired)
                    .MaximumLength(200)
                    .WithMessage(MessageCode.ProductNameMaxLength);

                RuleFor(x => x.Dto.Sku)
                    .NotEmpty()
                    .WithMessage(MessageCode.SkuIsRequired)
                    .MaximumLength(100)
                    .WithMessage(MessageCode.SkuMaxLength);

                RuleFor(x => x.Dto.ShortDescription)
                    .NotEmpty()
                    .WithMessage(MessageCode.ShortDescriptionIsRequired)
                    .MaximumLength(500)
                    .WithMessage(MessageCode.ShortDescriptionMaxLength);

                RuleFor(x => x.Dto.LongDescription)
                    .NotEmpty()
                    .WithMessage(MessageCode.LongDescriptionIsRequired)
                    .MaximumLength(1000)
                    .WithMessage(MessageCode.LongDescriptionMaxLength);

                RuleFor(x => x.Dto.Price)
                    .NotEmpty()
                    .WithMessage(MessageCode.PriceIsRequired)
                    .GreaterThan(0)
                    .WithMessage(MessageCode.PriceMustBeGreaterThanZero);

                RuleFor(x => x.Dto.SalePrice)
                    .GreaterThan(0)
                    .When(x => x.Dto.SalePrice.HasValue)
                    .WithMessage(MessageCode.SalePriceMustBeGreaterThanZero);
            });
    }

    #endregion
}

public class UpdateProductCommandHandler(IDocumentSession session, IMapper mapper, IMediator mediator) : ICommandHandler<UpdateProductCommand, Guid>
{
    #region Implementations

    public async Task<Guid> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        await session.BeginTransactionAsync(cancellationToken);

        var entity = await session.LoadAsync<ProductEntity>(command.ProductId, cancellationToken)
                     ?? throw new ClientValidationException(MessageCode.ProductIsNotExists, command.ProductId);

        var dto = command.Dto;

        // Update basic product information
        entity.Update(
            name: dto.Name!,
            sku: dto.Sku!,
            shortDescription: dto.ShortDescription!,
            longDescription: dto.LongDescription!,
            price: dto.Price,
            salePrice: dto.SalePrice,
            performedBy: command.Actor.ToString()
        );

        // Update slug if name changed
        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            entity.Slug = dto.Name.Slugify();
        }

        // Update category IDs
        if (dto.CategoryIds != null)
        {
            entity.CategoryIds = dto.CategoryIds.Distinct().ToList();
        }

        // Update brand ID
        if (dto.BrandId.HasValue)
        {
            entity.BrandId = dto.BrandId;
        }

        // Update colors
        if (dto.Colors != null && dto.Colors.Any())
        {
            entity.UpdateColors(dto.Colors.Distinct().ToList(), command.Actor.ToString());
        }

        // Update sizes
        if (dto.Sizes != null && dto.Sizes.Any())
        {
            entity.UpdateSizes(dto.Sizes.Distinct().ToList(), command.Actor.ToString());
        }

        // Update tags
        if (dto.Tags != null && dto.Tags.Any())
        {
            entity.UpdateTags(dto.Tags.Distinct().ToList(), command.Actor.ToString());
        }

        // Update SEO
        if (!string.IsNullOrWhiteSpace(dto.SEOTitle) || !string.IsNullOrWhiteSpace(dto.SEODescription))
        {
            entity.UpdateSEO(dto.SEOTitle, dto.SEODescription, command.Actor.ToString());
        }

        // Update featured status
        entity.UpdateFeatured(dto.Featured, command.Actor.ToString());

        // Update barcode
        if (!string.IsNullOrWhiteSpace(dto.Barcode))
        {
            entity.UpdateBarcode(dto.Barcode, command.Actor.ToString());
        }

        // Update unit and weight
        if (!string.IsNullOrWhiteSpace(dto.Unit) || dto.Weight.HasValue)
        {
            entity.UpdateUnitAndWeight(dto.Unit, dto.Weight, command.Actor.ToString());
        }

        // Update published status
        if (dto.Published && !entity.Published)
        {
            entity.Publish(command.Actor.ToString());
        }
        else if (!dto.Published && entity.Published)
        {
            entity.Unpublish(command.Actor.ToString());
        }

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

    #endregion
}
