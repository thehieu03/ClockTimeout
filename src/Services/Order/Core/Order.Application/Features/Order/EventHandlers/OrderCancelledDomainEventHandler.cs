//using Amazon.Runtime.Internal.Util;
//using MediatR;
//using Microsoft.Extensions.Logging;
//using Order.Domain.Abstractions;
//using Order.Domain.Events;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Order.Application.Features.Order.EventHandlers;

//public sealed class OrderCancelledDomainEventHandler(IUnitOfWork unitOfWork, ILogger<OrderCancelledDomainEventHandler> logger) : INotificationHandler<OrderCancelledDomainEvent>
//{
//    public async Task Handle(OrderCancelledDomainEvent @event, CancellationToken cancellationToken)
//    {
//        logger.LogInformation("Domain Event handled: {DomainEvent}", @event.GetType().Name);
//        await PushToOutboxAsync(@event, cancellationToken);
//    }

//    private async Task PushToOutboxAsync(OrderCancelledDomainEvent @event, CancellationToken cancellationToken)
//    {
//        var reason = string.IsNullOrEmpty(@event.Order.RefundReason) ? @event.Order.CancelReason : @event.Order.RefundReason;
//        var message = new OrderCancelledIntegrationEvent()
//        {
//            Id = Guid.NewGuid().ToString(),
//            OrderId = @event.Order.Id,
//            OrderNo = @event.Order.OrderNo.ToString(),
//            Reason = reason!,
//            OrderItems = @event.Order.OrderItems.Select(oi => new OrderItemIntegrationEvent
//            {
//                ProductId = oi.Product.Id,
//                Quantity = oi.Quantity,
//                UnitPrice = oi.Product.Price,
//                LineTotal = oi.LineTotal
//            }).ToList(),
//        };
//        var outboxMessage = OutboxMessageEntity.Create(
//            id: Guid.NewGuid(),
//            eventType: message.EventType!,
//            content: JsonConvert.SerializeObject(message),
//            occurredOnUtc: DateTimeOffset.UtcNow);

//        await unitOfWork.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
//    }
//}
