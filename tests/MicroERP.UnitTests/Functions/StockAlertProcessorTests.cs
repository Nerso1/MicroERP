using Azure;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using MicroERP.Functions;
using MicroERP.Functions.Messages;
using MicroERP.Functions.Processing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;

namespace MicroERP.UnitTests.Functions;

public class StockAlertProcessorTests
{
    private const string MessageId = "c0874b03-38ae-4cdf-a7b9-91d8c2a36eb7";

    private readonly Mock<IProcessedMessageStore> _storeMock = new();
    private readonly FakeLogger<StockAlertProcessor> _logger = new();
    private readonly StockAlertProcessor _processor;

    public StockAlertProcessorTests()
    {
        _processor = new StockAlertProcessor(_storeMock.Object, _logger);
    }

    [Fact]
    public async Task Run_NewMessage_ProcessesAlertAndKeepsItMarked()
    {
        SetupMarkResult(true);

        await _processor.Run(CreateMessage(ValidAlertBody()), CancellationToken.None);

        _logger.Collector.GetSnapshot()
            .Should().ContainSingle(r => r.Level == LogLevel.Warning)
            .Which.Message.Should().Contain("product 4");
        _storeMock.Verify(
            s => s.UnmarkAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_DuplicateMessage_SkipsProcessing()
    {
        SetupMarkResult(false);

        await _processor.Run(CreateMessage(ValidAlertBody()), CancellationToken.None);

        _logger.Collector.GetSnapshot()
            .Should().NotContain(r => r.Level == LogLevel.Warning);
        _storeMock.Verify(
            s => s.UnmarkAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_EmptyBody_UnmarksAndRethrows()
    {
        SetupMarkResult(true);

        var act = () => _processor.Run(CreateMessage(BinaryData.FromString("null")), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _storeMock.Verify(s => s.UnmarkAsync(MessageId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Run_StoreUnavailable_ThrowsWithoutProcessing()
    {
        _storeMock
            .Setup(s => s.TryMarkProcessedAsync(MessageId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException("Storage unavailable"));

        var act = () => _processor.Run(CreateMessage(ValidAlertBody()), CancellationToken.None);

        await act.Should().ThrowAsync<RequestFailedException>();
        _logger.Collector.GetSnapshot()
            .Should().NotContain(r => r.Level == LogLevel.Warning);
        _storeMock.Verify(
            s => s.UnmarkAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupMarkResult(bool isNewMessage) =>
        _storeMock
            .Setup(s => s.TryMarkProcessedAsync(MessageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(isNewMessage);

    // Serialized the same way as ServiceBusStockAlertPublisher in the API (BinaryData.FromObjectAsJson, default options).
    private static BinaryData ValidAlertBody() =>
        BinaryData.FromObjectAsJson(new StockLowAlertMessage(
            AlertId: Guid.Parse(MessageId),
            ProductId: 4,
            WarehouseId: 1,
            Quantity: 9,
            OccurredAtUtc: DateTime.UtcNow));

    private static ServiceBusReceivedMessage CreateMessage(BinaryData body) =>
        ServiceBusModelFactory.ServiceBusReceivedMessage(body: body, messageId: MessageId, deliveryCount: 1);
}
