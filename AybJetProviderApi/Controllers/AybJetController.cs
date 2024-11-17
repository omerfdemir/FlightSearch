using AybJetProviderApi.Services;
using FllightSearchApi.Models.FlightSearch;
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
}



