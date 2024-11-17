using AybJetProviderApi.Models;
using AybJetProviderApi.Models.Booking;
using AybJetProviderApi.Models.FlightSearch;

namespace AybJetProviderApi.Services
{
    public interface IAybJetService
    {
        Task<bool> BookFlightAsync(BookingRequest request, CancellationToken cancellationToken = default);
        Task<List<FlightSearchResponse>> SearchFlightsAsync(FlightSearchRequest request, CancellationToken cancellationToken = default);
        IAsyncEnumerable<FlightSearchResponse> StreamFlightsAsync(FlightSearchRequest request, CancellationToken cancellationToken = default);
    }
}
