using R3Polska.Sse.Mercure.Message.Contract;

namespace R3Polska.Sse.Mercure.Message;

public class MercureMessage
{
    public string? Id { get; set; } = null;

    public required string Topic { get; set; }

    public required IMercureMessagePayload Payload { get; set; }
}