using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SKA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WhereAmIController : ControllerBase
    {
        public async Task<IActionResult> Index()
        {
            return Ok(new
            {
                Ip = "8.8.8.8",
                CountryCode = "TW"
            });
        }
    }

    public class GeoResponse
    {
        public string CountryCode { get; set; }
        public string Ip { get; set; }
    }
}