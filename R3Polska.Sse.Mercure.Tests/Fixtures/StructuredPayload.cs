using R3Polska.Sse.Mercure.Message.Contract;

namespace R3Polska.Sse.Mercure.Tests.Fixtures;

public record StructuredPayload(string Name, int Count) : IMercureMessagePayload;
