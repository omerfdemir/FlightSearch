using AybJetProviderApi.Models.Booking;
using AybJetProviderApi.Models.FlightSearch;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace AybJetProviderApi.Services
{
    public class AybJetService : IAybJetService
    {
        private readonly ILogger<AybJetService> _logger;
        private static readonly List<FlightSearchResponse> _flightCache = new();

        public AybJetService(ILogger<AybJetService> logger)
        {
            _logger = logger;
        }

        public List<FlightSearchResponse> SearchFlights(FlightSearchRequest request)
        {
            var flights = GetMockAybJetFlights();
            
            var filteredFlights = flights
                .Where(f => f.Departure == request.Origin && f.Arrival == request.Destination)
                .ToList();

            return filteredFlights;
        }

        public async IAsyncEnumerable<FlightSearchResponse> StreamFlightsAsync(
            FlightSearchRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var flights = GetMockAybJetFlights();
            
            foreach (var flight in flights.Where(f => 
                f.Departure == request.Origin && 
                f.Arrival == request.Destination))
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;

                await Task.Delay(100, cancellationToken); // Simulate real-time streaming
                yield return flight;
            }
        }

        public async Task<bool> BookFlightAsync(BookingRequest request)
        {
            var flight = _flightCache.FirstOrDefault(f => f.FlightNumber == request.FlightNumber);
            if (flight == null)
            {
                _logger.LogWarning($"Flight {request.FlightNumber} not found in cache");
                return false;
            }

            _flightCache.Remove(flight);
            _logger.LogInformation($"Flight {request.FlightNumber} booked and removed from cache");
            return true;
        }

        private List<FlightSearchResponse> GetMockAybJetFlights()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "MockData", "AybJet-Provider-Response.json");

            try
            {
                var jsonData = File.ReadAllText(filePath);
                var flights = JsonSerializer.Deserialize<List<FlightSearchResponse>>(jsonData);

                return flights ?? new List<FlightSearchResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading mock data from AybJet-Provider-Response.json");
                return new List<FlightSearchResponse>();
            }
        }
    }
}