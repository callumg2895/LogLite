using LogLite.Core;
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

			loggerFactory = new LoggerFactory();
			loggerFactory.AddProvider(new LogLiteLoggerProvider(LogLevel.Trace));
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
		public void TestLoggerFactoryDisposalFlushesAllStatements()
		{
			int totalStatements = 100;
			string testStatement = "test";
			string testScope = "test";

			ILogger logger = loggerFactory.CreateLogger<BaseTest>();

			for (int i = 0; i < totalStatements; i++)
			{
				if (i % 2 == 0)
				{
					logger.LogInformation(testStatement);
				}
				else
				{
					using IDisposable scope = logger.BeginScope(testScope);

					logger.LogInformation(testStatement);
				}
			}

			loggerFactory.Dispose();

			Assert.AreEqual(totalStatements, testLoggerSink.Statements.Count);
			Assert.AreEqual(totalStatements, testLoggerSink.FlushedStatements.Count);
		}
	}
}
