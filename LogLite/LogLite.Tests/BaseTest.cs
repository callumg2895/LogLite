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
			LoggerConfiguration.AddSink(loggerSink);
		}

		[TestMethod]
		public void TestMethod1()
		{
			using (ILoggerFactory factory = new LoggerFactory()) 
			{
				factory.AddProvider(new LogLiteLoggerProvider(LogLevel.Trace));

				ILogger logger = factory.CreateLogger<BaseTest>();

				logger.LogInformation("hello");
			}
			
			foreach (string s in loggerSink.Statements)
			{
				Console.WriteLine(s);
			}

			Assert.AreEqual(loggerSink.Statements.Count, loggerSink.FlushedStatements.Count);
		}
	}
}
