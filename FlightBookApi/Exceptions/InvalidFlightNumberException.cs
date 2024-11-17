namespace FlightBookApi.Exceptions;

public class InvalidFlightNumberException : Exception
{
    public InvalidFlightNumberException(string message) : base(message) { }
} 