using System.Text.Json.Serialization;

public class FlightSearchResponse
{
    [JsonPropertyName("flightNumber")]
    public string FlightNumber { get; set; }

    [JsonPropertyName("departure")]
    public string Departure { get; set; }

    [JsonPropertyName("arrival")]
    public string Arrival { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("duration")]
    public string Duration { get; set; }

    [JsonPropertyName("departureTime")]
    public DateTime DepartureTime { get; set; }

    [JsonPropertyName("arrivalTime")]
    public DateTime ArrivalTime { get; set; }

    public string ProviderName { get; set; }
}