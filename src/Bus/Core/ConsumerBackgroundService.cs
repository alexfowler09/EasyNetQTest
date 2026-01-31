using EasyNetQ;
using EasyNetQ.Topology;

namespace EasyNetQTest.Bus.Core
{
    public abstract class ConsumerBackgroundService<TMessage> : BackgroundService
        where TMessage : class

    {
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly ushort _prefetchCount;
        private readonly IBus _bus;
        private readonly ILogger<ConsumerBackgroundService<TMessage>> _logger;

        private IAsyncDisposable? receiveRegistration;

        public ConsumerBackgroundService(
            string exchangeName,
            string queueName,            
            ushort prefetchCount,
            IBus bus,
            ILogger<ConsumerBackgroundService<TMessage>> logger)
        {            
            _exchangeName = exchangeName;
            _queueName = queueName;
            _prefetchCount = prefetchCount;
            _bus = bus;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var deadLetterExchange = Exchange.Default;

            var deadLetterQueueName = $"{_queueName}-dead-letter";

            var deadLetterQueue = await _bus.Advanced.QueueDeclareAsync(
                deadLetterQueueName,
                stoppingToken);            

            var deadLetterRoutingKey = deadLetterQueueName;            

            Queue queue;

            if (string.IsNullOrWhiteSpace(_exchangeName))
            {
                queue = await _bus.Advanced.QueueDeclareAsync(
                    _queueName,
                    configure =>
                    {                        
                        configure.AsDurable(true);
                        configure.AsAutoDelete(false);
                        configure.AsExclusive(false);                 
                        configure.WithDeadLetterExchange(deadLetterExchange);
                        configure.WithDeadLetterRoutingKey(deadLetterRoutingKey);                        
                    },
                    stoppingToken
                );
            }
            else
            {                
                var exchange = await _bus.Advanced.ExchangeDeclareAsync(
                    _exchangeName,
                    ExchangeType.Fanout,
                    cancellationToken: stoppingToken);

                queue = await _bus.Advanced.QueueDeclareAsync(
                    _queueName,
                    configure =>
                    {                        
                        configure.AsDurable(true);
                        configure.AsAutoDelete(false);
                        configure.AsExclusive(false);
                        configure.WithDeadLetterExchange(deadLetterExchange);
                        configure.WithDeadLetterRoutingKey(deadLetterRoutingKey);
                    },
                    stoppingToken
                );

                await _bus.Advanced.BindAsync(
                    exchange,
                    queue,                    
                    string.Empty,
                    stoppingToken);
            }

            receiveRegistration = await _bus.Advanced.ConsumeAsync<TMessage>(queue, async (msg, info) =>
            {
                await ProcessMessageAsync(msg.Body, stoppingToken);
            },
            configure =>
            {
                configure.WithPrefetchCount(_prefetchCount);
            });

            stoppingToken.Register(async () =>
            {
                _logger.LogInformation("Stopping consumer: {consumer}", GetType().Name);

                if (receiveRegistration != null)
                    await receiveRegistration.DisposeAsync();
            });
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (receiveRegistration != null)
                await receiveRegistration.DisposeAsync();
            
            await base.StopAsync(cancellationToken);
        }

        protected abstract Task ProcessMessageAsync(TMessage message, CancellationToken cancellationToken);
    }
}
