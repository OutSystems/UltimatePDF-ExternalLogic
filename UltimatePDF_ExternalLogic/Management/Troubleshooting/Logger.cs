﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using UltimatePDF_ExternalLogic.Utils;

namespace OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting {
    public class Logger {
        private static readonly string version;

        private readonly StringBuilder log;
        private readonly ICollection<LogAttachment> attachments;
        private readonly ICollection<CustomLoggerFactory> loggerFactories;
        private readonly bool attachFilesLogs;

        static Logger() {
            var versionNumber = ResourceAccessor.GetResource("UltimatePDF_ExternalLogic.resources.version");
            version = $"UltimatePDF_ExternalLogic v{versionNumber}";
        }

        private Logger() {
            log = new StringBuilder();
            attachments = new List<LogAttachment>(6);
            loggerFactories = new List<CustomLoggerFactory>(1);
        }
        private Logger(bool attachFilesLogs) : this() {
            this.attachFilesLogs = attachFilesLogs;
            Log(version);
        }

        public static Logger GetLogger(bool collectLogs, bool attachFilesLogs) {
            if (collectLogs) {
                return new Logger(attachFilesLogs);
            } else {
                return new NullLogger();
            }
        }

        public virtual bool IsEnabled {
            get { return true; }
        }

        public void Log(string message) {
            Log("info", message);
        }

        public void Error(Exception e) {
            Error(e.Message);
            Error(e.StackTrace ?? "");
        }

        public void Error(string message) {
            Log("error", message);
        }

        public virtual void Log(string level, string message) {
            lock (log) {
                log.AppendLine($"[{DateTime.UtcNow.ToString("o")}] [{level}] {message}");
            }
        }

        public void Log(string message, bool condition) {
            if (condition) {
                Log(message);
            }
        }

        public virtual ILoggerFactory GetLoggerFactory(string fileName) {
            var loggerFactory = new CustomLoggerFactory(fileName);
            loggerFactories.Add(loggerFactory);
            return loggerFactory;
        }

        public virtual void Attach(string filename, byte[] contents) {
            if (this.attachFilesLogs) {
                Log($"Attached {filename}");
                attachments.Add(new LogAttachment(filename, contents));
            }
        }

        public virtual void Attach(string filename, string content) {
            Attach(filename, Encoding.UTF8.GetBytes(content));
        }

        public virtual byte[] GetZipFile() {
            using var stream = new MemoryStream();
            using var zip = new ZipArchive(stream, ZipArchiveMode.Create);
            AddLogToZip(zip, "ultimate-pdf.txt");
            AddAttachmentsToZip(zip);
            AddCustomLoggersToZip(zip);

            return stream.ToArray();
        }

        private void AddLogToZip(ZipArchive zip, string file) {
            var entry = zip.CreateEntry(file);
            using var stream = entry.Open();
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            lock (log) {
                writer.Write(log.ToString());
            }
        }

        private void AddAttachmentsToZip(ZipArchive zip) {
            foreach (var attachment in attachments) {
                var attachmentEntry = zip.CreateEntry(attachment.filename);
                using var stream = attachmentEntry.Open();
                stream.Write(attachment.contents, 0, attachment.contents.Length);
            }
        }

        private void AddCustomLoggersToZip(ZipArchive zip) {
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

            public override void Log(string level, string message) {
            }

            public override void Attach(string filename, byte[] contents) {
            }

            public override ILoggerFactory GetLoggerFactory(string filename) {
                return new NullLoggerFactory();
            }

            public override byte[] GetZipFile() {
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
            }

            public ILogger CreateLogger(string categoryName) {
                return new CustomLogger(log, categoryName);
            }

            public override string ToString() {
                lock (log) {
                    return log.ToString();
                }
            }

            public void Dispose() {
            }

            public void Clear() {
                lock(log) {
                    log.Clear();
                }
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
                return new CustomLogger.Scope();
            }

            public bool IsEnabled(LogLevel logLevel) {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
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
}
