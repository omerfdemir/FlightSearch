using HopeAirProviderApi.Models.Booking;
using HopeAirProviderApi.Models.FlightSearch;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Xml;

namespace HopeAirProviderApi.Services
{
    public class HopeAirService : IHopeAirService
    {
        private readonly ILogger<HopeAirService> _logger;
        private static readonly List<FlightSearchResponse> _flightCache = new();
        private List<FlightSearchResponse> _availableFlights = new();

        public HopeAirService(ILogger<HopeAirService> logger)
        {
            _logger = logger;
        }

        public async Task<List<FlightSearchResponse>> SearchFlightsAsync(FlightSearchRequest request)
        {
            var soapRequestXml = CreateSoapRequest(request);
            var soapResponseXml = await SendSoapRequestAsync(soapRequestXml);
            var flights = ParseSoapResponse(soapResponseXml);
            
            var filteredFlights = flights
                .Where(f => f.Departure == request.Origin && 
                           f.Arrival == request.Destination &&
                           f.DepartureTime.Date == request.DepartureDate.Date)
                .ToList();

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
            var flights = new List<FlightSearchResponse>();
            if (string.IsNullOrEmpty(soapResponse))
            {
                _logger.LogError("SOAP response is empty.");
                return flights;
            }

            try
            {
                var doc = XDocument.Parse(soapResponse);
                XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace sky = "http://skyblue.com/flight";

                var flightElements = doc.Descendants(sky + "flight");
                foreach (var flightElement in flightElements)
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
                            ArrivalTime = DateTime.Parse(flightElement.Element(sky + "arrivalTime")?.Value ?? DateTime.MinValue.ToString()),
                            ProviderName = "HopeAir"
                        };

                        flights.Add(flight);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing flight data.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing SOAP response.");
            }

            return flights;
        }

        public void SetAvailableFlights(List<FlightSearchResponse> flights)
        {
            _availableFlights = flights;
        }

        public async Task<bool> BookFlightAsync(BookingRequest request)
        {
            var flight = _availableFlights.FirstOrDefault(f => f.FlightNumber == request.FlightNumber);
            if (flight == null)
            {
                _logger.LogWarning($"Flight {request.FlightNumber} not found");
                return false;
            }

            _availableFlights.Remove(flight);
            _logger.LogInformation($"Flight {request.FlightNumber} booked and removed from cache");
            return true;
        }

        public async Task<List<FlightSearchResponse>> ParseSoapResponseAsync(string soapResponse)
        {
            var flights = new List<FlightSearchResponse>();
            if (string.IsNullOrEmpty(soapResponse))
            {
                _logger.LogError("SOAP response is empty.");
                return flights;
            }

            var doc = new XmlDocument();
            try
            {
                doc.LoadXml(soapResponse);
                var flightNodes = doc.SelectNodes("//flight");
                
                if (flightNodes == null)
                {
                    _logger.LogError("No flight nodes found in response");
                    return flights;
                }

                foreach (XmlNode flightNode in flightNodes)
                {
                    try
                    {
                        var flight = new FlightSearchResponse
                        {
                            FlightNumber = flightNode.SelectSingleNode("flightNumber")?.InnerText,
                            Departure = flightNode.SelectSingleNode("departure")?.InnerText,
                            Arrival = flightNode.SelectSingleNode("arrival")?.InnerText,
                            Price = decimal.Parse(flightNode.SelectSingleNode("price")?.InnerText ?? "0"),
                            Currency = flightNode.SelectSingleNode("currency")?.InnerText,
                        };

                        if (string.IsNullOrEmpty(flight.FlightNumber) || 
                            string.IsNullOrEmpty(flight.Departure) || 
                            string.IsNullOrEmpty(flight.Arrival))
                        {
                            _logger.LogError("Error parsing flight data: Missing required fields");
                            continue;
                        }

                        flights.Add(flight);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing flight data");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing flight data");
            }

            return flights;
        }
    }
}