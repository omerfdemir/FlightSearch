using AybJetProviderApi.Models.Booking;
using AybJetProviderApi.Models.FlightSearch;
using AybJetProviderApi.Services;
using AybJetProviderApi.Services.Cache;
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
        private readonly Mock<ICache> _mockCache;
        private readonly AybJetService _service;

        public AybJetServiceTests()
        {
            _mockLogger = new Mock<ILogger<AybJetService>>();
            _mockCache = new Mock<ICache>();
            _service = new AybJetService(_mockLogger.Object, _mockCache.Object);
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
            var result = await _service.SearchFlightsAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, flight => 
            {
                Assert.Equal(request.Origin, flight.Departure);
                Assert.Equal(request.Destination, flight.Arrival);
                Assert.StartsWith("AY", flight.FlightNumber);
            });

            _mockCache.Verify(x => x.Set(
                "AybJet_Flights",
                It.Is<List<FlightSearchResponse>>(list => 
                    list.Any(f => f.FlightNumber == "AY123"))), 
                Times.Once);
        }

        [Fact]
        public async Task BookFlightAsync_WithExistingFlight_ReturnsTrue()
        {
            // Arrange
            var request = new BookingRequest { FlightNumber = "AY123" };
            var mockFlights = new List<FlightSearchResponse>
            {
                new FlightSearchResponse 
                { 
                    FlightNumber = "AY123",
                    Departure = "LHR",
                    Arrival = "JFK",
                    Price = 500.00m,
                    Currency = "USD",
                    Duration = "8h"
                }
            };

            _mockCache.Setup(x => x.Get<List<FlightSearchResponse>>("AybJet_Flights"))
                .Returns(mockFlights);

            // Act
            var result = await _service.BookFlightAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Flight AY123 booked and removed from cache")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once());
            
            _mockCache.Verify(x => x.Set("AybJet_Flights", It.IsAny<List<FlightSearchResponse>>()), Times.Once());
        }

        [Fact]
        public async Task BookFlightAsync_WithNonExistentFlight_ReturnsFalse()
        {
            // Arrange
            var request = new BookingRequest { FlightNumber = "AY999" };
            _mockCache.Setup(x => x.Get<List<FlightSearchResponse>>("AybJet_Flights"))
                .Returns((List<FlightSearchResponse>)null!);

            // Act
            var result = await _service.BookFlightAsync(request, CancellationToken.None);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Flight AY999 not found in cache")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once());
        }
    }
} 