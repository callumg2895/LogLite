using LogLite.Core.Interface;
using System;
using System.Collections.Generic;

namespace LogLite.Core
{
	public static class LogLiteConfiguration
	{
		private const string DefaultDateTimeFormat = "dd-MM-yyyy HH:mm:ss fff";

		private static readonly Func<string, Exception, string> DefaultLogFormatter = (string message, Exception exception) =>
		{
			return exception == null
				? message
				: $"{message} - {exception.Message} - \"{exception.StackTrace}\"";
		};

		internal static List<ILoggerSink> LoggerSinks { get; set; }

		internal static string DateTimeFormat { get; set; }

		internal static Func<string, Exception, string> LogFormatter { get; set; }

		static LogLiteConfiguration()
		{
			LoggerSinks = new List<ILoggerSink>();
			DateTimeFormat = DefaultDateTimeFormat;
			LogFormatter = DefaultLogFormatter;
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
	}
}
