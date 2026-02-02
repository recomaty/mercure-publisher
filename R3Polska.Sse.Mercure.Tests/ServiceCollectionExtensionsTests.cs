using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using R3Polska.Sse.Mercure.Contract;
using Shouldly;

namespace R3Polska.Sse.Mercure.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMercurePublisher_RegistersIMercurePublisher()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMercurePublisher(options =>
        {
            options.Host = "https://mercure.example.com";
            options.Token = "test-token";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var publisher = serviceProvider.GetService<IMercurePublisher>();
        publisher.ShouldNotBeNull();
        publisher.ShouldBeOfType<MercurePublisher>();
    }

    [Fact]
    public void AddMercurePublisher_RegistersOptionsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        const string expectedHost = "https://mercure.example.com";
        const string expectedToken = "test-jwt-token";

        // Act
        services.AddMercurePublisher(options =>
        {
            options.Host = expectedHost;
            options.Token = expectedToken;
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<MercurePublisherOptions>>();
        options.Value.Host.ShouldBe(expectedHost);
        options.Value.Token.ShouldBe(expectedToken);
    }

    [Fact]
    public void AddMercurePublisher_ConfiguresHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMercurePublisher(options =>
        {
            options.Host = "https://mercure.example.com";
            options.Token = "test-token";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        httpClientFactory.ShouldNotBeNull();
    }

    [Fact]
    public void AddMercurePublisher_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddMercurePublisher(options =>
        {
            options.Host = "https://mercure.example.com";
            options.Token = "test-token";
        });

        // Assert
        result.ShouldBe(services);
    }

    [Fact]
    public void AddMercurePublisher_InvalidOptions_ThrowsOptionsValidationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddMercurePublisher(options =>
        {
            options.Host = "invalid-url";
            options.Token = "";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        Should.Throw<OptionsValidationException>(() =>
        {
            // ValidateOnStart triggers validation when the service is resolved
            serviceProvider.GetRequiredService<IMercurePublisher>();
        });
    }

    [Fact]
    public void AddMercurePublisher_EmptyHost_ThrowsOptionsValidationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddMercurePublisher(options =>
        {
            options.Host = "";
            options.Token = "valid-token";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        Should.Throw<OptionsValidationException>(() =>
        {
            serviceProvider.GetRequiredService<IMercurePublisher>();
        });
    }

    [Fact]
    public void AddMercurePublisher_EmptyToken_ThrowsOptionsValidationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddMercurePublisher(options =>
        {
            options.Host = "https://mercure.example.com";
            options.Token = "";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        Should.Throw<OptionsValidationException>(() =>
        {
            serviceProvider.GetRequiredService<IMercurePublisher>();
        });
    }
}
