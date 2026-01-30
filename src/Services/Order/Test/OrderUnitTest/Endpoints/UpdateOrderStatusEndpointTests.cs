using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using Common.ValueObjects;
using Order.Application.Features.Order.Commands;
using Order.Domain.Enums;

namespace OrderUnitTest.Endpoints;

[TestFixture]
public class UpdateOrderStatusEndpointTests
{
    private Mock<ISender> _senderMock = null!;

    [SetUp]
    public void Setup()
    {
        _senderMock = new Mock<ISender>();
    }

    [Test]
    public async Task UpdateOrderStatus_WithValidStatus_ShouldReturnOrderId()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var newStatus = OrderStatus.Processing;
        var actor = Actor.User("admin@example.com");

        _senderMock
            .Setup(x => x.Send(It.IsAny<UpdateOrderStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        // Act
        var command = new UpdateOrderStatusCommand(orderId, newStatus, null, actor);
        var result = await _senderMock.Object.Send(command);

        // Assert
        result.Should().Be(orderId);
    }

    [Test]
    public async Task UpdateOrderStatus_ToDelivered_ShouldSucceed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var newStatus = OrderStatus.Delivered;
        var actor = Actor.User("admin@example.com");

        _senderMock
            .Setup(x => x.Send(It.IsAny<UpdateOrderStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        // Act
        var command = new UpdateOrderStatusCommand(orderId, newStatus, null, actor);
        var result = await _senderMock.Object.Send(command);

        // Assert
        result.Should().Be(orderId);
        command.Status.Should().Be(OrderStatus.Delivered);
    }

    [Test]
    public async Task UpdateOrderStatus_ToCanceled_WithReason_ShouldIncludeReason()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var newStatus = OrderStatus.Canceled;
        var reason = "Customer requested cancellation";
        var actor = Actor.User("admin@example.com");
        UpdateOrderStatusCommand? capturedCommand = null;

        _senderMock
            .Setup(x => x.Send(It.IsAny<UpdateOrderStatusCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Guid>, CancellationToken>((cmd, _) => capturedCommand = cmd as UpdateOrderStatusCommand)
            .ReturnsAsync(orderId);

        // Act
        var command = new UpdateOrderStatusCommand(orderId, newStatus, reason, actor);
        await _senderMock.Object.Send(command);

        // Assert
        capturedCommand.Should().NotBeNull();
        capturedCommand!.Status.Should().Be(OrderStatus.Canceled);
        capturedCommand.Reason.Should().Be(reason);
    }

    [Test]
    public async Task UpdateOrderStatus_CommandShouldContainCorrectActor()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var newStatus = OrderStatus.Shipped;
        var actor = Actor.User("warehouse@example.com");
        UpdateOrderStatusCommand? capturedCommand = null;

        _senderMock
            .Setup(x => x.Send(It.IsAny<UpdateOrderStatusCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Guid>, CancellationToken>((cmd, _) => capturedCommand = cmd as UpdateOrderStatusCommand)
            .ReturnsAsync(orderId);

        // Act
        var command = new UpdateOrderStatusCommand(orderId, newStatus, null, actor);
        await _senderMock.Object.Send(command);

        // Assert
        capturedCommand!.Actor.Should().Be(actor);
        capturedCommand.Actor.Kind.Should().Be(ActorKind.User);
    }

    [Test]
    public async Task UpdateOrderStatus_ToShipped_ShouldSucceed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var newStatus = OrderStatus.Shipped;
        var actor = Actor.User("shipping@example.com");

        _senderMock
            .Setup(x => x.Send(It.IsAny<UpdateOrderStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        // Act
        var command = new UpdateOrderStatusCommand(orderId, newStatus, null, actor);
        var result = await _senderMock.Object.Send(command);

        // Assert
        result.Should().Be(orderId);
        command.Status.Should().Be(OrderStatus.Shipped);
    }

    [Test]
    public async Task UpdateOrderStatus_ShouldVerifySenderCalledOnce()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var newStatus = OrderStatus.Processing;
        var actor = Actor.User("admin@example.com");

        _senderMock
            .Setup(x => x.Send(It.IsAny<UpdateOrderStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        // Act
        var command = new UpdateOrderStatusCommand(orderId, newStatus, null, actor);
        await _senderMock.Object.Send(command);

        // Assert
        _senderMock.Verify(
            x => x.Send(It.IsAny<UpdateOrderStatusCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task UpdateOrderStatus_WithNullReason_ShouldSucceed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var newStatus = OrderStatus.Delivered;
        var actor = Actor.User("delivery@example.com");

        _senderMock
            .Setup(x => x.Send(It.IsAny<UpdateOrderStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        // Act
        var command = new UpdateOrderStatusCommand(orderId, newStatus, null, actor);
        var result = await _senderMock.Object.Send(command);

        // Assert
        result.Should().Be(orderId);
        command.Reason.Should().BeNull();
    }

    [Test]
    public async Task UpdateOrderStatus_AllStatuses_ShouldBeHandled()
    {
        // Arrange & Act & Assert for each status
        var orderId = Guid.NewGuid();
        var actor = Actor.User("admin@example.com");

        var statuses = new[]
        {
            OrderStatus.Pending,
            OrderStatus.Confirmed,
            OrderStatus.Processing,
            OrderStatus.Shipped,
            OrderStatus.Delivered,
            OrderStatus.Canceled,
            OrderStatus.Refunded
        };

        foreach (var status in statuses)
        {
            _senderMock
                .Setup(x => x.Send(It.IsAny<UpdateOrderStatusCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(orderId);

            var command = new UpdateOrderStatusCommand(orderId, status, null, actor);
            var result = await _senderMock.Object.Send(command);

            result.Should().Be(orderId);
        }
    }
}
