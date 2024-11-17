using FllightSearchApi.Models.FlightSearch;

namespace AybJetProviderApi.Services;


    public interface IAybJetService
    {
        List<FlightSearchResponse> SearchFlights(FlightSearchRequest request);
    }
