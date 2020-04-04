using LogLite.Core;
using LogLite.Sinks.File;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LogLite.Tests
{
	[TestClass]
	public class BaseTest
	{
		private static readonly TestLoggerSink testLoggerSink = new TestLoggerSink();
		private static readonly FileLoggerSink fileLoggerSink = new FileLoggerSink();

		private ILoggerFactory loggerFactory;

		static BaseTest()
		{
			LogLiteConfiguration.AddSink(testLoggerSink);
			LogLiteConfiguration.AddSink(fileLoggerSink);
		}

		[TestInitialize]
		public void TestInitialize()
		{
			loggerFactory = new LoggerFactory();
			loggerFactory.AddProvider(new LogLiteLoggerProvider(LogLevel.Trace));
		}

		[TestCleanup]
		public void TestCleanup()
		{
			loggerFactory.Dispose();
		}

		[TestMethod]
		public void TestLoggerFactoryDisposalFlushesAllStatements()
		{
			int totalStatements = 10000;
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
