using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using UltimatePDF_ExternalLogic.Utils;

namespace OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;
public class Logger {
    private readonly ILogger logger;
    private readonly ICollection<LogAttachment> attachments;
    private readonly ICollection<CustomLoggerFactory> loggerFactories;
    private readonly StringBuilder executionLog;
    private readonly bool attachFilesLogs;

    private Logger() {
        logger = NullLogger<Logger>.Instance;
        attachments = new List<LogAttachment>(6);
        loggerFactories = new List<CustomLoggerFactory>(1);
        executionLog = new StringBuilder();
    }

    protected Logger(ILogger logger, bool attachFilesLogs) : this() {
        this.logger = logger;
        this.attachFilesLogs = attachFilesLogs;
    }

    public static Logger GetLogger(ILogger _odcLogger, bool collectLogs, bool attachFilesLogs) {
        using var activity = Activity.Current?.Source.StartActivity("Logger.GetLogger");
        if (collectLogs) {
            return new Logger(_odcLogger, attachFilesLogs);
        } else {
            return new NullLogger();
        }
    }

    public virtual bool IsEnabled {
        get { return true; }
    }

    /// <summary>
    /// Whether <see cref="Attach"/>/<see cref="AttachAsync"/> actually add files to the log zip,
    /// so callers can describe a referenced file accurately instead of assuming it was attached.
    /// </summary>
    public virtual bool AttachFilesLogs => attachFilesLogs;

    public void Log(string message) {
        using var activity = Activity.Current?.Source.StartActivity("Logger.Log");
        Log(LogLevel.Information, message);
    }

    public virtual void Error(Exception? e, string? message, params object?[] args) {
        using var activity = Activity.Current?.Source.StartActivity("Logger.Error");
        AppendToExecutionLog(LogLevel.Error, message, e);
        logger.LogError(e, message, args);
    }

    public virtual void Error(string message) {
        using var activity = Activity.Current?.Source.StartActivity("Logger.Error");
        AppendToExecutionLog(LogLevel.Error, message);
        logger.LogError(message);
    }

    public virtual void Warning(string message) {
        using var activity = Activity.Current?.Source.StartActivity("Logger.Warning");
        AppendToExecutionLog(LogLevel.Warning, message);
        logger.LogWarning(message);
    }

    public virtual void Warning(string? message, params object?[] args) {
        using var activity = Activity.Current?.Source.StartActivity("Logger.Warning");
        AppendToExecutionLog(LogLevel.Warning, message);
        logger.LogWarning(message, args);
    }

    public virtual void Log(LogLevel level, string? message, params object?[] args) {
        using var activity = Activity.Current?.Source.StartActivity("Logger.Log");
        AppendToExecutionLog(level, message);
        logger.Log(level, message, args);
    }

    private void AppendToExecutionLog(LogLevel level, string? message, Exception? exception = null) {
        using var activity = Activity.Current?.Source.StartActivity("Logger.AppendToExecutionLog");
        lock (executionLog) {
            executionLog.AppendLine($"[{DateTime.UtcNow:o}] [{level}] - {message}");
            if (exception != null) {
                executionLog.AppendLine(exception.ToString());
            }
        }
    }

    public void Log(string message, bool condition) {
        using var activity = Activity.Current?.Source.StartActivity("Logger.Log");
        if (condition) {
            Log(message);
        }
    }

    public virtual ILoggerFactory GetLoggerFactory(string fileName) {
        using var activity = Activity.Current?.Source.StartActivity("Logger.GetLoggerFactory");
        var loggerFactory = new CustomLoggerFactory(fileName);
        loggerFactories.Add(loggerFactory);
        return loggerFactory;
    }

    public virtual void Attach(string filename, byte[] contents) {
        using var activity = Activity.Current?.Source.StartActivity("Logger.Attach");
        if (this.attachFilesLogs) {
            Log($"Attached {filename}");
            attachments.Add(new LogAttachment(filename, contents));
        }
    }

    public virtual void Attach(string filename, string content) {
        using var activity = Activity.Current?.Source.StartActivity("Logger.Attach");
        Attach(filename, Encoding.UTF8.GetBytes(content));
    }

    /// <summary>
    /// Attaches content that is only worth producing when attachments are actually being collected.
    /// The provider is never invoked if attaching is off, so callers can hand over an expensive read
    /// (draining a response body, re-rendering) without paying for it on the common path.
    /// </summary>
    public virtual async Task AttachAsync(string filename, Func<Task<byte[]>> contents) {
        using var activity = Activity.Current?.Source.StartActivity("Logger.AttachAsync");
        if (this.attachFilesLogs) {
            Attach(filename, await contents());
        }
    }

    public virtual byte[] GetZipFile() {
        using var activity = Activity.Current?.Source.StartActivity("Logger.GetZipFile");
        using var stream = new MemoryStream();
        
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            AddExecutionLogToZip(zip);
            AddAttachmentsToZip(zip);
            AddCustomLoggersToZip(zip);
        }

        return stream.ToArray();
    }

    private void AddExecutionLogToZip(ZipArchive zip) {
        using var activity = Activity.Current?.Source.StartActivity("Logger.AddExecutionLogToZip");
        if (executionLog.Length == 0) {
            return;
        }

        var entry = zip.CreateEntry("execution.txt");
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        writer.Write(executionLog.ToString());
    }

    private void AddAttachmentsToZip(ZipArchive zip) {
        using var activity = Activity.Current?.Source.StartActivity("Logger.AddAttachmentsToZip");
        foreach (var attachment in attachments) {
            var attachmentEntry = zip.CreateEntry(attachment.filename);
            using var stream = attachmentEntry.Open();
            stream.Write(attachment.contents, 0, attachment.contents.Length);
        }
    }

    private void AddCustomLoggersToZip(ZipArchive zip) {
        using var activity = Activity.Current?.Source.StartActivity("Logger.AddCustomLoggersToZip");
        foreach (var logger in loggerFactories) {
            var attachmentEntry = zip.CreateEntry(logger.filename);
            using var stream = attachmentEntry.Open();
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(logger.ToString());
        }
    }

    private class NullLogger : Logger {
        public override bool IsEnabled {
            get { return false; }
        }

        public override bool AttachFilesLogs => false;

        public override void Log(LogLevel level, string? message, params object?[] args) {
            using var activity = Activity.Current?.Source.StartActivity("NullLogger.Log");
        }

        public override void Warning(string message) {
            using var activity = Activity.Current?.Source.StartActivity("NullLogger.Warning");
        }

        public override void Warning(string? message, params object?[] args) {
            using var activity = Activity.Current?.Source.StartActivity("NullLogger.Warning");
        }

        public override void Attach(string filename, byte[] contents) {
            using var activity = Activity.Current?.Source.StartActivity("NullLogger.Attach");
        }

        public override Task AttachAsync(string filename, Func<Task<byte[]>> contents) {
            using var activity = Activity.Current?.Source.StartActivity("NullLogger.AttachAsync");
            return Task.CompletedTask;
        }

        public override ILoggerFactory GetLoggerFactory(string filename) {
            using var activity = Activity.Current?.Source.StartActivity("NullLogger.GetLoggerFactory");
            return new NullLoggerFactory();
        }

        public override byte[] GetZipFile() {
            using var activity = Activity.Current?.Source.StartActivity("NullLogger.GetZipFile");
            return Array.Empty<byte>();
        }
    }

    private class LogAttachment {
        public readonly string filename;
        public readonly byte[] contents;

        public LogAttachment(string filename, byte[] contents) {
            this.filename = filename;
            this.contents = contents;
        }
    }

    private class CustomLoggerFactory : ILoggerFactory {

        public readonly string filename;
        private readonly StringBuilder log = new();

        public CustomLoggerFactory(string filename) {
            this.filename = filename;
        }

        public void AddProvider(ILoggerProvider provider) {
            using var activity = Activity.Current?.Source.StartActivity("CustomLoggerFactory.AddProvider");
        }

        public ILogger CreateLogger(string categoryName) {
            using var activity = Activity.Current?.Source.StartActivity("CustomLoggerFactory.CreateLogger");
            return new CustomLogger(log, categoryName);
        }

        public override string ToString() {
            using var activity = Activity.Current?.Source.StartActivity("CustomLoggerFactory.ToString");
            lock (log) {
                return log.ToString();
            }
        }

        public void Dispose() {
        }
    }

    private class CustomLogger : ILogger {

        private readonly StringBuilder log;
        private readonly string categoryName;

        public CustomLogger(StringBuilder log, string categoryName) {
            this.log = log;
            this.categoryName = categoryName;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
            using var activity = Activity.Current?.Source.StartActivity("CustomLogger.BeginScope");
            return new CustomLogger.Scope();
        }

        public bool IsEnabled(LogLevel logLevel) {
            using var activity = Activity.Current?.Source.StartActivity("CustomLogger.IsEnabled");
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
            using var activity = Activity.Current?.Source.StartActivity("CustomLogger.Log");
            lock (log) {
                log.AppendLine($"[{DateTime.UtcNow.ToString("o")}] [{logLevel}] - {categoryName} - {formatter(state, exception)}");
            }
        }

        private class Scope : IDisposable {
            public void Dispose() {
            }
        }
    }
}
