using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;

namespace OutSystems.UltimatePDF_ExternalLogic.Test.Helpers;
internal sealed class SpyLogger : Logger, ILogger {
    public int WarningCalls { get; private set; }
    public int ErrorCalls { get; private set; }
    public Exception? LastException { get; private set; }
    public string? LastWarningMessage { get; private set; }
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    bool ILogger.IsEnabled(LogLevel logLevel) {
        return true;
    }

    public SpyLogger() : base(NullLogger<Logger>.Instance, attachFilesLogs: false) {
    }

    public override void Warning(string message) {
        WarningCalls++;
        LastWarningMessage = message;
    }

    public override void Warning(string? message, params object?[] args) {
        WarningCalls++;
        LastWarningMessage = message;
    }

    public override void Error(string message) {
        ErrorCalls++;
    }

    public override void Error(Exception? e, string? message, params object?[] args) {
        ErrorCalls++;
        LastException = e;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter) {
        if (logLevel >= LogLevel.Error) {
            ErrorCalls++;
        }
        if (logLevel >= LogLevel.Warning) {
            WarningCalls++;
        }
    }
}

