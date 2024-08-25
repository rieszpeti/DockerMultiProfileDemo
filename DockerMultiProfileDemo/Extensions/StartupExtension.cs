using DockerMultiProfileDemo.Metrics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace DockerMultiProfileDemo.Extensions
{
    public static class StartupExtension
    {
        public static void SetupOpenTelemetry(this WebApplicationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Logging.AddOpenTelemetry(x =>
            {
                x.IncludeScopes = true;
                x.IncludeFormattedMessage = true;
            });

            builder.Services.AddOpenTelemetry()
                .WithMetrics(x =>
                {
                    x.AddAspNetCoreInstrumentation()
                     .AddHttpClientInstrumentation();

                    x.AddRuntimeInstrumentation()
                        .AddMeter(
                            "Microsoft.AspNetCore.Hosting",
                            "Microsoft.AspNetCore.Server.Kestrel",
                            "System.Net.Http",
                            "dockermultiprofiledemo.api"
                        );

                    x.AddOtlpExporter();
                })
                .WithTracing(x =>
                {
                    if (builder.Environment.IsDevelopment())
                    {
                        x.SetSampler<AlwaysOnSampler>();
                    }

                    x.AddAspNetCoreInstrumentation()
                     .AddHttpClientInstrumentation()
                     .AddEntityFrameworkCoreInstrumentation();
                });

            builder.Services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
            builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter());
            builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());

            builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

            builder.Services.ConfigureHttpClientDefaults(http =>
            {
                http.AddStandardResilienceHandler();
            });

            var apiKey = Environment.GetEnvironmentVariable("OTPLKEY");

            builder.Services.Configure<OtlpExporterOptions>(
                o => o.Headers = $"x-otlp-api-key={apiKey}");
            builder.Services.AddMetrics();
            builder.Services.AddSingleton<DemoMetrics>();
        }
    }
}
