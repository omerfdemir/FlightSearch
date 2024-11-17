using HopeAirProviderApi.Models.Booking;
using HopeAirProviderApi.Models.FlightSearch;
using HopeAirProviderApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HopeAirProviderApi.Controllers;

[ApiController]
[Route("[controller]")]
public class HopeAirController : ControllerBase
{
    private readonly IHopeAirService _hopeAirService;

    public HopeAirController(IHopeAirService hopeAirService)
    {
        _hopeAirService = hopeAirService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] FlightSearchRequest request)
    {
        var flights = await _hopeAirService.SearchFlightsAsync(request);
        return Ok(flights);
    }

    [HttpGet("search/stream")]
    public IAsyncEnumerable<FlightSearchResponse> StreamFlights([FromQuery] FlightSearchRequest request, CancellationToken cancellationToken)
    {
        return _hopeAirService.StreamFlightsAsync(request, cancellationToken);
    }

    [HttpPost("book")]
    public async Task<IActionResult> BookFlight([FromBody] BookingRequest request)
    {
        var success = await _hopeAirService.BookFlightAsync(request);
        if (!success)
        {
            return NotFound(new { message = "Flight not available" });
        }
        return Ok(new { message = "Booking successful" });
    }
}