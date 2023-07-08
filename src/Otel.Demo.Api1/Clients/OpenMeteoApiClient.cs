using System;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using System.Web;

namespace Otel.Demo.Api1.Clients;

public interface IOpenMeteoApiClient
{
    Task<WeatherForecastResponse> GetWeatherForecast(decimal latitude, decimal longitude, CancellationToken cancellationToken = default);
}

public class OpenMeteoApiClient : IOpenMeteoApiClient
{
    public HttpClient Client { get; set; }

    public OpenMeteoApiClient(IHttpClientFactory httpClientFactory)
    {
        Client = httpClientFactory.CreateClient("open-meteo-client");
    }

    public async Task<WeatherForecastResponse> GetWeatherForecast(decimal latitude, decimal longitude, CancellationToken cancellationToken = default)
    {
        var queryString = HttpUtility.ParseQueryString("");
        queryString["latitude"] = latitude.ToString(CultureInfo.InvariantCulture);
        queryString["longitude"] = longitude.ToString(CultureInfo.InvariantCulture);
        FormattableString requestUri = $"v1/forecast?latitude={latitude}&longitude={longitude}&current_weather=true";

        var httpResponse = await Client.GetAsync(FormattableString.Invariant(requestUri), cancellationToken: cancellationToken);        
        httpResponse.EnsureSuccessStatusCode();

        return await httpResponse.Content.ReadFromJsonAsync<WeatherForecastResponse>(cancellationToken: cancellationToken);
    }
}


public class WeatherForecastResponse
{
    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    [JsonPropertyName("generationtime_ms")]
    public decimal GenerationtimeMs { get; set; }

    [JsonPropertyName("utc_offset_seconds")]
    public int UtcOffsetSeconds { get; set; }

    public string Timezone { get; set; }

    [JsonPropertyName("timezone_abbreviation")]
    public string TimezoneAbbreviation { get; set; }

    public decimal Elevation { get; set; }

    [JsonPropertyName("current_weather")]
    public CurrentWeather CurrentWeather { get; set; }
}

public class CurrentWeather
{
    public decimal Temperature { get; set; }

    public decimal Windspeed { get; set; }

    public decimal Winddirection { get; set; }

    public int Weathercode { get; set; }

    [JsonPropertyName("is_day")]
    public int IsDay { get; set; }

    public DateTime Time { get; set; }
}
