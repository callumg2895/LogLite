using LogLite.Core;
using LogLite.Core.Sinks;
using LogLite.Tests.Sinks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace LogLite.Tests
{
	[TestClass]
	public class BaseTest
	{
		protected static TestSink testLoggerSink;
		protected static FileSink fileLoggerSink;
		protected static EventLogSink eventLoggerSink;

		protected static ILoggerFactory loggerFactory;

		[TestInitialize]
		public void TestInitialize()
		{
			testLoggerSink = new TestSink();
			fileLoggerSink = new FileSink();
			eventLoggerSink = new EventLogSink();

			LogLiteConfiguration.AddSink(testLoggerSink);
			LogLiteConfiguration.AddSink(fileLoggerSink);
			LogLiteConfiguration.AddSink(eventLoggerSink);

			loggerFactory = new LoggerFactory();
			loggerFactory.AddProvider(new LogLiteLoggerProvider(LogLevel.Trace));
		}

		[TestCleanup]
		public void TestCleanup()
		{
			loggerFactory.Dispose();

			testLoggerSink.FlushedStatements.Clear();
			testLoggerSink.Statements.Clear();
		}


	}
}
