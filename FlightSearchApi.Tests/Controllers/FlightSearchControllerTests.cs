using Microsoft.AspNetCore.Mvc;
using Moq;
using FlightSearchApi.Controllers;
using FlightSearchApi.Services;
using FlightSearchApi.Models.FlightSearch;


namespace FlightSearchApi.Tests.Controllers
{
    public class FlightSearchControllerTests
    {
        private readonly Mock<IFlightSearchService> _mockService;
        private readonly FlightSearchController _controller;

        public FlightSearchControllerTests()
        {
            _mockService = new Mock<IFlightSearchService>();
            _controller = new FlightSearchController(_mockService.Object);
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
                new() { FlightNumber = "HH123", Departure = "LHR", Arrival = "JFK" },
                new() { FlightNumber = "AY456", Departure = "LHR", Arrival = "JFK" }
            };
            _mockService
                .Setup(s => s.SearchFlightsAsync(request))
                .ReturnsAsync(expectedFlights);

            // Act
            var result = await _controller.SearchFlights(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<FlightSearchResponse>>(okResult.Value);
            Assert.Equal(expectedFlights, returnValue);
        }

        [Fact]
        public async Task Search_WhenNoFlightsFound_ReturnsEmptyList()
        {
            // Arrange
            var request = new FlightSearchRequest 
            { 
                Origin = "LHR",
                Destination = "JFK" 
            };
            _mockService
                .Setup(s => s.SearchFlightsAsync(request))
                .ReturnsAsync(new List<FlightSearchResponse>());

            // Act
            var result = await _controller.SearchFlights(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<FlightSearchResponse>>(okResult.Value);
            Assert.Empty(returnValue);
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
                new() { FlightNumber = "HH123", Departure = "LHR", Arrival = "JFK" }
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