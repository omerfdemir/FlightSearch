using AybJetProviderApi.Models.Booking;
using AybJetProviderApi.Models.FlightSearch;
using AybJetProviderApi.Services;
using AybJetProviderApi.Tests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AybJetProviderApi.Tests.Services
{
    public class AybJetServiceTests
    {
        private readonly Mock<ILogger<AybJetService>> _mockLogger;
        private readonly AybJetService _service;

        public AybJetServiceTests()
        {
            _mockLogger = new Mock<ILogger<AybJetService>>();
            _service = new AybJetService(_mockLogger.Object);
        }

        [Fact]
        public void SearchFlights_WithValidRequest_ReturnsFilteredFlights()
        {
            // Arrange
            var request = new FlightSearchRequest 
            { 
                Origin = "LHR",
                Destination = "JFK" 
            };

            // Act
            var result = _service.SearchFlights(request);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, flight => 
            {
                Assert.Equal(request.Origin, flight.Departure);
                Assert.Equal(request.Destination, flight.Arrival);
                Assert.StartsWith("AY", flight.FlightNumber);
            });
        }

        [Fact]
        public async Task BookFlightAsync_WithExistingFlight_ReturnsTrue()
        {
            // Arrange
            var request = new BookingRequest { FlightNumber = "AY123" };
            
            // First add a flight to the cache via search
            var searchRequest = new FlightSearchRequest 
            { 
                Origin = "LHR",
                Destination = "JFK" 
            };
            _service.SearchFlights(searchRequest);

            // Act
            var result = await _service.BookFlightAsync(request);

            // Assert
            Assert.True(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Flight AY123 booked and removed from cache")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task BookFlightAsync_WithNonExistentFlight_ReturnsFalse()
        {
            // Arrange
            var request = new BookingRequest { FlightNumber = "AY999" };

            // Act
            var result = await _service.BookFlightAsync(request);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Flight AY999 not found in cache")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task StreamFlightsAsync_ReturnsCorrectFlights()
        {
            // Arrange
            var request = new FlightSearchRequest
            {
                Origin = "LHR",
                Destination = "JFK"
            };

            // Act
            var results = new List<FlightSearchResponse>();
            await foreach (var flight in _service.StreamFlightsAsync(request, CancellationToken.None))
            {
                results.Add(flight);
            }

            // Assert
            Assert.NotEmpty(results);
            Assert.All(results, flight =>
            {
                Assert.Equal(request.Origin, flight.Departure);
                Assert.Equal(request.Destination, flight.Arrival);
                Assert.StartsWith("AY", flight.FlightNumber);
            });
        }
    }
} 