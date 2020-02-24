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

            int threadHash = Thread.CurrentThread.GetHashCode();
            string scopeMessage = string.Empty;

            lock (_scopeLookupLock)
            {
                if (_scopeLookup.ContainsKey(threadHash))
                {
                    scopeMessage = _scopeLookup[threadHash];
                }
            }

            string statement = $"[{_category}] [{scopeMessage}] {formatter(state, exception)}";

            currentTask = Task.Run(() =>
            {
                List<Task> tasks = new List<Task>();

                foreach (ILoggerSink sink in _sinks)
                {
                    tasks.Add(Task.Run(() => { sink.Write(statement); }));
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
    }
}
