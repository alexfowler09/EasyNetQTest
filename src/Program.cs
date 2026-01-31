using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQTest.Consumers;
using EasyNetQTest.Bus.Configuration;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<IExchangeDeclareStrategy, AvoidExchangeDeclareStrategy>();
builder.Services.AddSingleton<IConsumeErrorStrategy, RetryConsumerErrorStrategy>();
builder.Services.AddEasyNetQ(builder.Configuration.GetConnectionString("rabbitmq")).UseNewtonsoftJson();

builder.Services.AddHostedService<GetWeatherByCountryConsumer>();
builder.Services.AddHostedService<GetWeatherByCityConsumer>();
builder.Services.AddHostedService<WeatherForecastCreatedConsumer1>();
builder.Services.AddHostedService<WeatherForecastCreatedConsumer2>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
//{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "EasyNetQ Test");
    });
//}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
