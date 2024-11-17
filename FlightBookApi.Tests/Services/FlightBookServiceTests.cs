using FlightBookApi.Models.Booking;
using FlightBookApi.Services;
using FlightBookApi.Exceptions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;
using System.Text.Json;
using System.Text;

namespace FlightBookApi.Tests.Services
{
    public class FlightBookServiceTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<ILogger<FlightBookService>> _mockLogger;
        private readonly FlightBookService _service;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

        public FlightBookServiceTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<FlightBookService>>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            
            var client = new HttpClient(_mockHttpMessageHandler.Object);
            _mockHttpClientFactory
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(client);

            _service = new FlightBookService(_mockHttpClientFactory.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task BookFlightAsync_WithValidHHFlight_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new BookingRequest { FlightNumber = "HH123" };
            SetupMockResponse(HttpStatusCode.OK);

            // Act
            var result = await _service.BookFlightAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("HH123", result.FlightNumber);
            Assert.Equal("Confirmed", result.Status);
        }

        [Fact]
        public async Task BookFlightAsync_WithValidAYFlight_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new BookingRequest { FlightNumber = "AY123" };
            SetupMockResponse(HttpStatusCode.OK);

            // Act
            var result = await _service.BookFlightAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("AY123", result.FlightNumber);
        }

        [Fact]
        public async Task BookFlightAsync_WithInvalidFlightNumber_ReturnsFailureResponse()
        {
            // Arrange
            var request = new BookingRequest { FlightNumber = "XX123" };

            // Act
            var result = await _service.BookFlightAsync(request, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid Flight Number", result.Status);
        }

        [Fact]
        public async Task BookFlightAsync_WhenProviderFails_ReturnsFailureResponse()
        {
            // Arrange
            var request = new BookingRequest { FlightNumber = "HH123" };
            SetupMockResponse(HttpStatusCode.BadRequest);

            // Act
            var result = await _service.BookFlightAsync(request, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed", result.Status);
        }

        private void SetupMockResponse(HttpStatusCode statusCode)
        {
            var response = new HttpResponseMessage(statusCode);
            
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
        }
    }
} 