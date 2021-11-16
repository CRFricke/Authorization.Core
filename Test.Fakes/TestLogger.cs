using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Fricke.Test.Fakes
{
    /// <summary>
    /// Defines the information contained in a log entry.
    /// </summary>
    public struct LogEntry
    {
        /// <summary>
        /// Creates a new <see cref="LogEntry"/> using the specified parameters.
        /// </summary>
        /// <param name="logLevel">The severity of the log entry.</param>
        /// <param name="message">A message to be included in the log entry.</param>
        /// <param name="exception">An <see cref="Exception"/> object to be included in the log entry.</param>
        public LogEntry(LogLevel logLevel, string message, Exception? exception)
        {
            LogLevel = logLevel;
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// The severity level of the log entry.
        /// </summary>
        public LogLevel LogLevel;

        /// <summary>
        /// The message contained in the log entry.
        /// </summary>
        public string Message;

        /// <summary>
        /// The <see cref="Exception"/> contained in the log entry. <em>null</em>, if no cref="Exception"/> was logged.
        /// </summary>
        public Exception? Exception;


        /// <summary>
        /// Returns a string representing the current <see cref="LogEntry"/> object.
        /// </summary>
        /// <returns>A string representing the current <see cref="LogEntry"/> object.</returns>
        public override string ToString()
        {
            return $"{LogLevel}: {Message}";
        }
    }

    /// <summary>
    /// Handles logging for a test.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TestLogger<T> : ILogger<T> where T : class
    {
        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The entries logged by the test run.
        /// </summary>
        public List<LogEntry> LogEntries = new List<LogEntry>();

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
        {
            var messsage = state?.ToString() ?? throw new ArgumentNullException(nameof(state));
            LogEntries.Add(new LogEntry(logLevel, messsage, exception));
        }
    }
}
