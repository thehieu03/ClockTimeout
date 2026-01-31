using BuildingBlocks.CQRS;
using Common.Constants;
using Common.ValueObjects;
using FluentValidation;
using Order.Application.Dtos.Orders;
using Order.Domain.Abstractions;
using Order.Domain.Entities;
using Order.Domain.ValueObjects;

namespace Order.Application.Features.Order.Commands;

public record CreateOrderCommand(CreateOrUpdateOrderDto Dto, Actor Actor) : ICommand<Guid>;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    #region Ctors

    public CreateOrderCommandValidator()
    {
        // Validate Dto không null
        RuleFor(x => x.Dto)
            .NotNull()
            .WithMessage(MessageCode.BadRequest)
            .DependentRules(() =>
            {
                // Validate Customer (nested object)
                RuleFor(x => x.Dto.Customer)
                    .NotNull()
                    .WithMessage(MessageCode.BadRequest)
                    .DependentRules(() =>
                    {
                        RuleFor(x => x.Dto.Customer.PhoneNumber)
                            .NotEmpty()
                            .WithMessage(MessageCode.PhoneNumberIsRequired);

                        RuleFor(x => x.Dto.Customer.Name)
                            .NotEmpty()
                            .WithMessage(MessageCode.NameIsRequired);

                        RuleFor(x => x.Dto.Customer.Email)
                            .NotEmpty()
                            .WithMessage(MessageCode.EmailIsRequired)
                            .EmailAddress()
                            .WithMessage(MessageCode.InvalidEmailAddress);
                    });

                // Validate ShippingAddress (nested object)
                RuleFor(x => x.Dto.ShippingAddress)
                    .NotNull()
                    .WithMessage(MessageCode.BadRequest)
                    .DependentRules(() =>
                    {
                        RuleFor(x => x.Dto.ShippingAddress.AddressLine)
                            .NotEmpty()
                            .WithMessage(MessageCode.AddressLineIsRequired);

                        RuleFor(x => x.Dto.ShippingAddress.City)
                            .NotEmpty()
                            .WithMessage(MessageCode.CityIsRequired);

                        RuleFor(x => x.Dto.ShippingAddress.Country)
                            .NotEmpty()
                            .WithMessage(MessageCode.CountryIsRequired);
                    });

                // Validate Order Items
                RuleFor(x => x.Dto.OrderItems)
                    .NotEmpty()
                    .WithMessage(MessageCode.OrderItemsIsRequired);

                // Validate each Order Item
                RuleForEach(x => x.Dto.OrderItems)
                    .ChildRules(item =>
                    {
                        item.RuleFor(x => x.ProductId)
                            .NotEmpty()
                            .WithMessage(MessageCode.ProductIdIsRequired);

                        item.RuleFor(x => x.Quantity)
                            .GreaterThan(0)
                            .WithMessage(MessageCode.QuantityMustBeGreaterThanZero);
                    });
            });

        // Validate Actor
        RuleFor(x => x.Actor)
            .NotNull()
            .WithMessage(MessageCode.ActorIsRequired);
    }
    
    #endregion
}

public class CreateOrderCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreateOrderCommand, Guid>
{

    #region Implementations

    public async Task<Guid> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Dto;
        using var transaction =await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Create Customer value object (nested structure)
            var customer=Customer.Of(
                id:dto.Customer.Id,
                phoneNumber:dto.Customer.PhoneNumber,
                email:dto.Customer.Email,
                name:dto.Customer.Name);
            var shippingAddress = Address.Of(
                addressLine: dto.ShippingAddress.AddressLine,
                subdivision: dto.ShippingAddress.Subdivision,
                city: dto.ShippingAddress.City,
                country: dto.ShippingAddress.Country,
                stateOrProvince: dto.ShippingAddress.StateOrProvince,
                postalCode:dto.ShippingAddress.PostalCode
                );
            var orderNo = OrderNo.Create();
            var orderId=Guid.NewGuid();
            var order = OrderEntity.Create(
                id: orderId,
                customer: customer,
                orderNo: orderNo,
                shippingAddress: shippingAddress,
                notes: dto.Notes,
                performedBy:command.Actor.ToString()
            );
            foreach (var itemDto in dto.OrderItems)
            {
                var product = Product.Of(
                    id: itemDto.ProductId,
                    name: itemDto.ProductName,
                    price: itemDto.ProductPrice,
                    imageUrl: itemDto.ProductImageUrl ?? string.Empty
                );
                order.AddOrderItem(product, itemDto.Quantity);
            }
            if (!string.IsNullOrWhiteSpace(dto.CouponCode))
            {
                var discount = Discount.Of(dto.CouponCode, 0);
                order.ApplyDiscount(discount);
            }
            // Save to database
            await unitOfWork.Orders.AddAsync(order, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return orderId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    #endregion
}