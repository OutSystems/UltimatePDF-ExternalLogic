using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;

namespace OutSystems.UltimatePDF_ExternalLogic.UnitTests.TestHelpers {

    /// <summary>
    /// Test double for Logger that counts Warning / Error invocations and captures the last
    /// message text and Exception passed in.
    /// </summary>
    internal sealed class LoggerSpy : Logger {

        public int WarningCalls { get; private set; }
        public int ErrorCalls { get; private set; }
        public Exception? LastException { get; private set; }
        public string? LastWarningMessage { get; private set; }

        public LoggerSpy() : base(NullLogger<Logger>.Instance, attachFilesLogs: false) {
        }

        public override void Warning(string message) {
            WarningCalls++;
            LastWarningMessage = message;
        }

        public override void Warning(Exception? e, string? message, params object?[] args) {
            WarningCalls++;
            LastException = e;
            LastWarningMessage = message;
        }
    }
}
