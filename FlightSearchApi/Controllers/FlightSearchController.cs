using FlightSearchApi.Models.FlightSearch;
using FlightSearchApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlightSearchApi.Controllers;

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

        [HttpGet("search/stream")]
        public IAsyncEnumerable<FlightSearchResponse> StreamFlights([FromQuery] FlightSearchRequest request, CancellationToken cancellationToken)
        {
            return _flightSearchService.StreamFlightsAsync(request, cancellationToken);
        }
    }
    