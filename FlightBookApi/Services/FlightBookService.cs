using FlightBookApi.Exceptions;
using FlightBookApi.Models.Booking;

namespace FlightBookApi.Services;

public class FlightBookService: IFlightBookService
{
    private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FlightBookService> _logger;

        public FlightBookService(IHttpClientFactory httpClientFactory, ILogger<FlightBookService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }


        public async Task<BookingResponse> BookFlightAsync(BookingRequest request, CancellationToken cancellationToken)
        {
            var (client, uri) = GetProviderDetails(request.FlightNumber);
            
            try
            {
                var response = await client.PostAsJsonAsync($"{uri}/book", request, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    return new BookingResponse
                    {
                        Success = true,
                        BookingReference = $"BK-{Guid.NewGuid():N}",
                        FlightNumber = request.FlightNumber,
                        Status = "Confirmed",
                        BookingTime = DateTime.UtcNow,
                        Passenger = new PassengerDetails
                        {
                            Name = request.PassengerName,
                            Email = request.PassengerEmail,
                            Phone = request.PassengerPhone
                        }
                    };
                }

                _logger.LogWarning($"Booking failed with status code: {response.StatusCode}");
                return new BookingResponse { Success = false, Status = "Failed" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking flight");
                return new BookingResponse { Success = false, Status = "Error" };
            }
        }

        private (HttpClient client, string uri) GetProviderDetails(string flightNumber)
        {
            if (string.IsNullOrEmpty(flightNumber))
            {
                throw new InvalidFlightNumberException("Flight number cannot be empty");
            }

            if (flightNumber.StartsWith("HH"))
                return (_httpClientFactory.CreateClient("HopeAirClient"), "HopeAir");
            
            if (flightNumber.StartsWith("AY"))
                return (_httpClientFactory.CreateClient("AybJetClient"), "AybJet");
            
            throw new InvalidFlightNumberException($"Unsupported airline prefix in flight number: {flightNumber}");
        }
}