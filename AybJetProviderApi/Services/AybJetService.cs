using AybJetProviderApi.Services;
using FllightSearchApi.Models.FlightSearch;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AybJetProviderService.Services
{
    public class AybJetService : IAybJetService
    {
        private readonly ILogger<AybJetService> _logger;

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