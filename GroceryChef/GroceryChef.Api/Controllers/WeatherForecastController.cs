using Microsoft.AspNetCore.Mvc;

namespace GroceryChef.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public IActionResult Get()
    {
        return NoContent();
    }
}