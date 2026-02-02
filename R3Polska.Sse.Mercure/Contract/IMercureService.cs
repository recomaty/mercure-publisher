using R3Polska.Sse.Mercure.Message;

namespace R3Polska.Sse.Mercure.Contract;

/// <summary>
/// Interface for publishing messages to Mercure.
/// </summary>
public interface IMercurePublisher
{
    /// <summary>
    /// Publishes a message to Mercure.
    /// </summary>
    /// <param name="mercureMessage">The message to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task Publish(MercureMessage mercureMessage, CancellationToken cancellationToken = default);
}