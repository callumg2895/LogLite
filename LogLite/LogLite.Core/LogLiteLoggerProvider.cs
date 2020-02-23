using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace LogLite.Core
{
	public class LogLiteLoggerProvider : ILoggerProvider
	{
		private ConcurrentDictionary<string, LogLiteLogger> loggers;
		private LogLevel logLevel;

		public LogLiteLoggerProvider(LogLevel level)
		{
			loggers = new ConcurrentDictionary<string, LogLiteLogger>();
			logLevel = level;
		}

		public ILogger CreateLogger(string categoryName)
		{
			if (loggers.TryGetValue(categoryName, out LogLiteLogger logger))
			{
				return logger;
			}

			logger = new LogLiteLogger(logLevel, categoryName);

			if (loggers.TryAdd(categoryName, logger)) 
			{
				return logger;
			}

			throw new Exception($"Could not create new Logger instance with category name {categoryName}");
		}

		public void Dispose()
		{
			foreach (LogLiteLogger logger in loggers.Values)
			{
				logger.Dispose();
			}
		}
	}
}
