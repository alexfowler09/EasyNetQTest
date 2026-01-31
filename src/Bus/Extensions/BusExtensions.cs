using EasyNetQ;

namespace EasyNetQTest.Bus.Extensions
{
    public static class BusExtensions
    {
        extension(IBus bus)
        {
            public async Task PublishAsync<T>(                
                string exchange,
                T eventMessage,
                CancellationToken cancellationToken)
            {
                await bus.Advanced.PublishAsync(
                     exchange: exchange,
                     routingKey: string.Empty,
                     mandatory: false,
                     publisherConfirms: true,
                     message: new Message<T>(eventMessage),
                     cancellationToken: cancellationToken);
            }
        }
    }
}