using HopeAirProviderApi.Models.Booking;
using HopeAirProviderApi.Models.FlightSearch;
using HopeAirProviderApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HopeAirProviderApi.Tests.Services
{
    public class HopeAirServiceTests
    {
        private readonly Mock<ILogger<HopeAirService>> _mockLogger;
        private readonly HopeAirService _service;

        public HopeAirServiceTests()
        {
            _mockLogger = new Mock<ILogger<HopeAirService>>();
            _service = new HopeAirService(_mockLogger.Object);
        }

        [Fact]
        public async Task SearchFlightsAsync_WithValidRequest_ReturnsFilteredFlights()
        {
            // Arrange
            var request = new FlightSearchRequest 
            { 
                Origin = "LHR",
                Destination = "JFK" 
            };

            // Act
            var result = await _service.SearchFlightsAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, flight => 
            {
                Assert.Equal(request.Origin, flight.Departure);
                Assert.Equal(request.Destination, flight.Arrival);
                Assert.StartsWith("HH", flight.FlightNumber);
            });
        }

        [Fact]
        public async Task BookFlightAsync_WithExistingFlight_ReturnsTrue()
        {
            // Arrange
            var request = new BookingRequest { FlightNumber = "HH123" };
            
            // First add a flight to the cache via search
            var searchRequest = new FlightSearchRequest 
            { 
                Origin = "LHR",
                Destination = "JFK" 
            };
            await _service.SearchFlightsAsync(searchRequest);

            // Act
            var result = await _service.BookFlightAsync(request);

            // Assert
            Assert.True(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Flight HH123 booked and removed from cache")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task BookFlightAsync_WithNonExistentFlight_ReturnsFalse()
        {
            // Arrange
            var request = new BookingRequest { FlightNumber = "HH999" };

            // Act
            var result = await _service.BookFlightAsync(request);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Flight HH999 not found in cache")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ParseSoapResponse_WithInvalidXml_LogsErrorAndReturnsEmptyList()
        {
            // Arrange
            var request = new FlightSearchRequest 
            { 
                Origin = "INVALID",
                Destination = "INVALID" 
            };

            // Act
            var result = await _service.SearchFlightsAsync(request);

            // Assert
            Assert.Empty(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error parsing flight data")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
} 