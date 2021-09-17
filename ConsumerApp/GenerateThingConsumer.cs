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
    internal class GenerateThingConsumer : IConsumer<GenerateThing>
    {
        private readonly ILogger<GenerateThingConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public GenerateThingConsumer(ILogger<GenerateThingConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<GenerateThing> context)
        {
            var stopWatch = new Stopwatch();

            stopWatch.Start();

            var thingFactory = new Faker<ThingGenerated>()
                .RuleFor(e => e.Value, (f, u) => f.Name.FirstName());
            var thingsGenerated = new ConcurrentBag<ThingGenerated>();

            Parallel.ForEach(Partitioner.Create(0, context.Message.Value, 1000), range =>
            {

                var tempGenerated = thingFactory.GenerateLazy(range.Item2 - range.Item1);
                tempGenerated.AsParallel().ForAll(t => thingsGenerated.Add(t));
            });

            _publishEndpoint.PublishBatch(thingsGenerated).ConfigureAwait(false);

            stopWatch.Stop();

            _logger.LogInformation($"Time took to generate {context.Message.Value} things: {stopWatch.ElapsedMilliseconds} ms");

        }
    }
}
