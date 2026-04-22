using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ProductServiceApp.Api.Controllers.Base.BaseCrudApiController;
using ProductServiceApp.Application.ApiResponse;

namespace ProductServiceApp.Api.Controllers;

[ApiVersion("1.0")]
public class WeatherForecastController : BaseCrudApiController<
    WeatherForecast,
    WeatherForecast,
    WeatherForecast,
    WeatherForecast,
    WeatherForecast>
{

    public override async Task<IActionResult> Create([FromBody] WeatherForecast request)
    {
        throw new NotImplementedException();
    }

    public override async Task<IActionResult> Delete(Guid id)
    {
        throw new NotImplementedException();
    }

    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WeatherForecast>>), StatusCodes.Status200OK)]
    public override async Task<IActionResult> GetAll()
    {
        string[] Summaries =
            [
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            ];

        var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

        return Ok(ApiResponse<IEnumerable<WeatherForecast>>.Success(result));
    }

    public override async Task<IActionResult> GetById(Guid id)
    {
        throw new NotImplementedException();
    }

    public override async Task<IActionResult> Update(Guid id, [FromBody] WeatherForecast request)
    {
        throw new NotImplementedException();
    }
}
