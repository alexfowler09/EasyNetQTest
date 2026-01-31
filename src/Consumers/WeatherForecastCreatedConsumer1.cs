using EasyNetQ;
using EasyNetQTest.Bus.Core;
using EasyNetQTest.Constants;
using EasyNetQTest.Messaging.Events;
using Newtonsoft.Json;

namespace EasyNetQTest.Consumers
{
    public class WeatherForecastCreatedConsumer1 : ConsumerBackgroundService<WeatherForecastCreatedEvent>
    {
        private readonly ILogger<WeatherForecastCreatedConsumer1> _logger;
        
        public WeatherForecastCreatedConsumer1(IBus bus, ILogger<WeatherForecastCreatedConsumer1> logger)
            : base(
                exchangeName: ExchangeNames.WeatherForecastCreated,
                queueName: QueueNames.WeatherForecastCreated1,
                prefetchCount: 10,
                bus: bus,
                logger: logger)
        {
            _logger = logger;
        }
        
        protected override Task ProcessMessageAsync(WeatherForecastCreatedEvent message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received WeatherForecastCreated1: {@message}", JsonConvert.SerializeObject(message));            
            return Task.CompletedTask;
        }
    }    
}
