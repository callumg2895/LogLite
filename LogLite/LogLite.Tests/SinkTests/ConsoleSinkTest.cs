using LogLite.Core;
using LogLite.Core.Extensions;
using LogLite.Core.Sinks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogLite.Tests.SinkTests
{
	[TestClass]
	public class ConsoleSinkTest : BaseTest
	{
		/// <summary>
		/// When console output is set to an instance of this, all output to the console is captured.
		/// 
		/// Attribution:
		/// https://stackoverflow.com/questions/34920124/read-values-already-printed-to-the-console
		/// </summary>
		private class OutputCapture : TextWriter, IDisposable
		{
			private readonly TextWriter _stdOutWriter;
			private readonly object _lock;

			private bool _isNewLine;

			public List<string> CapturedOutput { get; private set; }
			public override Encoding Encoding { get { return Encoding.ASCII; } }

			public OutputCapture()
			{
				_stdOutWriter = Console.Out;
				_lock = new object();
				CapturedOutput = new List<string>();	
			}

			override public void Write(string output)
			{
				lock (_lock)
				{
					if (_isNewLine)
					{
						CapturedOutput.Add(output);
					}
					else
					{
						CapturedOutput.Last().Concat(output);
					}

					_stdOutWriter.Write(output);

					_isNewLine = false;
				}
			}

			override public void WriteLine(string output)
			{
				lock (_lock)
				{
					CapturedOutput.Add(output);
					_stdOutWriter.WriteLine(output);

					_isNewLine = true;
				}
			}
		}

		private static ConsoleSink _consoleSink;
		private static TextWriter _textWriter;

		[TestInitialize]
		public void TestInitialize()
		{
			_consoleSink = new ConsoleSink();
			_textWriter = Console.Out;

			LogLiteConfiguration.AddSink(_consoleSink);

			loggerFactory = new LoggerFactory();
			loggerFactory.AddProvider(new LogLiteLoggerProvider(LogLevel.Trace));

			Console.SetOut(_textWriter);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			LogLiteConfiguration.RemoveSink(_consoleSink);

			loggerFactory.Dispose();

			Console.SetOut(_textWriter);
		}

		[TestMethod]
		[DoNotParallelize]
		public void TestConsoleLoggerSinkDisposalFlushesAllStatements()
		{
			int totalStatements = 100;
			string testStatement = "test";
			string testScope = "test";

			using OutputCapture writer = new OutputCapture();

			Console.SetOut(writer);

			ILogger logger = loggerFactory.CreateLogger<BaseTest>();

			for (int i = 0; i < totalStatements; i++)
			{
				if (i % 2 == 0)
				{
					logger.Information(testStatement);
				}
				else
				{
					using IDisposable scope = logger.BeginScope(testScope);

					logger.Information(testStatement);
				}
			}

			loggerFactory.Dispose();

			Assert.AreEqual(totalStatements, writer.CapturedOutput.Count);		
		}
	}
}
