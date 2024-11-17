namespace FlightSearchApi.Models.FlightSearch;

public class FlightSearchResponse
{
    public string FlightNumber { get; set; }
    public string Departure { get; set; }
    public string Arrival { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; }
    public string Duration { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
}