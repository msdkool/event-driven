using System;
using System.IO;
using System.Threading.Tasks;
using KafkaCDCClass.Consumer;
using KafkaCDCClass.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;

namespace KafkaCDCClass
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, configBuilder) =>
                {
                    configBuilder.SetBasePath(Directory.GetCurrentDirectory());
                    configBuilder.AddJsonFile("appsettings.json", optional: true);
                    configBuilder.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    var logSection = hostContext.Configuration.GetSection("Logging");
                    configLogging.AddConfiguration(logSection);
                    configLogging.AddConsole();
                    configLogging.AddSerilog();
                    configLogging.AddDebug();

                    var defaultLevel = logSection.GetSection("LogLevel:Default").Value.GetLogLevel();
                    var systemLevel = logSection.GetSection("LogLevel:System").Value.GetLogLevel();
                    var microsoftLevel = logSection.GetSection("LogLevel:Microsoft").Value.GetLogLevel();

                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.ControlledBy(defaultLevel)
                        .MinimumLevel.Override("System", systemLevel)
                        .MinimumLevel.Override("Default", defaultLevel)
                        .MinimumLevel.Override("Microsoft", microsoftLevel)
                        .Enrich.FromLogContext()
                        .Enrich.WithExceptionDetails()
                        .CreateLogger();

                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(hostContext.Configuration);
                    services.AddHostedService<KafkaConsumer>();

                });

            await hostBuilder.RunConsoleAsync();
        }
    }
}
