using System.Net;
using R3Polska.Sse.Mercure.Message;
using RichardSzalay.MockHttp;

namespace R3Polska.Sse.Mercure.BddTests.Support;

public class TestContext
{
    public string? Host { get; set; }
    public string? Token { get; set; }
    public MercureMessage? Message { get; set; }
    public TestPayload Payload { get; set; } = new();
    public MockHttpMessageHandler MockHttp { get; set; } = new();
    public HttpStatusCode? MockStatusCode { get; set; }
    public string? MockErrorMessage { get; set; }
    public bool SimulateNetworkError { get; set; }
    public bool SimulateCancellation { get; set; }
    public string? CapturedRequestBody { get; set; }
    public HttpRequestMessage? CapturedRequest { get; set; }
    public Exception? ThrownException { get; set; }
    public bool PublishSucceeded { get; set; }
    public MercurePublisherOptions? Options { get; set; }
    public List<System.ComponentModel.DataAnnotations.ValidationResult> ValidationResults { get; set; } = new();
}
