using FlightSearchApi.Models.FlightSearch;
using System.Runtime.CompilerServices;
using System.Text.Json;
using FlightSearchApi.Helpers;

namespace FlightSearchApi.Services;

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

        public async IAsyncEnumerable<FlightSearchResponse> StreamFlightsAsync(
            FlightSearchRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(30));

            var hopeAirClient = _httpClientFactory.CreateClient("HopeAirClient");
            var aybJetClient = _httpClientFactory.CreateClient("AybJetClient");

            var hopeAirStream = hopeAirClient.GetStreamAsync("HopeAir/search/stream", cts.Token);
            var aybJetStream = aybJetClient.GetStreamAsync("AybJet/search/stream", cts.Token);

            var hopeAirFlights = StreamProvider(hopeAirStream, "HopeAir", request);
            var aybJetFlights = StreamProvider(aybJetStream, "AybJet", request);

            await foreach (var flight in hopeAirFlights.MergeStreams(aybJetFlights, cts.Token))
            {
                yield return flight;
            }
        }

        private async IAsyncEnumerable<FlightSearchResponse> StreamProvider(
            Task<Stream> streamTask,
            string provider,
            FlightSearchRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Stream? stream = null;
            
            try
            {
                stream = await streamTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting stream from {provider}");
                yield break;
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var enumerable = JsonSerializer.DeserializeAsyncEnumerable<FlightSearchResponse>(
                stream,
                options,
                cancellationToken);
            
            await using var enumerator = enumerable.GetAsyncEnumerator(cancellationToken);
            
            while (true)
            {
                FlightSearchResponse? flight;
                try
                {
                    if (!await enumerator.MoveNextAsync())
                    {
                        break;
                    }
                    flight = enumerator.Current;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error reading flight from {provider}");
                    break;
                }

                if (flight != null && 
                    flight.Departure == request.Origin && 
                    flight.Arrival == request.Destination)
                {
                    yield return flight;
                }
            }
        }
}