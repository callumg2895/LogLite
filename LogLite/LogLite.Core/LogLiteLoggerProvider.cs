using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace LogLite.Core
{
	public class LogLiteLoggerProvider : ILoggerProvider
	{
		private readonly ConcurrentDictionary<string, LogLiteLogger> _loggers;
		private readonly LogLevel _logLevel;

		public LogLiteLoggerProvider(LogLevel logLevel)
		{
			_loggers = new ConcurrentDictionary<string, LogLiteLogger>();
			_logLevel = logLevel;
		}

		public ILogger CreateLogger(string categoryName)
		{
			if (_loggers.TryGetValue(categoryName, out LogLiteLogger logger))
			{
				return logger;
			}

			logger = new LogLiteLogger(_logLevel, categoryName);

			if (_loggers.TryAdd(categoryName, logger)) 
			{
				return logger;
			}

			throw new Exception($"Could not create new Logger instance with category name {categoryName}");
		}

		public void Dispose()
		{
			foreach (LogLiteLogger logger in _loggers.Values)
			{
				logger.Dispose();
			}
		}
	}
}
