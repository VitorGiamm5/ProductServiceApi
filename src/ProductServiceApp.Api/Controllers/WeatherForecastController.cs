using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ProductServiceApp.Api.Controllers.Base;
using ProductServiceApp.Application.ApiResponse;

namespace ProductServiceApp.Api.Controllers;

[ApiVersion("1.0")]
public class WeatherForecastController : BaseApiController
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [HttpGet(Name = "GetWeatherForecast")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WeatherForecast>>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

        return Ok(ApiResponse<IEnumerable<WeatherForecast>>.Success(result));
    }
}
