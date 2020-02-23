using LogLite.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using System.Threading;

namespace LogLite.Scratchpad
{
	class Program
	{
		static void Main(string[] args)
		{
			LogLevel level = (LogLevel)Enum.Parse(typeof(LogLevel), ConfigurationManager.AppSettings.Get("LogLevel"));

			ILoggerProvider logLiteLoggerProvider = new LogLiteLoggerProvider(level);

			using (ILoggerFactory factory = new LoggerFactory())
			{
				factory.AddProvider(logLiteLoggerProvider);

				ILogger logger = factory.CreateLogger<Program>();

				logger.Log(LogLevel.Information, "Starting...");

				using (IDisposable scope = logger.BeginScope("new scope"))
				{
					logger.Log(LogLevel.Information, "Hello, World!");
				};

				logger.Log(LogLevel.Information, "Finishing...");
			}
		}
	}
}
