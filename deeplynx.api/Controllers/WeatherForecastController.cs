using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IWeatherForecastBusiness _weatherForecast;
    public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherForecastBusiness weatherForecast)
    {
        _logger = logger;
        _weatherForecast = weatherForecast;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IActionResult> Get()
    {
       return  Ok(_weatherForecast.GetWeatherForecast());
        
    }
}
