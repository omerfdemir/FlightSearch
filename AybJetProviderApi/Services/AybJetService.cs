using AybJetProviderApi.Models;
using AybJetProviderApi.Models.Booking;
using AybJetProviderApi.Models.FlightSearch;
using AybJetProviderApi.Services.Cache;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace AybJetProviderApi.Services
{
    public class AybJetService: IAybJetService
    {
        private readonly ILogger<AybJetService> _logger;
        private readonly ICache _cache;
        private const string FLIGHTS_CACHE_KEY = "AybJet_Flights";

        public AybJetService(ILogger<AybJetService> logger, ICache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public async Task<bool> BookFlightAsync(BookingRequest request, CancellationToken cancellationToken = default)
        {
            var flights = _cache.Get<List<FlightSearchResponse>>(FLIGHTS_CACHE_KEY);
            
            if (flights == null)
            {
                _logger.LogWarning($"Flight {request.FlightNumber} not found in cache");
                return false;
            }

            var flight = flights.FirstOrDefault(f => f.FlightNumber == request.FlightNumber);
            if (flight == null)
            {
                _logger.LogWarning($"Flight {request.FlightNumber} not found in cache");
                return false;
            }

            flights.Remove(flight);
            _cache.Set(FLIGHTS_CACHE_KEY, flights);
            _logger.LogInformation($"Flight {request.FlightNumber} booked and removed from cache");
            return true;
        }

        public async Task<List<FlightSearchResponse>> SearchFlightsAsync(FlightSearchRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var filePath = Path.Combine("MockData", "AybJet-Provider-Response.json");
                var jsonContent = await File.ReadAllTextAsync(filePath, cancellationToken);
                var allFlights = JsonSerializer.Deserialize<List<FlightSearchResponse>>(jsonContent);

                if (allFlights != null)
                {
                    foreach (var flight in allFlights)
                    {
                        flight.ProviderName = "AybJet";
                    }
                }

                var filteredFlights = allFlights?
                    .Where(f => f.Departure == request.Origin && 
                               f.Arrival == request.Destination &&
                               (request.DepartureDate == default || 
                                f.DepartureTime.Date == request.DepartureDate.Date))
                    .ToList() ?? new List<FlightSearchResponse>();

                _cache.Set(FLIGHTS_CACHE_KEY, filteredFlights);
                return filteredFlights;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading mock data from AybJet-Provider-Response.json");
                return new List<FlightSearchResponse>();
            }
        }

        public async IAsyncEnumerable<FlightSearchResponse> StreamFlightsAsync(
            FlightSearchRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var flights = await SearchFlightsAsync(request, cancellationToken);
            foreach (var flight in flights)
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;
                    
                yield return flight;
            }
        }
    }
}