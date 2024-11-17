using AybJetProviderApi.Models.Booking;
using AybJetProviderApi.Models.FlightSearch;

namespace AybJetProviderApi.Services;


    public interface IAybJetService
    {
        List<FlightSearchResponse> SearchFlights(FlightSearchRequest request);
        IAsyncEnumerable<FlightSearchResponse> StreamFlightsAsync(FlightSearchRequest request, CancellationToken cancellationToken);
        Task<bool> BookFlightAsync(BookingRequest request);
    }
