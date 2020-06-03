using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SKA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WhereAmIController : ControllerBase
    {
        private readonly IGeoIpService _geoIpService;

        public WhereAmIController(IGeoIpService geoIpService)
        {
            _geoIpService = geoIpService;
        }

        [HttpGet, ServiceFilter(typeof(LogFilter)), ResponseCache(Duration = 30)]
        public async Task<IActionResult> Index()
        {
            var currentIp = await _geoIpService.GetCurrentIpAsync();
            return Ok(new
            {
                Ip = currentIp,
                CountryCode = "TW"
            });
        }
    }

    public interface IGeoIpService
    {
        Task<string> GetCurrentIpAsync();
    }

    public class GeoIpService : IGeoIpService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GeoIpService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetCurrentIpAsync()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var result = await httpClient.GetAsync("https://api.ipify.org?format=json");
            result.EnsureSuccessStatusCode();
            var currentIp = JsonConvert.DeserializeObject<GeoResponse>(await result.Content.ReadAsStringAsync());
            return currentIp.Ip;

        }
    }

    public class GeoResponse
    {
        public string Ip { get; set; }
    }

    class LogFilter : ActionFilterAttribute
    {
        private readonly ILogger<LogFilter> _logger;

        public LogFilter(ILogger<LogFilter> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.Log(LogLevel.Information, "in");
            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            _logger.Log(LogLevel.Information, "out");
        }
    }
}