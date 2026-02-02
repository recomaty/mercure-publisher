namespace R3Polska.Sse.Mercure;

/// <summary>
/// Exception thrown when publishing to Mercure fails.
/// </summary>
public class MercurePublisherException : Exception
{
    public MercurePublisherException(string message) : base(message)
    {
    }

    public MercurePublisherException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
