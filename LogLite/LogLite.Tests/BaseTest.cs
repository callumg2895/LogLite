using LogLite.Core;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LogLite.Tests
{
	[TestClass]
	public class BaseTest
	{
		private static readonly TestLoggerSink loggerSink = new TestLoggerSink();

		private ILoggerFactory loggerFactory;

		static BaseTest()
		{
			LogLiteConfiguration.AddSink(loggerSink);
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

			Assert.AreEqual(totalStatements, loggerSink.Statements.Count);
			Assert.AreEqual(totalStatements, loggerSink.FlushedStatements.Count);
		}
	}
}
