using FllightSearchApi.Models.FlightSearch;

namespace FllightSearchApi.Services;

public interface IFlightSearchService
{
    Task<List<FlightSearchResponse>> SearchFlightsAsync(FlightSearchRequest request);
}