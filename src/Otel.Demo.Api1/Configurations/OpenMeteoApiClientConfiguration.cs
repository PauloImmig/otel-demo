using Otel.Demo.Api1.Clients;

namespace Otel.Demo.Api1.Configurations;

public static class OpenMeteoApiClientConfiguration
{
    public static void AddOpenMeteoClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("open-meteo-client", httClient =>
        {
            var openMeteoClientSection = configuration.GetRequiredSection("OpenMeteoClient");
            var baseUri = openMeteoClientSection.GetValue<string>("BaseUri") ?? throw new Exception("Invalid client configuration. OpenMeteoClient 'BaseUri' configuration not set.");
            httClient.BaseAddress = new Uri(baseUri);
        });

        services.AddTransient<IOpenMeteoApiClient, OpenMeteoApiClient>();
    }
}

