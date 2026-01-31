using EasyNetQ;
using EasyNetQ.Topology;

namespace EasyNetQTest.Bus.Configuration
{
    public class AvoidExchangeDeclareStrategy : IExchangeDeclareStrategy
    {
        public Task<Exchange> DeclareExchangeAsync(string exchangeName, string exchangeType, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Exchange.Default);
        }

        public Task<Exchange> DeclareExchangeAsync(Type messageType, string exchangeType, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Exchange.Default);
        }        
    }
}
