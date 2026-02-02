using R3Polska.Sse.Mercure.Message;
using R3Polska.Sse.Mercure.Tests.Fixtures;
using Shouldly;

namespace R3Polska.Sse.Mercure.Tests;

public class MercureMessageTests
{
    [Fact]
    public void MercureMessage_WithRequiredProperties_CreatesInstance()
    {
        // Arrange
        var payload = new SimplePayload("test-value");
        const string topic = "test/topic";

        // Act
        var message = new MercureMessage
        {
            Topic = topic,
            Payload = payload
        };

        // Assert
        message.Topic.ShouldBe(topic);
        message.Payload.ShouldBe(payload);
    }

    [Fact]
    public void MercureMessage_IdIsOptional_DefaultsToNull()
    {
        // Arrange
        var payload = new SimplePayload("test-value");

        // Act
        var message = new MercureMessage
        {
            Topic = "test/topic",
            Payload = payload
        };

        // Assert
        message.Id.ShouldBeNull();
    }

    [Fact]
    public void MercureMessage_WithId_SetsIdCorrectly()
    {
        // Arrange
        var payload = new SimplePayload("test-value");
        const string expectedId = "msg-123";

        // Act
        var message = new MercureMessage
        {
            Id = expectedId,
            Topic = "test/topic",
            Payload = payload
        };

        // Assert
        message.Id.ShouldBe(expectedId);
    }

    [Fact]
    public void MercureMessage_WithStructuredPayload_SetsPayloadCorrectly()
    {
        // Arrange
        var payload = new StructuredPayload("test-name", 42);

        // Act
        var message = new MercureMessage
        {
            Topic = "test/topic",
            Payload = payload
        };

        // Assert
        message.Payload.ShouldBe(payload);
        var typedPayload = message.Payload.ShouldBeOfType<StructuredPayload>();
        typedPayload.Name.ShouldBe("test-name");
        typedPayload.Count.ShouldBe(42);
    }

    [Fact]
    public void MercureMessage_IdCanBeSetToNull()
    {
        // Arrange
        var payload = new SimplePayload("test-value");

        // Act
        var message = new MercureMessage
        {
            Id = null,
            Topic = "test/topic",
            Payload = payload
        };

        // Assert
        message.Id.ShouldBeNull();
    }
}
