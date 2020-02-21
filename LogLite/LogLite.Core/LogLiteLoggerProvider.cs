using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogLite.Core
{
	public class LogLiteLoggerProvider : ILoggerProvider
	{
		public ILogger CreateLogger(string categoryName)
		{
			return LogLiteLogger.Instance;
		}

		public void Dispose()
		{
			LogLiteLogger.Instance.Dispose();
		}
	}
}
