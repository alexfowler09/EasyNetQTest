using EasyNetQ;
using EasyNetQTest.Bus.Core;
using EasyNetQTest.Constants;
using EasyNetQTest.Messaging.Events;
using Newtonsoft.Json;

namespace EasyNetQTest.Consumers
{
    public class WeatherForecastCreatedConsumer2 : ConsumerBackgroundService<WeatherForecastCreatedEvent>
    {
        private readonly ILogger<WeatherForecastCreatedConsumer2> _logger;
        
        public WeatherForecastCreatedConsumer2(IBus bus, ILogger<WeatherForecastCreatedConsumer2> logger)
            : base(
                exchangeName: ExchangeNames.WeatherForecastCreated,
                queueName: QueueNames.WeatherForecastCreated2,
                prefetchCount: 10,
                bus: bus,
                logger: logger)
        {
            _logger = logger;
        }
        
        protected override Task ProcessMessageAsync(WeatherForecastCreatedEvent message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received WeatherForecastCreated2: {@message}", JsonConvert.SerializeObject(message));                        
            return Task.CompletedTask;
        }
    }    
}
