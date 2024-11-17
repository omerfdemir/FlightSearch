using FlightSearchApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace FlightSearchApi.Tests.Services;

public class FlightSearchServiceTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<FlightSearchService>> _mockLogger;
    private readonly FlightSearchService _service;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly IConfiguration _configuration;
    public FlightSearchServiceTests()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json")
            .Build();

        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<FlightSearchService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var client = new HttpClient(_mockHttpMessageHandler.Object);
        _mockHttpClientFactory
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(client);
        _service = new FlightSearchService(_mockHttpClientFactory.Object, _mockLogger.Object);
    }
}