using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using AybJetProviderApi.Controllers;
using AybJetProviderApi.Models;
using AybJetProviderApi.Models.Booking;
using AybJetProviderApi.Models.FlightSearch;
using AybJetProviderApi.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
        public void Search_ReturnsOkResultWithFlights()
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
            _mockService.Setup(s => s.SearchFlights(request)).Returns(expectedFlights);

            // Act
            var result = _controller.Search(request);

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
                .Setup(x => x.BookFlightAsync(request))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.BookFlight(request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task BookFlight_WhenFlightNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new BookingRequest { FlightNumber = "AY999" };
            _mockService
                .Setup(x => x.BookFlightAsync(request))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.BookFlight(request);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task StreamFlights_ReturnsAsyncEnumerable()
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
            var results = new List<FlightSearchResponse>();
            await foreach (var flight in _controller.StreamFlights(request, CancellationToken.None))
            {
                results.Add(flight);
            }

            // Assert
            Assert.Equal(expectedFlights.Count, results.Count);
            Assert.Equal(expectedFlights.First().FlightNumber, results.First().FlightNumber);
        }
    }
} 