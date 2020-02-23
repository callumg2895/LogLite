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

        private readonly string _rootDirectory;
        private readonly string _logFileDirectory;

        private readonly List<string> _logQueue;
        private readonly Dictionary<int, string> _scopeLookup;
        private readonly LogLevel _logLevel;
        private readonly string _category;

        private readonly object _logQueueLock;
        private readonly object _scopeLookupLock;
        private readonly object _writeLock;

        private Task currentTask = null;
        
        public LogLiteLogger(LogLevel logLevel, string category)
        {
            _rootDirectory = Path.GetPathRoot(Environment.SystemDirectory);
            _logFileDirectory = Path.Combine(_rootDirectory, "/Logs");

            _logQueue = new List<string>();
            _scopeLookup = new Dictionary<int, string>();
            _logLevel = logLevel;
            _category = category;

            _logQueueLock = new object();
            _scopeLookupLock = new object();
            _writeLock = new object();
        }   

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel < _logLevel)
            {
                // The log message should be suppressed if it does not meet the configured minimum level

                return;
            }

            string statment = formatter(state, exception);

            Log(statment);
        }

        public void Log(string statement)
        {
            int threadHash = Thread.CurrentThread.GetHashCode();
            string scopeMessage = string.Empty;

            lock (_scopeLookupLock)
            {
                if (_scopeLookup.ContainsKey(threadHash))
                {
                    scopeMessage = _scopeLookup[threadHash];
                }
            }

            lock (_logQueueLock)
            {
                _logQueue.Add($"[{_category}] [{scopeMessage}] {statement}");
            }

            // Flush to disk asynchronously so we don't disrupt the main thread
            currentTask = Task.Run(Flush);
        }

        public void Flush()
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<string> statements;
            string logFilePath = Path.Combine(_logFileDirectory, "logFile.log");

            lock (_logQueueLock)
            {
                statements = new List<string>(_logQueue);

                _logQueue.Clear();
            }


            foreach (string item in statements)
            {
                stringBuilder.AppendLine(item);

            }

            lock (_writeLock)
            {
                if (!Directory.Exists(_logFileDirectory))
                {
                    Directory.CreateDirectory(_logFileDirectory);
                }

                if (!File.Exists(logFilePath))
                {
                    File.Create(logFilePath);
                }

                using FileStream stream = File.Open(logFilePath, FileMode.Open);

                stream.Write(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
            }
        }

        public void Dispose()
        {
            if (currentTask != null)
            {
                currentTask.Wait();
            }

            Flush();
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
