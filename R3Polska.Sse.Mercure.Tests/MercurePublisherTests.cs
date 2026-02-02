using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using R3Polska.Sse.Mercure.Message;
using R3Polska.Sse.Mercure.Tests.Fixtures;
using RichardSzalay.MockHttp;
using Shouldly;

namespace R3Polska.Sse.Mercure.Tests;

public class MercurePublisherTests
{
    private const string TestHost = "https://mercure.example.com";
    private const string TestToken = "test-jwt-token";
    private const string MercureEndpoint = "/.well-known/mercure";

    private readonly Mock<ILogger<MercurePublisher>> _loggerMock;
    private readonly IOptions<MercurePublisherOptions> _options;
    private readonly MockHttpMessageHandler _mockHttp;

    public MercurePublisherTests()
    {
        _loggerMock = new Mock<ILogger<MercurePublisher>>();
        _options = Options.Create(new MercurePublisherOptions
        {
            Host = TestHost,
            Token = TestToken
        });
        _mockHttp = new MockHttpMessageHandler();
    }

    private MercurePublisher CreatePublisher()
    {
        var httpClient = _mockHttp.ToHttpClient();
        return new MercurePublisher(_loggerMock.Object, _options, httpClient);
    }

    [Fact]
    public async Task Publish_WithValidMessage_SendsCorrectRequest()
    {
        // Arrange
        var payload = new SimplePayload("test-value");
        var message = new MercureMessage
        {
            Topic = "test/topic",
            Payload = payload
        };

        string? capturedContent = null;
        _mockHttp.Expect(HttpMethod.Post, TestHost + MercureEndpoint)
            .WithHeaders("Authorization", $"Bearer {TestToken}")
            .Respond(req =>
            {
                capturedContent = req.Content!.ReadAsStringAsync().Result;
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var publisher = CreatePublisher();

        // Act
        await publisher.Publish(message);

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
        capturedContent.ShouldNotBeNull();
        capturedContent.ShouldContain("topic=test%2Ftopic");
        capturedContent.ShouldContain("data=");
    }

    [Fact]
    public async Task Publish_WithMessageId_IncludesIdInFormData()
    {
        // Arrange
        var payload = new SimplePayload("test-value");
        var message = new MercureMessage
        {
            Id = "msg-123",
            Topic = "test/topic",
            Payload = payload
        };

        string? capturedContent = null;
        _mockHttp.Expect(HttpMethod.Post, TestHost + MercureEndpoint)
            .Respond(req =>
            {
                capturedContent = req.Content!.ReadAsStringAsync().Result;
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var publisher = CreatePublisher();

        // Act
        await publisher.Publish(message);

        // Assert
        capturedContent.ShouldNotBeNull();
        capturedContent.ShouldContain("id=msg-123");
        // Verify id comes first in the form data
        var idIndex = capturedContent.IndexOf("id=");
        var topicIndex = capturedContent.IndexOf("topic=");
        idIndex.ShouldBeLessThan(topicIndex);
    }

    [Fact]
    public async Task Publish_WithoutMessageId_ExcludesIdFromFormData()
    {
        // Arrange
        var payload = new SimplePayload("test-value");
        var message = new MercureMessage
        {
            Topic = "test/topic",
            Payload = payload
        };

        string? capturedContent = null;
        _mockHttp.Expect(HttpMethod.Post, TestHost + MercureEndpoint)
            .Respond(req =>
            {
                capturedContent = req.Content!.ReadAsStringAsync().Result;
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var publisher = CreatePublisher();

        // Act
        await publisher.Publish(message);

        // Assert
        capturedContent.ShouldNotBeNull();
        capturedContent.ShouldNotContain("id=");
    }

    [Fact]
    public async Task Publish_WithSimplePayload_SerializesCorrectly()
    {
        // Arrange
        var payload = new SimplePayload("hello-world");
        var message = new MercureMessage
        {
            Topic = "test/topic",
            Payload = payload
        };

        string? capturedContent = null;
        _mockHttp.Expect(HttpMethod.Post, TestHost + MercureEndpoint)
            .Respond(req =>
            {
                capturedContent = req.Content!.ReadAsStringAsync().Result;
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var publisher = CreatePublisher();

        // Act
        await publisher.Publish(message);

        // Assert
        capturedContent.ShouldNotBeNull();
        // URL-decoded the data should contain the JSON
        var decodedContent = Uri.UnescapeDataString(capturedContent);
        decodedContent.ShouldContain("\"Value\":\"hello-world\"");
    }

    [Fact]
    public async Task Publish_WithStructuredPayload_SerializesCorrectly()
    {
        // Arrange
        var payload = new StructuredPayload("test-name", 42);
        var message = new MercureMessage
        {
            Topic = "test/topic",
            Payload = payload
        };

        string? capturedContent = null;
        _mockHttp.Expect(HttpMethod.Post, TestHost + MercureEndpoint)
            .Respond(req =>
            {
                capturedContent = req.Content!.ReadAsStringAsync().Result;
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var publisher = CreatePublisher();

        // Act
        await publisher.Publish(message);

        // Assert
        capturedContent.ShouldNotBeNull();
        var decodedContent = Uri.UnescapeDataString(capturedContent);
        decodedContent.ShouldContain("\"Name\":\"test-name\"");
        decodedContent.ShouldContain("\"Count\":42");
    }

    [Fact]
    public async Task Publish_OnSuccess_LogsDebugMessage()
    {
        // Arrange
        var payload = new SimplePayload("test-value");
        var message = new MercureMessage
        {
            Topic = "test/topic",
            Payload = payload
        };

        _mockHttp.Expect(HttpMethod.Post, TestHost + MercureEndpoint)
            .Respond(HttpStatusCode.OK);

        var publisher = CreatePublisher();

        // Act
        await publisher.Publish(message);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sent to Mercure")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Publish_OnSuccess_LogsInformationMessage()
    {
        // Arrange
        var payload = new SimplePayload("test-value");
        var message = new MercureMessage
        {
            Topic = "test/topic",
            Payload = payload
        };

        _mockHttp.Expect(HttpMethod.Post, TestHost + MercureEndpoint)
            .Respond(HttpStatusCode.OK);

        var publisher = CreatePublisher();

        // Act
        await publisher.Publish(message);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending message to Mercure")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task Publish_NonSuccessStatusCode_ThrowsMercurePublisherException(HttpStatusCode statusCode)
    {
        // Arrange
        var payload = new SimplePayload("test-value");
        var message = new MercureMessage
        {
            Topic = "test/topic",
            Payload = payload
        };

        const string errorContent = "Error response content";
        _mockHttp.Expect(HttpMethod.Post, TestHost + MercureEndpoint)
            .Respond(statusCode, "text/plain", errorContent);

        var publisher = CreatePublisher();

        // Act & Assert
        var exception = await Should.ThrowAsync<MercurePublisherException>(
            async () => await publisher.Publish(message));

        exception.Message.ShouldContain(statusCode.ToString());
        exception.Message.ShouldContain(errorContent);
    }

    [Fact]
    public async Task Publish_HttpRequestException_WrapsInMercurePublisherException()
    {
        // Arrange
        var payload = new SimplePayload("test-value");
        var message = new MercureMessage
        {
            Topic = "test/topic",
            Payload = payload
        };

        var innerException = new HttpRequestException("Network error");
        _mockHttp.Expect(HttpMethod.Post, TestHost + MercureEndpoint)
            .Throw(innerException);

        var publisher = CreatePublisher();

        // Act & Assert
        var exception = await Should.ThrowAsync<MercurePublisherException>(
            async () => await publisher.Publish(message));

        exception.Message.ShouldContain("Network error");
        exception.InnerException.ShouldBe(innerException);
    }

    [Fact]
    public async Task Publish_CancellationRequested_ThrowsMercurePublisherException()
    {
        // Arrange
        var payload = new SimplePayload("test-value");
        var message = new MercureMessage
        {
            Topic = "test/topic",
            Payload = payload
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockHttp.Expect(HttpMethod.Post, TestHost + MercureEndpoint)
            .Respond(HttpStatusCode.OK);

        var publisher = CreatePublisher();

        // Act & Assert
        var exception = await Should.ThrowAsync<MercurePublisherException>(
            async () => await publisher.Publish(message, cts.Token));

        exception.InnerException.ShouldBeOfType<TaskCanceledException>();
    }

    [Fact]
    public async Task Publish_GenericException_WrapsInMercurePublisherException()
    {
        // Arrange
        var payload = new SimplePayload("test-value");
        var message = new MercureMessage
        {
            Topic = "test/topic",
            Payload = payload
        };

        var innerException = new InvalidOperationException("Something went wrong");
        _mockHttp.Expect(HttpMethod.Post, TestHost + MercureEndpoint)
            .Throw(innerException);

        var publisher = CreatePublisher();

        // Act & Assert
        var exception = await Should.ThrowAsync<MercurePublisherException>(
            async () => await publisher.Publish(message));

        exception.Message.ShouldContain("Something went wrong");
        exception.InnerException.ShouldBe(innerException);
    }
}
