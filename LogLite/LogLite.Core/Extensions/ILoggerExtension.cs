using Microsoft.Extensions.Logging;
using System;

namespace LogLite.Core.Extensions
{
	public static class ILoggerExtension
	{
		public static void Trace(this ILogger logger, string message)
		{
			LogLevel logLevel = LogLevel.Trace;
			EventId eventId = new EventId();
			Exception? exception = null;


			logger.Log(logLevel, eventId, message, exception, LogLiteConfiguration.LogFormatter);
		}

		public static void Debug(this ILogger logger, string message)
		{
			LogLevel logLevel = LogLevel.Debug;
			EventId eventId = new EventId();
			Exception? exception = null;


			logger.Log(logLevel, eventId, message, exception, LogLiteConfiguration.LogFormatter);
		}

		public static void Information(this ILogger logger, string message)
		{
			LogLevel logLevel = LogLevel.Information;
			EventId eventId = new EventId();
			Exception? exception = null;


			logger.Log(logLevel, eventId, message, exception, LogLiteConfiguration.LogFormatter);
		}

		public static void Warning(this ILogger logger, string message)
		{
			LogLevel logLevel = LogLevel.Warning;
			EventId eventId = new EventId();
			Exception? exception = null;


			logger.Log(logLevel, eventId, message, exception, LogLiteConfiguration.LogFormatter);
		}

		public static void Error(this ILogger logger, string message)
		{
			LogLevel logLevel = LogLevel.Error;
			EventId eventId = new EventId();
			Exception? exception = null;


			logger.Log(logLevel, eventId, message, exception, LogLiteConfiguration.LogFormatter);
		}

		public static void Critical(this ILogger logger, string message)
		{
			LogLevel logLevel = LogLevel.Critical;
			EventId eventId = new EventId();
			Exception? exception = null;


			logger.Log(logLevel, eventId, message, exception, LogLiteConfiguration.LogFormatter);
		}

	}
}
