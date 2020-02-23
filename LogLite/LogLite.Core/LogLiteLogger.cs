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
            private LogLiteLogger logger;

            internal LoggerScope(LogLiteLogger logger)
            {
                this.logger = logger;
            }

            public void Dispose()
            {
                int threadHash = Thread.CurrentThread.GetHashCode();

                lock (logger.scopeLookupLock)
                {
                    logger.scopeLookup.Remove(threadHash);
                }
            }
        }

        private readonly string rootDirectory;
        private readonly string logFileDirectory;

        private readonly List<string> logQueue;
        private readonly Dictionary<int, string> scopeLookup;
        private readonly LogLevel logLevel;
        private readonly string category;

        private readonly object logQueueLock;
        private readonly object scopeLookupLock;
        private readonly object writeLock;

        private Task currentTask = null;
        
        public LogLiteLogger(LogLevel level, string category)
        {
            this.rootDirectory = Path.GetPathRoot(Environment.SystemDirectory);
            this.logFileDirectory = Path.Combine(rootDirectory, "/Logs");

            this.logQueue = new List<string>();
            this.scopeLookup = new Dictionary<int, string>();
            this.logLevel = level;
            this.category = category;

            this.logQueueLock = new object();
            this.scopeLookupLock = new object();
            this.writeLock = new object();
        }   

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel < this.logLevel)
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

            lock (scopeLookupLock)
            {
                if (scopeLookup.ContainsKey(threadHash))
                {
                    scopeMessage = scopeLookup[threadHash];
                }
            }

            lock (logQueueLock)
            {
                logQueue.Add($"[{category}] [{scopeMessage}] {statement}");
            }

            // Flush to disk asynchronously so we don't disrupt the main thread
            currentTask = Task.Run(Flush);
        }

        public void Flush()
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<string> statements;
            string logFilePath = Path.Combine(logFileDirectory, "logFile.log");

            lock (logQueueLock)
            {
                statements = new List<string>(logQueue);

                logQueue.Clear();
            }


            foreach (string item in statements)
            {
                stringBuilder.AppendLine(item);

            }

            lock (writeLock)
            {
                if (!Directory.Exists(logFileDirectory))
                {
                    Directory.CreateDirectory(logFileDirectory);
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

            lock (scopeLookupLock)
            {
                scopeLookup.TryAdd(threadHash, scopeMessage);
            }

            return new LoggerScope(this);
        }
    }
}
