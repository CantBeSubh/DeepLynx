using deeplynx.models;

namespace deeplynx.interfaces;

public interface IWeatherForecastBusiness
{
    IEnumerable<WeatherForecast> GetWeatherForecast();
}