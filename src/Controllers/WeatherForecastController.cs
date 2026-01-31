using EasyNetQ;
using EasyNetQTest.Bus.Extensions;
using EasyNetQTest.Constants;
using EasyNetQTest.Messaging.Events;
using EasyNetQTest.Messaging.Messages;
using EasyNetQTest.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasyNetQTest.Controllers
{
    [ApiController]
    [Route("/api/weather")]
    public class WeatherForecastController : ControllerBase
    {        
        private readonly IBus _bus;        

        public WeatherForecastController(IBus bus)
        {            
            _bus = bus;
        }

        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            return [.. Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })];
        }


        [HttpGet("country")]
        public async Task<IActionResult> GetByCountry(string country, CancellationToken cancellationToken)
        {
            var message = new GetWeatherByCountryMessage { Country = country };
            await _bus.SendReceive.SendAsync(QueueNames.GetWeatherByCountry, message, cancellationToken);
            return Accepted();
        }

        [HttpGet("city")]
        public async Task<IActionResult> GetByCity(string country, string state, string city, CancellationToken cancellationToken)
        {
            var message = new GetWeatherByCityMessage { Country = country, State = state, City = city };
            await _bus.SendReceive.SendAsync(QueueNames.GetWeatherByCity, message);
            return Accepted();
        }

        [HttpPost("CreateWeatherForecast")]
        public async Task<IActionResult> CreateWeatherForecast(CancellationToken cancellationToken)
        {            
            var forecast = new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            };
            
            await Task.Delay(100, cancellationToken);

            var weatherForecastCreatedEvent = new WeatherForecastCreatedEvent
            {
                Date = forecast.Date,
                TemperatureC = forecast.TemperatureC,
                Summary = forecast.Summary
            };

            await _bus.PublishAsync(
                ExchangeNames.WeatherForecastCreated,              
                weatherForecastCreatedEvent,
                cancellationToken);            

            return Ok();
        }
    }
}
