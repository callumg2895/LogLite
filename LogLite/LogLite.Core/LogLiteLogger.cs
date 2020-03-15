using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogLite.Core
{
	public sealed class LogLiteLogger : IDisposable, ILogger
    {
        private class LoggerScope : IDisposable
        {
            private readonly LogLiteLogger _logger;

            internal LoggerScope(LogLiteLogger logger)
            {
                _logger = logger;
            }

            public void Dispose()
            {
                _logger.EndScope();
            }
        }

        private readonly List<ILoggerSink> _sinks;
        private readonly Dictionary<int, string> _scopeLookup;
        private readonly LogLevel _logLevel;
        private readonly string _category;
        private readonly string _dateTimeFormat;

        private readonly object _scopeLookupLock;
        
        public LogLiteLogger(LogLevel logLevel, string category)
        {
            _sinks = new List<ILoggerSink>();
            _scopeLookup = new Dictionary<int, string>();
            _logLevel = logLevel;
            _category = category;
            _dateTimeFormat = LogLiteConfiguration.DateTimeFormat;

            _scopeLookupLock = new object();
        }   

        public void AddSink(ILoggerSink sink)
        {
            _sinks.Add(sink);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            StringBuilder statement = new StringBuilder();

            string dateMessage = DateTime.Now.ToString(_dateTimeFormat);
            string scopeMessage = GetCurrentScope();
            string stateMessage = formatter(state, exception);

            statement.Append($"[{dateMessage}] ")
                     .Append($"[{_category}] ");

            if (!string.IsNullOrEmpty(scopeMessage))
            {
                statement.Append($"[{scopeMessage}] ");
            }

            statement.Append($" {stateMessage}");

            foreach (ILoggerSink sink in _sinks)
            {
                sink.Write(statement.ToString());
            }
        }

        public void Dispose()
        {
            foreach(ILoggerSink sink in _sinks)
            {
                sink.Flush();
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel > _logLevel;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new NullReferenceException($"{nameof(state)} cannot be null");
            }

            int threadHash = Thread.CurrentThread.GetHashCode();
            string? scopeMessage = state.ToString();

            lock (_scopeLookupLock)
            {
                _scopeLookup.TryAdd(threadHash, scopeMessage!);
            }

            return new LoggerScope(this);
        }

        private void EndScope()
        {
            int threadHash = Thread.CurrentThread.GetHashCode();

            lock (_scopeLookupLock)
            {
                _scopeLookup.Remove(threadHash);
            }
        }

        private string GetCurrentScope()
        {
            int threadHash = Thread.CurrentThread.GetHashCode();
            string? scopeMessage;

            lock (_scopeLookupLock)
            {
                _scopeLookup.TryGetValue(threadHash, out scopeMessage);
            }

            return scopeMessage ?? string.Empty;
        }
    }
}
