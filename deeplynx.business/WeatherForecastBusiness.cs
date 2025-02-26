using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.business;

public class WeatherForecastBusiness : IWeatherForecastBusiness
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    
    public WeatherForecastBusiness()
    {
    }

    public IEnumerable<WeatherForecast> GetWeatherForecast()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }

}