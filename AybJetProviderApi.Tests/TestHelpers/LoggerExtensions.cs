using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace AybJetProviderApi.Tests.TestHelpers
{
    public static class LoggerExtensions
    {
        public static void VerifyLog<T>(
            this Mock<ILogger<T>> logger,
            Action<ILogger<T>> verify,
            Times? times = null)
        {
            logger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                times ?? Times.Once());
        }
    }
} 