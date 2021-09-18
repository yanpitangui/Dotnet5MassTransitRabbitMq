using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;
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

            var configuration = builder.Build();
            var elasticSearchUri = configuration.GetValue("ELASTIC_URI", "http://localhost:9200");
            var rabbitMqHost = configuration.GetValue("RABBITMQ_HOST", "localhost");

            Log.Logger = new LoggerConfiguration()
                         .ReadFrom.Configuration(configuration)
                         .Enrich.FromLogContext()
                         .Enrich.FromMassTransit()
                         .WriteTo.Console()
                         .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticSearchUri))
                         {
                            AutoRegisterTemplate = true,
                            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                            IndexFormat = "consumerapp-{0:yyyy.MM}",
                            CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true)
                         })
                         .CreateLogger();


            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {              
                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<GetRandomNumberConsumer>();

                        x.AddConsumer<GenerateThingConsumer>();

                        x.SetKebabCaseEndpointNameFormatter();

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.ConfigureEndpoints(context);
                            cfg.Host(rabbitMqHost);
                            cfg.UseSerilogEnricher();

                        });
                    });
                    services.AddMassTransitHostedService();
                })
                .UseSerilog()
                .Build();

            await host.RunAsync();
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
