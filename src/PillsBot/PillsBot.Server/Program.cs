using System;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

namespace PillsBot.Server;

public static class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "Unhandled error, exiting.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) => Host
        .CreateDefaultBuilder(args)
        .ConfigureServices((context, services) => services
            .AddApplicationInsightsTelemetryWorkerService(context.Configuration)
            .AddPillsBot(context.Configuration))
        .UseSerilog((context, services, configuration) => configuration
            .MinimumLevel.Is(context.HostingEnvironment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.SemanticKernel", (LogEventLevel)services.GetRequiredService<IOptions<PillsBotOptions>>().Value.AI.LogLevel)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .Enrich.WithProperty("Version", typeof(Program).GetAssemblyVersionString(), true)
            .WriteTo.Console()
            .WriteTo.ApplicationInsights(services.GetRequiredService<TelemetryConfiguration>(),
                TelemetryConverter.Traces));
}
