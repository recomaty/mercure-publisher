using R3Polska.Sse.Mercure.Message.Contract;

namespace R3Polska.Sse.Mercure.BddTests.Support;

public class TestPayload : IMercureMessagePayload
{
    public Dictionary<string, string> Fields { get; set; } = new();
}
