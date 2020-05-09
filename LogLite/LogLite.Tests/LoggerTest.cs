using LogLite.Core;
using LogLite.Core.Extensions;
using LogLite.Tests.Sinks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogLite.Tests
{
	[TestClass]
	public class LoggerTest : BaseTest
	{
		private TestSink _testLoggerSink;

		[TestInitialize]
		public void TestInitialize()
		{
			_testLoggerSink = new TestSink();

			LogLiteConfiguration.AddSink(_testLoggerSink);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			_testLoggerSink.FlushedStatements.Clear();
			_testLoggerSink.Statements.Clear();

			LogLiteConfiguration.RemoveSink(_testLoggerSink);
		}

		[TestMethod]
		[DoNotParallelize]
		[DataRow(LogLevel.Trace)]
		[DataRow(LogLevel.Debug)]
		[DataRow(LogLevel.Information)]
		[DataRow(LogLevel.Warning)]
		[DataRow(LogLevel.Error)]
		[DataRow(LogLevel.Critical)]
		public void TestLoggerFactoryDisposalFlushesAllStatements(LogLevel logLevel)
		{
			ILoggerFactory loggerFactory = new LoggerFactory();

			loggerFactory.AddProvider(new LogLiteLoggerProvider(logLevel));

			LogGenerator logGenerator = new LogGenerator(loggerFactory.CreateLogger<BaseTest>(), logLevel);

			logGenerator.GenerateLogStatements(100);
			loggerFactory.Dispose();

			Assert.AreEqual(logGenerator.ExpectedStatements, _testLoggerSink.Statements.Count);
			Assert.AreEqual(logGenerator.ExpectedStatements, _testLoggerSink.FlushedStatements.Count);
		}
	}
}
