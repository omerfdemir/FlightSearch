using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using HopeAirProviderApi.Controllers;
using HopeAirProviderApi.Services;
using HopeAirProviderApi.Models.Booking;
using HopeAirProviderApi.Models.FlightSearch;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HopeAirProviderApi.Tests.Controllers
{
    public class HopeAirControllerTests
    {
        private readonly Mock<IHopeAirService> _mockService;
        private readonly HopeAirController _controller;

        public HopeAirControllerTests()
        {
            _mockService = new Mock<IHopeAirService>();
            _controller = new HopeAirController(_mockService.Object);
        }

        [Fact]
        public async Task Search_ReturnsOkResultWithFlights()
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
                .Setup(s => s.SearchFlightsAsync(request))
                .ReturnsAsync(expectedFlights);

            // Act
            var result = await _controller.Search(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<FlightSearchResponse>>(okResult.Value);
            Assert.Equal(expectedFlights, returnValue);
        }

        [Fact]
        public async Task BookFlight_WhenSuccessful_ReturnsOkResult()
        {
            // Arrange
            var request = new BookingRequest { FlightNumber = "HH123" };
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
            var request = new BookingRequest { FlightNumber = "HH999" };
            _mockService
                .Setup(x => x.BookFlightAsync(request))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.BookFlight(request);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
} 