using AutoMapper;
using Order.Application.Dtos.Orders;
using Order.Domain.Entities;

namespace Order.Application.Mappings;

public sealed class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateOrderMappings();
        CreateOrderItemMappings();
        CreateValueObjectMappings();
    }
    private void CreateValueObjectMappings()
    {
        // OrderEntity -> OrderDto
        CreateMap<OrderEntity, OrderDto>()
            .ForMember(dest => dest.OrderNo, opt => opt.MapFrom(src => src.OrderNo.Value))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CouponCode, opt => opt.MapFrom(src => src.Discount != null ? src.Discount.CouponCode : null))
            .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.Discount != null ? src.Discount.DiscountAmount : 0));
    }
    private void CreateOrderItemMappings()
    {
        // OrderItemEntity -> OrderItemDto
        CreateMap<OrderItemEntity, OrderItemDto>();
    }
    private void CreateOrderMappings()
    {
        // OrderEntity -> OrderDto
        CreateMap<OrderEntity, OrderDto>()
            .ForMember(dest => dest.OrderNo, opt => opt.MapFrom(src => src.OrderNo.Value))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CouponCode, opt => opt.MapFrom(src => src.Discount != null ? src.Discount.CouponCode : null))
            .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.Discount != null ? src.Discount.DiscountAmount : 0));
    }
}