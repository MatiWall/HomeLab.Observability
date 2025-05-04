using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Configure OpenTelemetry Tracing
                services.AddOpenTelemetryTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault()
                            .AddService("HomeLab.Observability.SampleApp"))
                        .AddSource("SampleApp.Tracer")
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri("http://otel-collector:4317"); // OTLP gRPC endpoint
                        });
                });

                // Configure OpenTelemetry Metrics
                services.AddOpenTelemetryMetrics(meterProviderBuilder =>
                {
                    meterProviderBuilder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault()
                            .AddService("HomeLab.Observability.SampleApp"))
                        .AddMeter("SampleApp.Meter")
                        .AddAspNetCoreInstrumentation()
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri("http://otel-collector:4317"); // OTLP gRPC endpoint
                        });
                });

                // Configure Logging
                services.AddLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddOpenTelemetry(options =>
                    {
                        options.SetResourceBuilder(ResourceBuilder.CreateDefault()
                            .AddService("HomeLab.Observability.SampleApp"));
                        options.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri("http://otel-collector:4317"); // OTLP gRPC endpoint
                        });
                    });
                });
            });

        var host = builder.Build();

        // Simulate application logic
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        var tracer = new ActivitySource("SampleApp.Tracer");
        var meter = new Meter("SampleApp.Meter");
        var requestCounter = meter.CreateCounter<long>("http_requests_total", "Counts the total number of HTTP requests");

        using (var activity = tracer.StartActivity("MainOperation"))
        {
            logger.LogInformation("Application started");
            logger.LogWarning("This is a warning message");
            logger.LogError("This is an error message");

            // Simulate metrics
            requestCounter.Add(1, new("http.route", "/home"), new("http.method", "GET"));

            // Simulate a trace
            using (var subActivity = tracer.StartActivity("SubOperation"))
            {
                logger.LogInformation("Performing a sub-operation");
                Thread.Sleep(500); // Simulate work
            }
        }

        logger.LogInformation("Application finished");

        await host.RunAsync();
    }
}