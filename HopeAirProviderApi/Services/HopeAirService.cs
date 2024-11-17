using HopeAirProviderApi.Models.Booking;
using HopeAirProviderApi.Models.FlightSearch;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace HopeAirProviderApi.Services
{
    public class HopeAirService : IHopeAirService
    {
        private readonly ILogger<HopeAirService> _logger;
        private static readonly List<FlightSearchResponse> _flightCache = new();

        public HopeAirService(ILogger<HopeAirService> logger)
        {
            _logger = logger;
        }

        public async Task<List<FlightSearchResponse>> SearchFlightsAsync(FlightSearchRequest request)
        {
            var soapRequestXml = CreateSoapRequest(request);
            var soapResponseXml = await SendSoapRequestAsync(soapRequestXml);
            var flights = ParseSoapResponse(soapResponseXml);
            
            var filteredFlights = flights.FindAll(f => f.Departure == request.Origin && f.Arrival == request.Destination);

            return filteredFlights;
        }

        public async IAsyncEnumerable<FlightSearchResponse> StreamFlightsAsync(
            FlightSearchRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var soapRequestXml = CreateSoapRequest(request);
            var soapResponseXml = await SendSoapRequestAsync(soapRequestXml);
            var flights = ParseSoapResponse(soapResponseXml);

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

        private string CreateSoapRequest(FlightSearchRequest request)
        {
            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace sky = "http://skyblue.com/flight";

            var soapEnvelope = new XDocument(
                new XElement(soapenv + "Envelope",
                    new XElement(soapenv + "Header"),
                    new XElement(soapenv + "Body",
                        new XElement(sky + "GetFlightInfoRequest",
                            new XElement(sky + "departure", request.Origin),
                            new XElement(sky + "arrival", request.Destination),
                            new XElement(sky + "date", request.DepartureDate.ToString("yyyy-MM-dd")),
                            new XElement(sky + "passengerCount", request.PassengerCount)
                        )
                    )
                )
            );

            return soapEnvelope.ToString();
        }

        private async Task<string> SendSoapRequestAsync(string soapRequest)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "MockData", "HopeAir-Provider-Response.xml");

            try
            {
                using var reader = new StreamReader(filePath);
                var content = await reader.ReadToEndAsync();
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading mock data from HopeAir-Provider-Response.xml");
                return string.Empty;
            }
        }

        private List<FlightSearchResponse> ParseSoapResponse(string soapResponse)
        {
            if (string.IsNullOrEmpty(soapResponse))
            {
                _logger.LogError("SOAP response is empty.");
                return new List<FlightSearchResponse>();
            }

            XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace sky = "http://skyblue.com/flight";

            var xdoc = XDocument.Parse(soapResponse);

            var flights = new List<FlightSearchResponse>();

            foreach (var flightElement in xdoc.Descendants(sky + "flight"))
            {
                try
                {
                    var flight = new FlightSearchResponse
                    {
                        FlightNumber = flightElement.Element(sky + "flightNumber")?.Value,
                        Departure = flightElement.Element(sky + "departure")?.Value,
                        Arrival = flightElement.Element(sky + "arrival")?.Value,
                        Price = decimal.Parse(flightElement.Element(sky + "price")?.Value ?? "0"),
                        Currency = flightElement.Element(sky + "currency")?.Value,
                        Duration = flightElement.Element(sky + "duration")?.Value,
                        DepartureTime = DateTime.Parse(flightElement.Element(sky + "departureTime")?.Value ?? DateTime.MinValue.ToString()),
                        ArrivalTime = DateTime.Parse(flightElement.Element(sky + "arrivalTime")?.Value ?? DateTime.MinValue.ToString())
                    };

                    flights.Add(flight);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing flight data.");
                }
            }

            return flights;
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
    }
}