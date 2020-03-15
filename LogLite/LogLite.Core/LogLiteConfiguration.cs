using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogLite.Core
{
	public static class LogLiteConfiguration
	{
		private const string DefaultDateTimeFormat = "dd-MM-yyyy HH:mm:ss fff";

		internal static List<ILoggerSink> LoggerSinks { get; set; }

		internal static string DateTimeFormat { get; set; }

		static LogLiteConfiguration()
		{
			LoggerSinks = new List<ILoggerSink>();
			DateTimeFormat = DefaultDateTimeFormat;
		}

		public static void AddSink(ILoggerSink sink)
		{
			LoggerSinks.Add(sink);
		}

		public static void SetDateTimeFormat(string format)
		{
			DateTimeFormat = format;
		}
	}
}
