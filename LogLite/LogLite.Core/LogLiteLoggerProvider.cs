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

		public LogLiteLoggerProvider()
		{
			loggers = new ConcurrentDictionary<string, LogLiteLogger>();
		}

		public ILogger CreateLogger(string categoryName)
		{
			if (loggers.TryGetValue(categoryName, out LogLiteLogger logger))
			{
				return logger;
			}

			logger = new LogLiteLogger();

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
