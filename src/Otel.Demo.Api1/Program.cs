using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Otel.Demo.Api1.Clients;
using Otel.Demo.Api1.Configurations;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOpenTelemetry(builder.Configuration);

builder.Services.AddLogging(logBuilder => logBuilder.UseAzureMonitor(builder.Configuration));
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.All;
});

builder.Services.AddOpenMeteoClient(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpLogging();

app.MapGet("/weatherforecast-api1",
    async ([FromServices] ILoggerFactory loggerFactory,
    [FromServices] IOpenMeteoApiClient openMeteoApiClient,
    decimal? latitude,
    decimal? longitude) =>
{
    latitude ??= 52.52m;
    longitude ??= 13.41m;

    using var activity = DiagnosticsConfig.ActivitySource.StartActivity("RequestWeatherForecast");

    activity?.SetTag("latitude", latitude);
    activity?.SetTag("longitude", longitude);

    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogInformation("Running API 1 endpoint {at}", DateTime.UtcNow);
    var weatherForecastResponse = await openMeteoApiClient.GetWeatherForecast(latitude.Value, longitude.Value);

    SaveWeatherForecastRequest(weatherForecastResponse);

    return Results.Ok(weatherForecastResponse);
})
.WithName("GetWeatherForecastApi1")
.WithOpenApi();

app.MapPost("/weatherforecast-api1",
    async ([FromServices] ILoggerFactory loggerFactory, [FromBody] SampleBody sampleBody) =>
    {        
        await Task.Yield();
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogInformation("Running API 1 POST endpoint {at}", DateTime.UtcNow);        

        return Results.Ok();
    })
.WithName("PostWeatherForecastApi1")
.WithOpenApi();

app.Run();

void SaveWeatherForecastRequest(WeatherForecastResponse weatherForecastResponse)
{
    using (var dbConn = new MySqlConnection("Server=db;Database=oteldemo;Uid=user;Pwd=password;"))
    {
        dbConn.Open();
        var command = new MySqlCommand("INSERT INTO WeatherForecastRequests (ResponseBody, RequestedAt) VALUES (@responseBody, @date)", dbConn);
        command.Parameters.AddWithValue("responseBody", JsonSerializer.Serialize(weatherForecastResponse));
        command.Parameters.AddWithValue("date", DateTimeOffset.Now);
        var reader = command.ExecuteNonQuery();        
    }
}

public record SampleBody(Guid id, string name);