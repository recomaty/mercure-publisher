using Microsoft.Extensions.DependencyInjection;
using R3Polska.Sse.Mercure.Contract;

namespace R3Polska.Sse.Mercure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMercurePublisher(
        this IServiceCollection services,
        Action<MercurePublisherOptions> configureOptions)
    {
        services.AddOptions<MercurePublisherOptions>()
            .Configure(configureOptions)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IMercurePublisher, MercurePublisher>();

        return services;
    }
}
