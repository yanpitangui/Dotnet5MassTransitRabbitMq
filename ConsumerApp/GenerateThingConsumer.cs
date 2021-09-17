using Bogus;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;
using Microsoft.Extensions.Logging;
using Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerApp
{
    internal class GenerateThingConsumer : IConsumer<Batch<GenerateThing>>
    {
        private readonly ILogger<GenerateThingConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public GenerateThingConsumer(ILogger<GenerateThingConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<Batch<GenerateThing>> context)
        {
            var stopWatch = new Stopwatch();

            stopWatch.Start();

            var thingFactory = new Faker<ThingGenerated>()
                .RuleFor(e => e.Value, (f, u) => f.Name.FirstName());
            var thingsGenerated = new ConcurrentBag<ThingGenerated>();

            Parallel.ForEach(Partitioner.Create(0, context.Message.Length, 500), range =>
            {

                var tempGenerated = thingFactory.GenerateLazy(range.Item2 - range.Item1);
                tempGenerated.AsParallel().ForAll(t => thingsGenerated.Add(t));
            });

            await _publishEndpoint.PublishBatch(thingsGenerated);

            stopWatch.Stop();

            _logger.LogInformation($"Time took to generate {context.Message.Length} things: {stopWatch.ElapsedMilliseconds} ms");

        }
    }


    internal class GenerateThingConsumerDefinition : ConsumerDefinition<GenerateThingConsumer>
    {
        public GenerateThingConsumerDefinition()
        {
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<GenerateThingConsumer> consumerConfigurator)
        {
            consumerConfigurator.Options<BatchOptions>(options => options
                .SetMessageLimit(2000)
                .SetTimeLimit(1000)
                .SetConcurrencyLimit(24));
        }
    }
}
