using AutoMapper;
using BuildingBlocks.CQRS;
using Catalog.Application.Dtos;
using Catalog.Domain.Entities;
using Common.Constants;
using Common.Extensions;
using FluentValidation;
using Marten;

namespace Catalog.Application.Features.Product.Commands;

public record class CreateProductCommand(CreateProductDto Dto, Actor Actor) : ICommand<Guid>
{

}
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
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
            .NotEmpty()
            .WithMessage(MessageCode.SalePriceMustBeGreaterThanZero)
            .GreaterThan(0)
            .WithMessage(MessageCode.SalePriceMustBeGreaterThanZero);
            RuleFor(x => x.Actor)
            .NotEmpty()
            .WithMessage(MessageCode.ActorIsRequired);
            RuleFor(x => x.Actor)
            .NotNull()
            .WithMessage(MessageCode.ActorIsRequired);
        });
    }
}
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IDocumentSession _session;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(IDocumentSession session, IMapper mapper)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        await _session.BeginTransactionAsync(cancellationToken);
        var entity = ProductEntity.Create(
            id: Guid.NewGuid(),
            name: dto.Name!,
            sku: dto.Sku!,
            shortDescription: dto.ShortDescription!,
            longDescription: dto.LongDescription!,
            slug: dto.Name!.Slugify(),
            price: dto.Price,
            salePrice: dto.SalePrice,
            categoryIds: dto.CategoryIds?.Distinct().ToList(),
            brandId: dto.BrandId,
            performedBy: request.Actor.ToString()
        );
        if (dto.Colors != null && dto.Colors.Any())
        {
            entity.UpdateColors(dto.Colors.Distinct().ToList(), request.Actor.ToString());
        }
        if (dto.Sizes != null && dto.Sizes.Any())
        {
            entity.UpdateSizes(dto.Sizes.Distinct().ToList(), request.Actor.ToString());
        }
        if (dto.Tags != null && dto.Tags.Any())
        {
            entity.UpdateTags(dto.Tags.Distinct().ToList(), request.Actor.ToString());
        }
        if (!string.IsNullOrWhiteSpace(dto.SEOTitle) || !string.IsNullOrWhiteSpace(dto.SEODescription))
        {
            entity.UpdateSEO(dto.SEOTitle, dto.SEODescription, request.Actor.ToString());
        }
        if (dto.Featured)
        {
            entity.UpdateFeatured(dto.Featured, request.Actor.ToString());
        }
        if (!string.IsNullOrWhiteSpace(dto.Barcode))
        {
            entity.UpdateBarcode(dto.Barcode, request.Actor.ToString());
        }
        if (!string.IsNullOrWhiteSpace(dto.Unit) || dto.Weight.HasValue)
        {
            entity.UpdateUnitAndWeight(dto.Unit, dto.Weight, request.Actor.ToString());
        }
        if (dto.Published)
        {
            entity.Publish(request.Actor.ToString());
        }
        // Store entity in database
        _session.Store(entity);
        // Save change
        await _session.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
