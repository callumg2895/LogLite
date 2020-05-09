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
		private static TestSink testLoggerSink;

		[TestInitialize]
		public void TestInitialize()
		{
			testLoggerSink = new TestSink();

			LogLiteConfiguration.AddSink(testLoggerSink);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			testLoggerSink.FlushedStatements.Clear();
			testLoggerSink.Statements.Clear();

			LogLiteConfiguration.RemoveSink(testLoggerSink);
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
			loggerFactory = new LoggerFactory();
			loggerFactory.AddProvider(new LogLiteLoggerProvider(logLevel));
			logGenerator = new LogGenerator(loggerFactory.CreateLogger<BaseTest>(), logLevel);

			logGenerator.GenerateLogStatements(100);
			loggerFactory.Dispose();

			Assert.AreEqual(logGenerator.ExpectedStatements, testLoggerSink.Statements.Count);
			Assert.AreEqual(logGenerator.ExpectedStatements, testLoggerSink.FlushedStatements.Count);
		}
	}
}
