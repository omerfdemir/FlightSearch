using FllightSearchApi.Models.FlightSearch;

namespace HopeAirProviderService.Services;

public interface IHopeAirService
{
    Task<List<FlightSearchResponse>> SearchFlightsAsync(FlightSearchRequest request);
}