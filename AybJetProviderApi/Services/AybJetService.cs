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
            var flights = new List<FlightSearchResponse>
            {
                new FlightSearchResponse 
                { 
                    FlightNumber = "AY123",
                    Departure = request.Origin,
                    Arrival = request.Destination,
                    Price = 500.00m,
                    Currency = "USD",
                    Duration = "8h"
                }
            };

            _cache.Set(FLIGHTS_CACHE_KEY, flights);
            return flights;
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