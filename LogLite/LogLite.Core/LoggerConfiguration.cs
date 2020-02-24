using System;
using System.Collections.Generic;
using System.Text;

namespace LogLite.Core
{
	public class LoggerConfiguration
	{

		public static List<ILoggerSink> LoggerSinks;

		static LoggerConfiguration()
		{
			LoggerSinks = new List<ILoggerSink>();
		}

		public static void AddSink(ILoggerSink sink)
		{
			LoggerSinks.Add(sink);
		}

	}
}
