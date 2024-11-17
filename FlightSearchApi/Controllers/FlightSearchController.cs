using FllightSearchApi.Models.FlightSearch;
using FllightSearchApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FllightSearchApi.Controllers;

    [ApiController]
    [Route("[controller]")]
    public class FlightSearchController : ControllerBase
    {
        private readonly IFlightSearchService _flightSearchService;

        public FlightSearchController(IFlightSearchService flightSearchService)
        {
            _flightSearchService = flightSearchService;
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchFlights([FromBody] FlightSearchRequest request)
        {
            var flights = await _flightSearchService.SearchFlightsAsync(request);
            return Ok(flights);
        }
    }
    