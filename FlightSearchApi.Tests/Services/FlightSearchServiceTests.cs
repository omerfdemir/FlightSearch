using FlightSearchApi.Models.FlightSearch;
using FlightSearchApi.Services;
using FlightSearchApi.Tests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace FlightSearchApi.Tests.Services
{
    public class FlightSearchServiceTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<ILogger<FlightSearchService>> _mockLogger;
        private readonly FlightSearchService _service;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

        public FlightSearchServiceTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<FlightSearchService>>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            
            var client = new HttpClient(_mockHttpMessageHandler.Object);
            _mockHttpClientFactory
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(client);

            _service = new FlightSearchService(_mockHttpClientFactory.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task SearchFlightsAsync_WhenBothProvidersRespond_ReturnsCombinedResults()
        {
            // Arrange
            var request = new FlightSearchRequest 
            { 
                Origin = "LHR",
                Destination = "JFK" 
            };

            var mockFlights = new List<FlightSearchResponse>
            {
                new() { FlightNumber = "HH123", Departure = "LHR", Arrival = "JFK" }
            };

            SetupMockResponse(HttpStatusCode.OK, mockFlights);

            // Act
            var result = await _service.SearchFlightsAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(2),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SearchFlightsAsync_WhenProviderTimeout_ReturnsPartialResults()
        {
            // Arrange
            var request = new FlightSearchRequest 
            { 
                Origin = "LHR",
                Destination = "JFK" 
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException());

            // Act
            var result = await _service.SearchFlightsAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockLogger.VerifyLog(logger => 
                logger.LogError(It.Is<string>(msg => 
                    msg.Contains("did not respond in time"))));
        }

        [Fact]
        public async Task StreamFlightsAsync_ReturnsStreamOfFlights()
        {
            // Arrange
            var request = new FlightSearchRequest 
            { 
                Origin = "LHR",
                Destination = "JFK" 
            };

            var mockFlights = new List<FlightSearchResponse>
            {
                new() { FlightNumber = "HH123", Departure = "LHR", Arrival = "JFK" }
            };

            SetupMockResponse(HttpStatusCode.OK, mockFlights);

            // Act
            var results = new List<FlightSearchResponse>();
            await foreach (var flight in _service.StreamFlightsAsync(request, CancellationToken.None))
            {
                results.Add(flight);
            }

            // Assert
            Assert.NotEmpty(results);
        }

        private void SetupMockResponse(HttpStatusCode statusCode, object content)
        {
            var response = new HttpResponseMessage(statusCode);
            response.Content = new StringContent(
                JsonSerializer.Serialize(content),
                Encoding.UTF8,
                "application/json");

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