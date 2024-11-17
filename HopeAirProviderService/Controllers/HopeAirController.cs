using FllightSearchApi.Models.FlightSearch;
using HopeAirProviderService.Services;
using Microsoft.AspNetCore.Mvc;

namespace HopeAirProviderService.Controllers;

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
}