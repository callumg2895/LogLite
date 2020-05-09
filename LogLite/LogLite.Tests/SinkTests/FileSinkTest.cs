using LogLite.Core;
using LogLite.Core.Extensions;
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
		private readonly string _logDirectoryName = @"C:\LogLiteTesting";
		private readonly string _logFileName = $"testing_{DateTime.Now.ToString("yyyyMMdd")}";

		protected static FileSink fileLoggerSink;

		[TestInitialize]
		public void TestInitialize()
		{
			fileLoggerSink = new FileSink()
				.ConfigureDirectoryName(_logDirectoryName)
				.ConfigureFileName(_logFileName);

			LogLiteConfiguration.AddSink(fileLoggerSink);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			LogLiteConfiguration.RemoveSink(fileLoggerSink);
		}

		[TestMethod]
		[DoNotParallelize]
		[DataRow(LogLevel.Trace)]
		[DataRow(LogLevel.Debug)]
		[DataRow(LogLevel.Information)]
		[DataRow(LogLevel.Warning)]
		[DataRow(LogLevel.Error)]
		[DataRow(LogLevel.Critical)]
		public void TestFileLoggerSinkDisposalFlushesAllStatements(LogLevel logLevel)
		{
			ILoggerFactory loggerFactory = new LoggerFactory();

			loggerFactory.AddProvider(new LogLiteLoggerProvider(logLevel));

			LogGenerator logGenerator = new LogGenerator(loggerFactory.CreateLogger<BaseTest>(), logLevel);

			logGenerator.GenerateLogStatements(100);
			loggerFactory.Dispose();

			FileInfo file = new FileInfo(@$"{logDirectoryName}\{logFileName}.log");
			int actualStatements = 0;

			using StreamReader streamReader = new StreamReader(file.FullName);

			while (!streamReader.EndOfStream)
			{
				streamReader.ReadLine();
				actualStatements++;
			}

			Assert.AreEqual(logGenerator.ExpectedStatements, actualStatements);
		}
	}
}
