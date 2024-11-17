using FlightBookApi.Models.Booking;
using FlightBookApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlightBookApi.Controllers;

    [ApiController]
    [Route("[controller]")]
    public class FlightBookController : ControllerBase
    {
        private readonly IFlightBookService _flightBookService;

        public FlightBookController(IFlightBookService flightBookService)
        {
            _flightBookService = flightBookService;
        }

        [HttpPost("book")]
        public async Task<ActionResult<BookingResponse>> BookFlight(
            [FromBody] BookingRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _flightBookService.BookFlightAsync(request, cancellationToken);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
    