using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConsumerApp
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {

            var builder = BuildConfig(new ConfigurationBuilder());

            Log.Logger = new LoggerConfiguration()
                         .ReadFrom.Configuration(builder.Build())
                         .Enrich.FromLogContext()
                         .WriteTo.Console()
                         .CreateLogger();


            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {              
                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<GetRandomNumberConsumer>();

                        x.AddConsumer<GenerateThingConsumer>();

                        x.SetKebabCaseEndpointNameFormatter();

                        x.UsingRabbitMq((context, cfg) => cfg.ConfigureEndpoints(context));
                    });
                })
                .UseSerilog()
                .Build();

            var busControl = host.Services.GetRequiredService<IBusControl>();

            await busControl.StartAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
            try
            {
                Console.WriteLine("Press enter to exit");

                await Task.Run(() => Console.ReadLine());
            }
            finally
            {
                await busControl.StopAsync();
            }
        }

        static IConfigurationBuilder BuildConfig(IConfigurationBuilder builder)
        {
            return builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
        }
    }
}
