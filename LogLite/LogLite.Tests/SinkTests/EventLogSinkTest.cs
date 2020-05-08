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
		public void TestFileLoggerSinkDisposalFlushesAllStatements()
		{
			int totalStatements = 100;
			string testStatement = "test";
			string testScope = "test";

			ILogger logger = loggerFactory.CreateLogger<BaseTest>();

			for (int i = 0; i < totalStatements; i++)
			{
				if (i % 2 == 0)
				{
					logger.Information(testStatement);
				}
				else
				{
					using IDisposable scope = logger.BeginScope(testScope);

					logger.Information(testStatement);
				}
			}

			loggerFactory.Dispose();

			Assert.AreEqual(totalStatements, _eventLog.Entries.Count);
		}
	}
}
