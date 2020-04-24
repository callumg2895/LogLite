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
		[TestMethod]
		[DoNotParallelize]
		public void TestFileLoggerSinkDisposalFlushesAllStatements()
		{
			int totalStatements = 100000;
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

			FileInfo file = new FileInfo(@"C:\Logs\logFile.log");
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
