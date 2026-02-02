using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using R3Polska.Sse.Mercure.Contract;
using R3Polska.Sse.Mercure.Message;

namespace R3Polska.Sse.Mercure;

/// <summary>
/// Service for publishing messages to Mercure.
/// </summary>
public class MercurePublisher(ILogger<MercurePublisher> logger, IOptions<MercurePublisherOptions> options, HttpClient httpClient) : IMercurePublisher
{
    private readonly string _host = options.Value.Host;
    private readonly string _token = options.Value.Token;
    private readonly ILogger _logger = logger;
    private readonly HttpClient _httpClient = httpClient;

    /// <summary>
    /// Publishes a message to Mercure.
    /// </summary>
    /// <param name="mercureMessage">The message to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task Publish(MercureMessage mercureMessage, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, _host + "/.well-known/mercure");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        var body = JsonSerializer.Serialize(mercureMessage.Payload, mercureMessage.Payload.GetType());

        var formData = new List<KeyValuePair<string, string>>
        {
            new("topic", mercureMessage.Topic),
            new("data", body)
        };

        if (mercureMessage.Id != null)
        {
            formData.Insert(0, new("id", mercureMessage.Id));
        }

        requestMessage.Content = new FormUrlEncodedContent(formData);
        _logger.LogInformation("Sending message to Mercure: topic={Topic}, id={Id}", mercureMessage.Topic, mercureMessage.Id);

        try
        {
            using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Sent to Mercure: {Body}", body);
                return;
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new MercurePublisherException(
                $"Failed to send to Mercure. Status: {response.StatusCode}, Content: {errorContent}");
        }
        catch (System.Exception ex) when (ex is not MercurePublisherException)
        {
            throw new MercurePublisherException($"Failed to send to Mercure: {ex.Message}", ex);
        }
    }
}