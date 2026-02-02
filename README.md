# R3Polska.Sse.Mercure

A .NET 9 client library for publishing messages to [Mercure](https://mercure.rocks/) hubs via Server-Sent Events (SSE).

[![CI](https://github.com/r3polska/mercure-client/actions/workflows/ci.yml/badge.svg)](https://github.com/r3polska/mercure-client/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/recomaty/mercure-publisher/graph/badge.svg?token=EzAKHM9DeH)](https://codecov.io/gh/recomaty/mercure-publisher)

## Features

- 🚀 Simple, strongly-typed API for publishing messages to Mercure
- 🔧 Easy integration with ASP.NET Core dependency injection
- 🛡️ Built-in support for resilient HTTP communication with Polly
- ✅ 100% test coverage

## Installation

```bash
dotnet add package R3Polska.Sse.Mercure
```

## Quick Start

### 1. Configure Services

```csharp
builder.Services.AddMercurePublisher(options =>
{
    options.Host = "https://mercure.example.com";
    options.Token = "your-jwt-token";
});
```

### 2. Create a Payload

Implement `IMercureMessagePayload` for your message payloads:

```csharp
using R3Polska.Sse.Mercure.Message.Contract;

public record OrderCreatedPayload(
    Guid OrderId,
    string CustomerEmail,
    decimal TotalAmount
) : IMercureMessagePayload;
```

### 3. Publish Messages

```csharp
using R3Polska.Sse.Mercure.Contract;
using R3Polska.Sse.Mercure.Message;

public class OrderService
{
    private readonly IMercurePublisher _mercurePublisher;

    public OrderService(IMercurePublisher mercurePublisher)
    {
        _mercurePublisher = mercurePublisher;
    }

    public async Task NotifyOrderCreated(Order order, CancellationToken ct)
    {
        var message = new MercureMessage
        {
            Id = Guid.NewGuid().ToString(),  // Optional: Mercure will generate one if omitted
            Topic = $"orders/{order.CustomerId}",
            Payload = new OrderCreatedPayload(order.Id, order.CustomerEmail, order.TotalAmount)
        };

        await _mercurePublisher.Publish(message, ct);
    }
}
```

## Configuration

### Basic Configuration

```csharp
builder.Services.AddMercurePublisher(options =>
{
    options.Host = "https://mercure.example.com";
    options.Token = "your-jwt-token";
});
```

### Configuration from appsettings.json

```json
{
  "Mercure": {
    "Host": "https://mercure.example.com",
    "Token": "your-jwt-token"
  }
}
```

```csharp
builder.Services.AddMercurePublisher(options =>
{
    builder.Configuration.GetSection("Mercure").Bind(options);
});
```

### Configuration Options

| Option | Type | Required | Description |
|--------|------|----------|-------------|
| `Host` | `string` | ✅ | The base URL of your Mercure hub (must be a valid URL) |
| `Token` | `string` | ✅ | JWT token for authenticating with the Mercure hub |

## Resilient HttpClient with Polly

For production use, it's **strongly recommended** to configure resilient HTTP communication using [Polly](https://github.com/App-vNext/Polly) and `Microsoft.Extensions.Http.Resilience`.

### 1. Install Required Packages

```bash
dotnet add package Microsoft.Extensions.Http.Resilience
```

### 2. Configure Resilient HttpClient

```csharp
using Microsoft.Extensions.Http.Resilience;

builder.Services.AddMercurePublisher(options =>
{
    options.Host = "https://mercure.example.com";
    options.Token = "your-jwt-token";
});

// Configure resilience for the Mercure HttpClient
builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler();
});
```

### 3. Advanced Resilience Configuration

For fine-grained control over retry policies, circuit breakers, and timeouts:

```csharp
using Microsoft.Extensions.Http.Resilience;
using Polly;

builder.Services.AddMercurePublisher(options =>
{
    options.Host = "https://mercure.example.com";
    options.Token = "your-jwt-token";
});

// Configure resilience specifically for IMercurePublisher's HttpClient
builder.Services.AddHttpClient<IMercurePublisher, MercurePublisher>()
    .AddResilienceHandler("mercure-resilience", builder =>
    {
        // Retry policy: retry up to 3 times with exponential backoff
        builder.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(500),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .HandleResult(r => r.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
        });

        // Circuit breaker: break after 5 failures, stay open for 30 seconds
        builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(10),
            MinimumThroughput = 5,
            BreakDuration = TimeSpan.FromSeconds(30),
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .HandleResult(r => r.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
        });

        // Timeout: 10 seconds per request
        builder.AddTimeout(TimeSpan.FromSeconds(10));
    });
```

### 4. Using Standard Resilience Handler (Recommended)

The simplest approach is using the standard resilience handler which includes sensible defaults:

```csharp
using Microsoft.Extensions.Http.Resilience;
using R3Polska.Sse.Mercure;
using R3Polska.Sse.Mercure.Contract;

builder.Services.AddMercurePublisher(options =>
{
    options.Host = "https://mercure.example.com";
    options.Token = "your-jwt-token";
});

builder.Services.AddHttpClient<IMercurePublisher, MercurePublisher>()
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromMilliseconds(200);
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
    });
```

### Standard Resilience Handler Defaults

The `AddStandardResilienceHandler()` includes these policies by default:

| Policy | Default Behavior |
|--------|------------------|
| **Rate Limiter** | Limits concurrent requests |
| **Total Timeout** | 30 seconds for entire request including retries |
| **Retry** | 3 retries with exponential backoff + jitter |
| **Circuit Breaker** | Opens after 10% failure rate over 30 seconds |
| **Attempt Timeout** | 2 seconds per individual attempt |

## Error Handling

The library throws `MercurePublisherException` when publishing fails:

```csharp
try
{
    await _mercurePublisher.Publish(message, cancellationToken);
}
catch (MercurePublisherException ex)
{
    _logger.LogError(ex, "Failed to publish message to Mercure: {Message}", ex.Message);
    
    // Check for inner exception (network errors, cancellation, etc.)
    if (ex.InnerException is HttpRequestException httpEx)
    {
        // Handle network-related errors
    }
}
```

## Message Structure

### MercureMessage Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `string?` | ❌ | Unique message identifier. If omitted, Mercure generates one. |
| `Topic` | `string` | ✅ | The topic URI that subscribers listen to |
| `Payload` | `IMercureMessagePayload` | ✅ | The message payload (serialized as JSON) |

### Example Payloads

```csharp
// Simple payload
public record NotificationPayload(string Message) : IMercureMessagePayload;

// Complex payload
public record UserActivityPayload(
    Guid UserId,
    string Action,
    Dictionary<string, object> Metadata,
    DateTime Timestamp
) : IMercureMessagePayload;
```

## Development

### Prerequisites

- .NET 9.0 SDK
- (Optional) `dotnet-reportgenerator-globaltool` for coverage reports

### Build

```bash
make build
# or
dotnet build R3Polska.Sse.Mercure.sln
```

### Run Tests

```bash
make test
# or
dotnet test R3Polska.Sse.Mercure.Tests/R3Polska.Sse.Mercure.Tests.csproj
```

### Generate Coverage Report

```bash
make coverage
```

The HTML report will be available at `coveragereport/index.html`.

## License

BSD 3-Clause License - see [LICENSE](LICENSE) for details.
