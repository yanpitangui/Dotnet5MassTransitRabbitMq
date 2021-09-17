using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;
using Microsoft.Extensions.Logging;
using Shared;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace WebApi
{
    public class ThingGeneratedConsumer : IConsumer<Batch<ThingGenerated>>
    {
        private readonly ILogger<ThingGeneratedConsumer> _logger;

        public ThingGeneratedConsumer(ILogger<ThingGeneratedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<Batch<ThingGenerated>> context)
        {
            _logger.LogInformation($"Things generated: {context.Message.Length}");

            return Task.CompletedTask;
        }


    }

    internal class ThingGeneratedConsumerDefinition : ConsumerDefinition<ThingGeneratedConsumer>
    {
        public ThingGeneratedConsumerDefinition()
        {
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<ThingGeneratedConsumer> consumerConfigurator)
        {
            consumerConfigurator.Options<BatchOptions>(options => options
                .SetMessageLimit(5000)
                .SetTimeLimit(1000)
                .SetConcurrencyLimit(24));
        }
    }
}
