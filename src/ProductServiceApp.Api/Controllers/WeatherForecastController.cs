using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ProductServiceApp.Api.Controllers.Base.Operations;
using ProductServiceApp.Domain.Controller.BaseApiResponse;
using System.Threading.Channels;

namespace ProductServiceApp.Api.Controllers;

[ApiVersion("1.0")]
public class WeatherForecastController(
    Channel<(WeatherForecast, TaskCompletionSource<IEnumerable<WeatherForecast>>, CancellationToken)> getAllChannel)
    : BaseGetAllController<WeatherForecast, WeatherForecast>(getAllChannel)
{
    protected override WeatherForecast BuildGetAllQuery() => new();

    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WeatherForecast>>), StatusCodes.Status200OK)]
    public override async Task<IActionResult> GetAll(CancellationToken ct)
    {
        string[] summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
        }).ToArray();

        return Ok(ApiResponse<IEnumerable<WeatherForecast>>.Success(result));
    }
}