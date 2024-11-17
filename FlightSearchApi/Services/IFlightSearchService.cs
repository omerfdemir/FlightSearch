using FlightSearchApi.Models.FlightSearch;

namespace FlightSearchApi.Services;

public interface IFlightSearchService
{
    Task<List<FlightSearchResponse>> SearchFlightsAsync(FlightSearchRequest request);
}