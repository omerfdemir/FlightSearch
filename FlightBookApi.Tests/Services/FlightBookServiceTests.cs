using FlightBookApi.Services;
using FlightBookApi.Models;
using FlightBookApi.Models.Booking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using System.Net.Http.Headers;

namespace FlightBookApi.Tests.Services
{
    public class FlightBookServiceTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<ILogger<FlightBookService>> _mockLogger;
        private readonly FlightBookService _service;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly IConfiguration _configuration;

        public FlightBookServiceTests()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json")
                .Build();

            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<FlightBookService>>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var hopeAirBaseUrl = _configuration["ProviderUrls:HopeAir"];
            var aybJetBaseUrl = _configuration["ProviderUrls:AybJet"];

            var hopeAirClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri(hopeAirBaseUrl ?? "http://localhost:5001")
            };

            var aybJetClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri(aybJetBaseUrl ?? "http://localhost:5002")
            };

            _mockHttpClientFactory
                .Setup(x => x.CreateClient("HopeAirClient"))
                .Returns(hopeAirClient);

            _mockHttpClientFactory
                .Setup(x => x.CreateClient("AybJetClient"))
                .Returns(aybJetClient);

            SetupMockHttpResponse(HttpStatusCode.OK);

            _service = new FlightBookService(_mockHttpClientFactory.Object, _mockLogger.Object);
        }

        private void SetupMockHttpResponse(HttpStatusCode statusCode)
        {
            var response = new HttpResponseMessage(statusCode);
            var bookingResponse = new BookingResponse 
            { 
                Success = statusCode == HttpStatusCode.OK,
                Status = statusCode == HttpStatusCode.OK ? "Confirmed" : "Error",
                FlightNumber = statusCode == HttpStatusCode.OK ? "HH123" : null
            };
            
            response.Content = new StringContent(
                JsonSerializer.Serialize(bookingResponse));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
        }

        [Fact]
        public async Task BookFlightAsync_WithValidHHFlight_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new BookingRequest { FlightNumber = "HH123" };

            // Act
            var result = await _service.BookFlightAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Confirmed", result.Status);
            Assert.Equal("HH123", result.FlightNumber);
        }

        // Add more tests...
    }
} 