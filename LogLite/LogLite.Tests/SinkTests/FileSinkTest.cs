using LogLite.Core;
using LogLite.Core.Sinks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LogLite.Tests.SinkTests
{
	[TestClass]
	public class FileSinkTest : BaseTest
	{
		private static readonly string logDirectoryName = @"C:\LogLiteTesting";
		private static readonly string logFileName = $"testing_{DateTime.Now.ToString("yyyyMMdd")}";

		protected static FileSink fileLoggerSink;

		[TestInitialize]
		public void TestInitialize()
		{
			fileLoggerSink = new FileSink()
				.ConfigureDirectoryName(logDirectoryName)
				.ConfigureFileName(logFileName);

			LogLiteConfiguration.AddSink(fileLoggerSink);

			loggerFactory = new LoggerFactory();
			loggerFactory.AddProvider(new LogLiteLoggerProvider(LogLevel.Trace));
		}

		[TestCleanup]
		public void TestCleanup()
		{
			LogLiteConfiguration.RemoveSink(fileLoggerSink);

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
					logger.LogInformation(testStatement);
				}
				else
				{
					using IDisposable scope = logger.BeginScope(testScope);

					logger.LogInformation(testStatement);
				}
			}

			loggerFactory.Dispose();

			FileInfo file = new FileInfo(@$"{logDirectoryName}\{logFileName}.log");
			int actualStatements = 0;

			using StreamReader streamReader = new StreamReader(file.FullName);

			while (!streamReader.EndOfStream)
			{
				streamReader.ReadLine();
				actualStatements++;
			}

			Assert.AreEqual(totalStatements, actualStatements);
		}
	}
}
