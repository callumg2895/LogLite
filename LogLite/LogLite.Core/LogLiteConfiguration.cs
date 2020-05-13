using LogLite.Core.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace LogLite.Core
{
	public static class LogLiteConfiguration
	{
		private const string DefaultDateTimeFormat = "dd-MM-yyyy HH:mm:ss fff";
		private const LogLevel DefaultScopeMessageLogLevel = LogLevel.Debug;

		private static readonly Func<string, Exception, string> DefaultLogFormatter = (string message, Exception exception) =>
		{
			return exception == null
				? message
				: $"{message} - {exception.Message} - \"{exception.StackTrace}\"";
		};

		public static LogLevel ScopeMessageLogLevel { get; private set; }

		public static bool EnableScopeMessages { get; set; }

		public static List<ILoggerSink> LoggerSinks { get; private set; }

		public static string DateTimeFormat { get; private set; }

		public static Func<string, Exception, string> LogFormatter { get; private set; }

		static LogLiteConfiguration()
		{
			LoggerSinks = new List<ILoggerSink>();
			DateTimeFormat = DefaultDateTimeFormat;
			LogFormatter = DefaultLogFormatter;
			ScopeMessageLogLevel = DefaultScopeMessageLogLevel;
			EnableScopeMessages = false;
		}

		public static void AddSink(ILoggerSink sink)
		{
			LoggerSinks.Add(sink);
		}

		public static void RemoveSink(ILoggerSink sink)
		{
			LoggerSinks.Remove(sink);
		}

		public static void SetDateTimeFormat(string format)
		{
			DateTimeFormat = format;
		}

		public static void SetLogFormatter(Func<string, Exception, string> formatter)
		{
			LogFormatter = formatter;
		}

		public static void SetScopeMessageLogLevel(LogLevel logLevel)
		{
			ScopeMessageLogLevel = logLevel;
		}
	}
}
