using EasyNetQ;
using EasyNetQ.Consumer;
using System.Text;

namespace EasyNetQTest.Bus.Configuration
{
    public sealed class RetryConsumerErrorStrategy : IConsumeErrorStrategy
    {
        private readonly IBus _bus;
        private readonly ILogger<RetryConsumerErrorStrategy> _logger;

        public RetryConsumerErrorStrategy(IBus bus, ILogger<RetryConsumerErrorStrategy> logger)
        {
            _bus = bus;
            _logger = logger;
        }

        public async ValueTask<AckStrategyAsync> HandleErrorAsync(ConsumeContext context, Exception exception, CancellationToken cancellationToken = default)
        {
            var headers = context.Properties.Headers;
            int currentAttempt = 2;
            if (headers != null && headers.TryGetValue("x-attempt", out var attemptObj) && attemptObj is byte[] attemptBytes)
            {
                var attemptStr = Encoding.UTF8.GetString(attemptBytes);
                if (int.TryParse(attemptStr, out var parsedAttempt))
                    currentAttempt = parsedAttempt + 1;
            }            

            if (currentAttempt <= 5)
            {
                _logger.LogWarning("Exception occurred while processing message: {message} of type: {type} in queue: {queue} attempt: {currentAttempt} exception: {exceptionMessage}. Requeuing",
                                    Encoding.UTF8.GetString(context.Body.ToArray()),
                                    context.Properties.Type,
                                    context.ReceivedInfo.Queue,
                                    currentAttempt,
                                    exception.Message);

                var newHeaders = CreateHeadersWithUpdatedProperties(headers, currentAttempt, exception);                

                await _bus.Advanced.PublishAsync(
                    exchange: "",
                    routingKey: context.ReceivedInfo.Queue,
                    mandatory: false,
                    publisherConfirms: true,
                    properties: new MessageProperties
                    {
                        Headers = newHeaders,
                        Type = context.Properties.Type,
                        CorrelationId = context.Properties.CorrelationId,
                        DeliveryMode = context.Properties.DeliveryMode,

                    },
                    body: context.Body,
                    cancellationToken: cancellationToken
                );

                return AckStrategies.AckAsync;
            }
            else
            {
                _logger.LogWarning("Exception occurred while processing message: {message} of type: {type} in queue: {queue} attempt: {currentAttempt} exception: {exceptionMessage}. Moving to dead letter",
                                    Encoding.UTF8.GetString(context.Body.ToArray()),
                                    context.Properties.Type,
                                    context.ReceivedInfo.Queue,
                                    currentAttempt,
                                    exception.Message);

                return AckStrategies.NackWithoutRequeueAsync;
            }
        }

        public ValueTask<AckStrategyAsync> HandleCancelledAsync(ConsumeContext context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private Dictionary<string, object> CreateHeadersWithUpdatedProperties(IDictionary<string, object>? headers, int currentAttempt, Exception exception)
        {
            var newHeaders = new Dictionary<string, object>(headers ?? new Dictionary<string, object>())
            {
                ["x-attempt"] = Encoding.UTF8.GetBytes(currentAttempt.ToString()),
                ["x-delay"] = currentAttempt * 10000
            };

            HandleExceptionHeaders(newHeaders, exception);

            return newHeaders;
        }

        private void HandleExceptionHeaders(Dictionary<string, object> headers, Exception exception)
        {
            if (headers == null)
                return;

            if (headers.ContainsKey("ExceptionMessage"))
            {
                headers.Remove("ExceptionMessage");
                headers.Remove("ExceptionStackTrace");
                headers.Remove("ExceptionInnerException");
                headers.Remove("ExceptionDate");
            }

            headers["ExceptionMessage"] = Encoding.UTF8.GetBytes(exception.Message);
            headers["ExceptionStackTrace"] = Encoding.UTF8.GetBytes(exception.StackTrace ?? string.Empty);
            headers["ExceptionInnerException"] = Encoding.UTF8.GetBytes(exception.InnerException?.ToString() ?? string.Empty);
            headers["ExceptionDate"] = Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("o"));
        }
    }
}