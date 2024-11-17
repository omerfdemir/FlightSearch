using HopeAirProviderApi.Models.Booking;
using HopeAirProviderApi.Models.FlightSearch;

namespace HopeAirProviderApi.Services;

public interface IHopeAirService
{
    Task<List<FlightSearchResponse>> SearchFlightsAsync(FlightSearchRequest request);
    IAsyncEnumerable<FlightSearchResponse> StreamFlightsAsync(FlightSearchRequest request, CancellationToken cancellationToken);
    Task<bool> BookFlightAsync(BookingRequest request);
}