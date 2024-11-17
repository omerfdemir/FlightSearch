using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using AybJetProviderApi.Controllers;
using AybJetProviderApi.Models;
using AybJetProviderApi.Models.Booking;
using AybJetProviderApi.Models.FlightSearch;
using AybJetProviderApi.Services;

namespace AybJetProviderApi.Tests.Controllers
{
    public class AybJetControllerTests
    {
        private readonly Mock<IAybJetService> _mockService;
        private readonly AybJetController _controller;

        public AybJetControllerTests()
        {
            _mockService = new Mock<IAybJetService>();
            _controller = new AybJetController(_mockService.Object);
        }

        [Fact]
        public async Task SearchFlights_ReturnsOkResultWithFlights()
        {
            // Arrange
            var request = new FlightSearchRequest 
            { 
                Origin = "LHR", 
                Destination = "JFK" 
            };
            var expectedFlights = new List<FlightSearchResponse>
            {
                new() { FlightNumber = "AY123", Departure = "LHR", Arrival = "JFK" }
            };
            _mockService
                .Setup(s => s.SearchFlightsAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedFlights);

            // Act
            var result = await _controller.Search(request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<FlightSearchResponse>>(okResult.Value);
            Assert.Equal(expectedFlights, returnValue);
        }

        [Fact]
        public async Task BookFlight_WhenSuccessful_ReturnsOkResult()
        {
            // Arrange
            var request = new BookingRequest { FlightNumber = "AY123" };
            _mockService
                .Setup(x => x.BookFlightAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.BookFlight(request, CancellationToken.None);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task BookFlight_WhenFlightNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new BookingRequest { FlightNumber = "AY999" };
            _mockService
                .Setup(x => x.BookFlightAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.BookFlight(request, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task StreamFlights_ReturnsStreamOfFlights()
        {
            // Arrange
            var request = new FlightSearchRequest 
            { 
                Origin = "LHR", 
                Destination = "JFK" 
            };
            var expectedFlights = new List<FlightSearchResponse>
            {
                new() { FlightNumber = "AY123", Departure = "LHR", Arrival = "JFK" }
            };

            _mockService
                .Setup(s => s.StreamFlightsAsync(request, It.IsAny<CancellationToken>()))
                .Returns(expectedFlights.ToAsyncEnumerable());

            // Act
            var result = _controller.StreamFlights(request, CancellationToken.None);

            // Assert
            var flights = await result.ToListAsync();
            Assert.Equal(expectedFlights, flights);
        }
    }
} 