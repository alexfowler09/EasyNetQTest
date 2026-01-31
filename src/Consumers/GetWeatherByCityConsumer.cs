using EasyNetQ;
using EasyNetQTest.Bus.Core;
using EasyNetQTest.Constants;
using EasyNetQTest.Messaging.Messages;
using Newtonsoft.Json;

namespace EasyNetQTest.Consumers
{
    public class GetWeatherByCityConsumer : ConsumerBackgroundService<GetWeatherByCityMessage>
    {
        private readonly ILogger<GetWeatherByCityConsumer> _logger;

        public GetWeatherByCityConsumer(IBus bus, ILogger<GetWeatherByCityConsumer> logger)
            : base(
                exchangeName: string.Empty,
                queueName: QueueNames.GetWeatherByCity,
                prefetchCount: 10,
                bus: bus,
                logger: logger)
        {
            _logger = logger;
        }
        protected override Task ProcessMessageAsync(GetWeatherByCityMessage message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received GetWeatherByCityMessage: {@message}", JsonConvert.SerializeObject(message));
            return Task.CompletedTask;            
        }
    }    
}
