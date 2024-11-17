using FlightBookApi.Models.Booking;

namespace FlightBookApi.Services;

public interface IFlightBookService
{
    Task<BookingResponse> BookFlightAsync(BookingRequest request, CancellationToken cancellationToken);
}