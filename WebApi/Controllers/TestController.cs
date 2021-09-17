using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly IRequestClient<GetRandomNumber> _client;
        private readonly IPublishEndpoint _publishEndpoint;

        public TestController(ILogger<TestController> logger, IRequestClient<GetRandomNumber> client, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _client = client;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet("response")]
        public async Task<ActionResult> GetAsync()
        {
            var number = await _client.GetResponse<RandomNumber>(new GetRandomNumber());
            return Ok(number.Message);
        }


        [HttpGet("producer-consumer")]
        public async Task<ActionResult> ProducerConsumer(int quantity = 5)
        {
            var thingsToGenerate = Enumerable.Range(1, quantity).Select(x => new GenerateThing());

            await _publishEndpoint.PublishBatch(thingsToGenerate);
            return Ok();
        }
    }
}
