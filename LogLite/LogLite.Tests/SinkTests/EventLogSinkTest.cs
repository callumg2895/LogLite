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
		private readonly string _eventLogSource = "LogLiteTesting";

		private EventLogSink _eventLogSink;
		private EventLog _eventLog;

		[TestInitialize]
		public void TestInitialize()
		{
			_eventLogSink = new EventLogSink(_eventLogSource);
			_eventLog = new EventLog();

			_eventLog.Source = _eventLogSource;

			LogLiteConfiguration.AddSink(_eventLogSink);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			LogLiteConfiguration.RemoveSink(_eventLogSink);
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
			ILoggerFactory loggerFactory = new LoggerFactory();

			loggerFactory.AddProvider(new LogLiteLoggerProvider(logLevel));

			LogGenerator logGenerator = new LogGenerator(loggerFactory.CreateLogger<BaseTest>(), logLevel);

			logGenerator.GenerateLogStatements(100);
			loggerFactory.Dispose();

			Assert.AreEqual(logGenerator.ExpectedStatements, _eventLog.Entries.Count);
		}
	}
}
