namespace HopeAirProviderApi.Models.FlightSearch;

public class FlightSearchRequest
{
    public string Origin { get; set; }
    public string Destination { get; set; }
    public DateTime DepartureDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public int PassengerCount { get; set; }
}