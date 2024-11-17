using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace FlightSearchApi.Tests.TestHelpers
{
    public static class LoggerExtensions
    {
        public static void VerifyLog<T>(
            this Mock<ILogger<T>> logger,
            LogLevel level,
            string expectedMessage,
            Times? times = null)
        {
            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == level),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                times ?? Times.Once());
        }
    }
} 