using System;
using System.IO;
using System.Threading.Tasks;
using KafkaProducer.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Newtonsoft.Json;
using Serilog;
using Serilog.Exceptions;

namespace KafkaProducer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ConsoleKeyInfo cki;

            Console.Clear();

            // Establish an event handler to process key press events.
            Console.CancelKeyPress += new ConsoleCancelEventHandler(myHandler);

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
                    services.AddSingleton<Producer.KafkaProducer>();

                });

            var host = hostBuilder.Build();

            while (true)
            {
                Console.WriteLine("Name =>  ");
                var name = Console.ReadLine();
                Console.WriteLine("Age =>  ");
                var age = Console.ReadLine();

                var person = new
                {
                    name,
                    age
                };

                using (var serviceScope = host.Services.CreateScope())
                {
                    var services = serviceScope.ServiceProvider;
                    var json = JsonConvert.SerializeObject(person);
                    try
                    {
                        var myService = services.GetRequiredService<Producer.KafkaProducer>();
                        await myService.ProduceAsync(new Confluent.Kafka.Message<string, string> { Key = Guid.NewGuid().ToString(), Value = json });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error Occured {ex.Message}");
                    }
                }

                Console.Write("Press any key, or 'X' to quit, or ");
                Console.WriteLine("CTRL+C to interrupt the read operation:");

                // Start a console read operation. Do not display the input.
                cki = Console.ReadKey(true);
                // Announce the name of the key that was pressed .
                Console.WriteLine($"  Key pressed: {cki.Key}\n");

                // Exit if the user pressed the 'X' key.
                if (cki.Key == ConsoleKey.X) break;
            }

        }

        protected static void myHandler(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("\nThe read operation has been interrupted.");

            Console.WriteLine($"  Key pressed: {args.SpecialKey}");

            Console.WriteLine($"  Cancel property: {args.Cancel}");

            // Set the Cancel property to true to prevent the process from terminating.
            Console.WriteLine("Setting the Cancel property to true...");
            args.Cancel = true;

            // Announce the new value of the Cancel property.
            Console.WriteLine($"  Cancel property: {args.Cancel}");
            Console.WriteLine("The read operation will resume...\n");
        }
    }
}
