namespace AybJetProviderApi.Models.Booking;

public class BookingRequest
{
    public string FlightNumber { get; set; } = string.Empty;
    public string PassengerName { get; set; } = string.Empty;
    public string PassengerEmail { get; set; } = string.Empty;
    public string PassengerPhone { get; set; } = string.Empty;
}