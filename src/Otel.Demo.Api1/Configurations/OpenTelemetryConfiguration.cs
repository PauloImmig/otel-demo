using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace Otel.Demo.Api1.Configurations;

public static class OpenTelemetryConfiguration
{
    public static void ConfigureOpenTelemetry(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var otelTelemetrySection = configuration.GetRequiredSection("OpenTelemetrySettings");
        var azureMonitorConnectionString = otelTelemetrySection.GetValue<string>("AzureMonitor:ConnectionString");

        serviceCollection.AddOpenTelemetry()
        .WithTracing(builder => builder
            .AddSource(DiagnosticsConfig.ActivitySource.Name, "MySqlConnector")
            .ConfigureResource(resource => resource
                .AddService(DiagnosticsConfig.ServiceName))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter()
            .AddAzureMonitorTraceExporter(o => o.ConnectionString = azureMonitorConnectionString))
        .WithMetrics(builder => builder
            .ConfigureResource(resource => resource
                .AddService(DiagnosticsConfig.ServiceName))
            .AddAspNetCoreInstrumentation()
            .AddConsoleExporter()
            .AddAzureMonitorMetricExporter(o => o.ConnectionString = azureMonitorConnectionString));
    }

    public static void UseAzureMonitor(this ILoggingBuilder loggingBuilder, IConfiguration configuration)
    {
        var otelTelemetrySection = configuration.GetRequiredSection("OpenTelemetrySettings");
        var azureMonitorConnectionString = otelTelemetrySection.GetValue<string>("AzureMonitor:ConnectionString");

        loggingBuilder.AddOpenTelemetry(options =>
        {
            options.AddAzureMonitorLogExporter(o => o.ConnectionString = azureMonitorConnectionString);
        });
    }
}

public static class DiagnosticsConfig
{
    public const string ServiceName = "otel-demo-api1";
    public static ActivitySource ActivitySource = new ActivitySource(ServiceName);
}
