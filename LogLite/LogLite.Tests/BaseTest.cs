using LogLite.Core;
using LogLite.Core.Extensions;
using LogLite.Core.Sinks;
using LogLite.Tests.Sinks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogLite.Tests
{
	[TestClass]
	public class BaseTest
	{
		/// <summary>
		/// Generates a standard set of log statements to test against.
		/// </summary>
		protected class LogGenerator
		{
			private readonly ILogger _logger;
			private readonly LogLevel _logLevelFilter;
			private readonly LogLevel[] _logLevels;
			private readonly Dictionary<LogLevel, bool> _scopeGeneration;

			public int ExpectedStatements { get; private set; }

			public LogGenerator(ILogger logger, LogLevel logLevelFilter)
			{
				_logger = logger;
				_logLevelFilter = logLevelFilter;
				_logLevels = new LogLevel[]
				{
					LogLevel.Trace,
					LogLevel.Debug,
					LogLevel.Information,
					LogLevel.Warning,
					LogLevel.Error,
					LogLevel.Critical
				};
				_scopeGeneration = _logLevels.ToDictionary(l => l, l => false);

				ExpectedStatements = 0;
			}

			public void GenerateLogStatements(int totalStatements)
			{
				for (int i = 0; i < totalStatements; i++)
				{
					GenerateLogStatement(i);
				}
			}

			private void GenerateLogStatement(int index)
			{
				LogLevel logLevel = _logLevels[index % _logLevels.Length];

				bool includeScope = _scopeGeneration[logLevel];
				string statement = $"statement {index}";
				string statementScope = $"scope for {statement}";			

				using IDisposable scope = _logger.BeginScope(statementScope);

				if (!includeScope)
				{
					scope.Dispose();
				}

				_scopeGeneration[logLevel] = !includeScope;
				
				switch (logLevel)
				{
					case LogLevel.Trace: 
						_logger.Trace(statement);
						break;
					case LogLevel.Debug:
						_logger.Debug(statement);
						break;
					case LogLevel.Information:
						_logger.Information(statement);
						break;
					case LogLevel.Warning:
						_logger.Warning(statement);
						break;
					case LogLevel.Error:
						_logger.Error(statement);
						break;
					case LogLevel.Critical:
						_logger.Critical(statement);
						break;
				}

				if (logLevel >= _logLevelFilter)
				{
					ExpectedStatements++;
				}
			}
		}

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext context)
		{

		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{

		}
	}
}
