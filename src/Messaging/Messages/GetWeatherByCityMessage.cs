namespace EasyNetQTest.Messaging.Messages
{
    public class GetWeatherByCityMessage
    {
        public string Country { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }
}