using Microsoft.AspNetCore.Mvc;
using TempServer.Models;

namespace TempServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        // loggtjänst för att logga händelser och fel.
        private readonly ILogger<WeatherForecastController> _logger;

        // Konstruktor som tar emot loggtjänsten via dependency injection.
        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        // Metod som svarar på HTTP GET-förfrågningar.
        // Attributet "Name" används för att ge en identifierare till denna HTTP-route.
        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            // Generera och returnera en lista med väderprognoser.
            var rng = new Random();
            var forecasts = new List<WeatherForecast>();
            for (var i = 1; i <= 5; i++)
            {
                var forecast = new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(i),
                    TemperatureC = rng.Next(-20, 55), // Generera en temperatur mellan -20 och 55 grader Celsius.
                    Summary = "Sunny", //konstant väderbeskrivning.
                };
                forecasts.Add(forecast);
            }
            return forecasts;
        }

        // Metod som svarar på HTTP POST-förfrågningar.
        // Attributet "Name" används för att ge en identifierare till denna HTTP-route.
        [HttpPost(Name = "AddWeatherForecast")]
        public IActionResult Post(WeatherForecast forecast)
        {
            // exempel
            return Ok("Väderprognos har lagts till");
        }
    }
}
