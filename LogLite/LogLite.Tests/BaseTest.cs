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
using System.Threading;

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
			private class LogGenerationRules
			{
				private const int MaxRuleIndex = 4;

				private int _ruleIndex = 0;

				public LogGenerationRules()
				{
					GenerateScope = false;
					GenerateException = false;
				}

				public bool GenerateScope;
				public bool GenerateException;

				public void UpdateRule()
				{
					switch (++_ruleIndex % MaxRuleIndex)
					{

						case 0:
							GenerateScope = false;
							GenerateException = false;
							break;
						case 1:
							GenerateScope = true;
							GenerateException = false;
							break;
						case 2:
							GenerateScope = false;
							GenerateException = true;
							break;
						case 3:
							GenerateScope = true;
							GenerateException = true;
							break;
					}
				}
			}

			private readonly ILogger _logger;
			private readonly LogLevel _logLevelFilter;
			private readonly LogLevel[] _logLevels;
			private readonly Dictionary<LogLevel, LogGenerationRules> _generationRules;

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
				_generationRules = _logLevels.ToDictionary(l => l, l => new LogGenerationRules());

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
				IDisposable scope = null;
				LogLevel logLevel = _logLevels[index % _logLevels.Length];
				LogGenerationRules generationRules = _generationRules[logLevel];
				Exception? exception = null;

				string statement = $"statement {index}";
				string statementScope = $"scope for {statement}";	

				if (generationRules.GenerateScope)
				{
					scope = _logger.BeginScope(statementScope);

					if (LogLiteConfiguration.ScopeMessageLogLevel >= _logLevelFilter)
					{
						ExpectedStatements += 2;
					}
				}

				if (generationRules.GenerateException)
				{
					try
					{
						throw new Exception($"exception for {statement}");
					} 
					catch (Exception ex)
					{
						exception = ex;
					}
				}
				
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
						if (generationRules.GenerateException)
						{
							_logger.Warning(statement, exception);
						}
						else
						{
							_logger.Warning(statement);
						}
						break;
					case LogLevel.Error:
						if (generationRules.GenerateException)
						{
							_logger.Error(statement, exception);
						}
						else
						{
							_logger.Error(statement);
						}
						break;
					case LogLevel.Critical:
						if (generationRules.GenerateException)
						{
							_logger.Critical(statement, exception);
						}
						else
						{
							_logger.Critical(statement);
						}
						break;
				}

				if (logLevel >= _logLevelFilter)
				{
					ExpectedStatements++;
				}

				generationRules.UpdateRule();
				scope?.Dispose();
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
