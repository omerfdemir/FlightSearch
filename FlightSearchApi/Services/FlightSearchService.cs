using FllightSearchApi.Models.FlightSearch;

namespace FllightSearchApi.Services;

public class FlightSearchService: IFlightSearchService
{
    private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FlightSearchService> _logger;

        public FlightSearchService(IHttpClientFactory httpClientFactory, ILogger<FlightSearchService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<List<FlightSearchResponse>> SearchFlightsAsync(FlightSearchRequest request)
        {
            using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                var token = cancellationTokenSource.Token;

                var hopeAirTask = GetProviderFlightSearchAsync("HopeAirClient", request, token);
                var aybJetTask = GetProviderFlightSearchAsync("AybJetClient", request, token);

                var tasks = new[] { hopeAirTask, aybJetTask };
                var flightSearchResponses = new List<FlightSearchResponse>();

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("One or more provider services did not respond in time");
                }
                finally
                {
                    foreach (var task in tasks)
                    {
                        if (task.IsCompletedSuccessfully)
                        {
                            flightSearchResponses.AddRange(task.Result);
                        }
                    }
                }

                var sortedFlightSearchResponses = flightSearchResponses.OrderBy(f => f.Price).ToList();
                return sortedFlightSearchResponses;
            }
        }

        private async Task<List<FlightSearchResponse>> GetProviderFlightSearchAsync(string clientName, FlightSearchRequest request, CancellationToken token)
        {
            var client = _httpClientFactory.CreateClient(clientName);

            var uri = "";

            if (clientName == "HopeAirClient")
            {
                uri = "HopeAir/search";
            }
            else if (clientName == "AybJetClient")
            {
                uri = "AybJet/search";
            }

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsJsonAsync(uri, request, token);
            }
            catch (TaskCanceledException)
            {
                _logger.LogError($"{clientName} did not respond in time.");
                return new List<FlightSearchResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while calling {clientName}.");
                return new List<FlightSearchResponse>();
            }

            if (response.IsSuccessStatusCode)
            {
                var flightSearchResponses = await response.Content.ReadFromJsonAsync<List<FlightSearchResponse>>(cancellationToken: token);
                return flightSearchResponses ?? new List<FlightSearchResponse>();
            }

            _logger.LogWarning($"Received unsuccessful status code from {clientName}: {response.StatusCode}");
            return new List<FlightSearchResponse>();
        }
        
}