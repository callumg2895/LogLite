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
                int threadHash = Thread.CurrentThread.GetHashCode();

                lock (_logger._scopeLookupLock)
                {
                    _logger._scopeLookup.Remove(threadHash);
                }
            }
        }

        private readonly List<ILoggerSink> _sinks;
        private readonly Dictionary<int, string> _scopeLookup;
        private readonly LogLevel _logLevel;
        private readonly string _category;

        private readonly object _scopeLookupLock;

        private Task currentTask = null;
        
        public LogLiteLogger(LogLevel logLevel, string category)
        {
            _sinks = new List<ILoggerSink>();
            _scopeLookup = new Dictionary<int, string>();
            _logLevel = logLevel;
            _category = category;

            _scopeLookupLock = new object();

        }   

        public void AddSink(ILoggerSink sink)
        {
            _sinks.Add(sink);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel < _logLevel)
            {
                // The log message should be suppressed if it does not meet the configured minimum level

                return;
            }

            StringBuilder statement = new StringBuilder();

            int threadHash = Thread.CurrentThread.GetHashCode();
            string dateMessage = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string scopeMessage = GetCurrentScope();
            string stateMessage = formatter(state, exception);

            statement.Append($"[{dateMessage}] ")
                     .Append($"[{_category}] ");

            if (!string.IsNullOrEmpty(scopeMessage))
            {
                statement.Append($"[{scopeMessage}] ");
            }

            statement.Append($" {stateMessage}");

            currentTask = Task.Run(() =>
            {
                List<Task> tasks = new List<Task>();

                foreach (ILoggerSink sink in _sinks)
                {
                    tasks.Add(Task.Run(() => { sink.Write(statement.ToString()); }));
                }

                Task.WaitAll(tasks.ToArray());
            });
        }

        public void Dispose()
        {
            if (currentTask != null)
            {
                currentTask.Wait();
            }

            foreach(ILoggerSink sink in _sinks)
            {
                sink.Flush();
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            int threadHash = Thread.CurrentThread.GetHashCode();
            string scopeMessage = state.ToString();

            lock (_scopeLookupLock)
            {
                _scopeLookup.TryAdd(threadHash, scopeMessage);
            }

            return new LoggerScope(this);
        }

        private string GetCurrentScope()
        {
            int threadHash = Thread.CurrentThread.GetHashCode();

            lock (_scopeLookupLock)
            {
                if (_scopeLookup.ContainsKey(threadHash))
                {
                    return _scopeLookup[threadHash];
                }
            }

            return string.Empty;
        }
    }
}
