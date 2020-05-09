using LogLite.Core;
using LogLite.Core.Extensions;
using LogLite.Core.Sinks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace LogLite.Tests.SinkTests
{
	[TestClass]
	public class EventLogSinkTest : BaseTest
	{
		private static readonly string _eventLogSource = "LogLiteTesting";

		protected static EventLogSink _eventLogSink;
		protected static EventLog _eventLog;

		[TestInitialize]
		public void TestInitialize()
		{
			_eventLogSink = new EventLogSink(_eventLogSource);
			_eventLog = new EventLog();

			_eventLog.Source = _eventLogSource;

			LogLiteConfiguration.AddSink(_eventLogSink);

			loggerFactory = new LoggerFactory();
			loggerFactory.AddProvider(new LogLiteLoggerProvider(LogLevel.Trace));
		}

		[TestCleanup]
		public void TestCleanup()
		{
			LogLiteConfiguration.RemoveSink(_eventLogSink);

			loggerFactory.Dispose();
		}

		[TestMethod]
		[DoNotParallelize]
		[DataRow(LogLevel.Trace)]
		[DataRow(LogLevel.Debug)]
		[DataRow(LogLevel.Information)]
		[DataRow(LogLevel.Warning)]
		[DataRow(LogLevel.Error)]
		[DataRow(LogLevel.Critical)]
		public void TestFileLoggerSinkDisposalFlushesAllStatements(LogLevel logLevel)
		{
			loggerFactory = new LoggerFactory();
			loggerFactory.AddProvider(new LogLiteLoggerProvider(logLevel));
			logGenerator = new LogGenerator(loggerFactory.CreateLogger<BaseTest>(), logLevel);

			logGenerator.GenerateLogStatements(100);
			loggerFactory.Dispose();

			Assert.AreEqual(logGenerator.ExpectedStatements, _eventLog.Entries.Count);
		}
	}
}
