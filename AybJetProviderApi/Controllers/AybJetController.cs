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
    public async Task<IActionResult> Search([FromBody] FlightSearchRequest request, CancellationToken cancellationToken)
    {
        var flights = await _aybJetService.SearchFlightsAsync(request, cancellationToken);
        return Ok(flights);
    }

    [HttpGet("search/stream")]
    public IAsyncEnumerable<FlightSearchResponse> StreamFlights([FromQuery] FlightSearchRequest request, CancellationToken cancellationToken)
    {
        return _aybJetService.StreamFlightsAsync(request, cancellationToken);
    }

    [HttpPost("book")]
    public async Task<IActionResult> BookFlight([FromBody] BookingRequest request, CancellationToken cancellationToken)
    {
        var success = await _aybJetService.BookFlightAsync(request, cancellationToken);
        if (!success)
        {
            return NotFound(new { message = "Flight not available" });
        }
        return Ok(new { message = "Booking successful" });
    }
}



