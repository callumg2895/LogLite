using LogLite.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace LogLite.Scratchpad
{
	class Program
	{
		static void Main(string[] args)
		{
			ILoggerProvider logLiteLoggerProvider = new LogLiteLoggerProvider();
			ILoggerFactory factory = new LoggerFactory();

			factory.AddProvider(logLiteLoggerProvider);

			ILogger logger = factory.CreateLogger("");

			logger.Log(LogLevel.Information, "Hello, World!");


			using (var scope = logger.BeginScope("new scope"))
			{
				logger.Log(LogLevel.Information, "Hello, World!");
			};


			logger.Log(LogLevel.Information, "Hello, World!");

			factory.Dispose();
		}
	}
}
