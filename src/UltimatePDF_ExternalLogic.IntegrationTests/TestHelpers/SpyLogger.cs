using System;
using Microsoft.Extensions.Logging;

namespace OutSystems.UltimatePDF_ExternalLogic.IntegrationTests.TestHelpers {
    internal sealed class SpyLogger : ILogger {
        public int ErrorCount { get; private set; }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter) {
            if (logLevel >= LogLevel.Error) ErrorCount++;
        }
    }
}
