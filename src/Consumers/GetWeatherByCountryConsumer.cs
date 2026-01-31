using EasyNetQ;
using EasyNetQTest.Bus.Core;
using EasyNetQTest.Constants;
using EasyNetQTest.Messaging.Messages;
using Newtonsoft.Json;

namespace EasyNetQTest.Consumers
{
    public class GetWeatherByCountryConsumer : ConsumerBackgroundService<GetWeatherByCountryMessage>
    {
        private readonly ILogger<GetWeatherByCountryConsumer> _logger;

        public GetWeatherByCountryConsumer(IBus bus, ILogger<GetWeatherByCountryConsumer> logger)
            : base(
                exchangeName: string.Empty,
                queueName: QueueNames.GetWeatherByCountry,                  
                prefetchCount: 10,
                bus: bus,
                logger: logger)
        {
            _logger = logger;
        }

        protected override Task ProcessMessageAsync(GetWeatherByCountryMessage message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received GetWeatherByCountryConsumer: {message}", JsonConvert.SerializeObject(message));            
            return Task.CompletedTask;
        }
    }    
}
