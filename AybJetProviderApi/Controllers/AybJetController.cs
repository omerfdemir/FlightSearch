using AybJetProviderApi.Models.Booking;
using AybJetProviderApi.Models.FlightSearch;
using AybJetProviderApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AybJetProviderApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AybJetController : ControllerBase
{
    private readonly IAybJetService _aybJetService;

    public AybJetController(IAybJetService aybJetService)
    {
        _aybJetService = aybJetService;
    }

    [HttpPost("search")]
    public IActionResult Search([FromBody] FlightSearchRequest request)
    {
        var flights = _aybJetService.SearchFlights(request);
        return Ok(flights);
    }

    [HttpGet("search/stream")]
    public IAsyncEnumerable<FlightSearchResponse> StreamFlights([FromQuery] FlightSearchRequest request, CancellationToken cancellationToken)
    {
        return _aybJetService.StreamFlightsAsync(request, cancellationToken);
    }

    [HttpPost("book")]
    public async Task<IActionResult> BookFlight([FromBody] BookingRequest request)
    {
        var success = await _aybJetService.BookFlightAsync(request);
        if (!success)
        {
            return NotFound(new { message = "Flight not available" });
        }
        return Ok(new { message = "Booking successful" });
    }
}



