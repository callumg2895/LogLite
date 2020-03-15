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

		static BaseTest()
		{
			LogLiteConfiguration.AddSink(loggerSink);
		}

		[TestMethod]
		public void TestDisposalFlushesAllStatements()
		{
			int totalStatements = 10000;
			string testStatement = "test";
			string testScope = "test";

			using (ILoggerFactory factory = new LoggerFactory()) 
			{
				factory.AddProvider(new LogLiteLoggerProvider(LogLevel.Trace));

				ILogger logger = factory.CreateLogger<BaseTest>();

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
			}

			Assert.AreEqual(totalStatements, loggerSink.Statements.Count);
			Assert.AreEqual(totalStatements, loggerSink.FlushedStatements.Count);
		}
	}
}
