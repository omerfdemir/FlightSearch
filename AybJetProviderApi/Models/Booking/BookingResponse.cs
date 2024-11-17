namespace AybJetProviderApi.Models.Booking;

public class BookingResponse
{
    public bool Success { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime BookingTime { get; set; }
    public PassengerDetails Passenger { get; set; } = new();
}

public class PassengerDetails
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}