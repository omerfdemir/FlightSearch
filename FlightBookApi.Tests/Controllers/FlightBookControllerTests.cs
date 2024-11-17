using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FlightBookApi.Controllers;
using FlightBookApi.Services;
using FlightBookApi.Models;
using FlightBookApi.Models.Booking;
using System.Threading;

namespace FlightBookApi.Tests.Controllers
{
    public class FlightBookControllerTests
    {
        private readonly Mock<IFlightBookService> _mockService;
        private readonly FlightBookController _controller;

        public FlightBookControllerTests()
        {
            _mockService = new Mock<IFlightBookService>();
            _controller = new FlightBookController(_mockService.Object);
        }

        [Fact]
        public async Task BookFlight_WhenSuccessful_ReturnsOkResult()
        {
            // Arrange
            var request = new BookingRequest { FlightNumber = "HH123" };
            var response = new BookingResponse { Success = true };
            _mockService
                .Setup(x => x.BookFlightAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.BookFlight(request, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<BookingResponse>(okResult.Value);
            Assert.True(returnValue.Success);
        }

        [Fact]
        public async Task BookFlight_WhenFailed_ReturnsBadRequest()
        {
            // Arrange
            var request = new BookingRequest { FlightNumber = "XX123" };
            var response = new BookingResponse { Success = false };
            _mockService
                .Setup(x => x.BookFlightAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.BookFlight(request, CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }
} 