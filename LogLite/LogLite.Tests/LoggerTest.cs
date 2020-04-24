﻿using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogLite.Tests
{
	[TestClass]
	public class LoggerTest : BaseTest
	{
		[TestMethod]
		[DoNotParallelize]
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
