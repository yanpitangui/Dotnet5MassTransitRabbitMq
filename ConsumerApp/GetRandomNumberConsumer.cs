using MassTransit;
using Microsoft.Extensions.Logging;
using Shared;
using System;
using System.Threading.Tasks;

namespace ConsumerApp
{
    public class GetRandomNumberConsumer : IConsumer<GetRandomNumber>
    {
        private readonly Random _random;
        private readonly ILogger<GetRandomNumberConsumer> _logger;

        public GetRandomNumberConsumer(ILogger<GetRandomNumberConsumer> logger)
        {
           _random = new Random();
            this._logger = logger;
        }

        public async Task Consume(ConsumeContext<GetRandomNumber> context)
        {
           var number = new RandomNumber(_random.Next());
           _logger.LogInformation($"Generated new number {number.value}");
           await context.RespondAsync(number);
        }
    }
}