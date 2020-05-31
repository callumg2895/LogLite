using LogLite.Core;
using LogLite.Core.Extensions;
using LogLite.Core.Sinks;
using LogLite.Tests.Sinks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace LogLite.Tests
{
	[TestClass]
	public class BaseTest
	{
		private static DebugOutputSink _debugOutputSink;

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

			public void UpdateRules()
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

		/// <summary>
		/// Generates a standard set of log statements to test against.
		/// </summary>
		protected class LogGenerator
		{
			private readonly ILogger _logger;
			private readonly LogLevel _logLevelFilter;
			private readonly LogLevel[] _logLevels;
			private readonly Dictionary<LogLevel, LogGenerationRules> _generationRules;
			private readonly Dictionary<LogLevel, Action<string, Exception>> _logActions;

			public int ExpectedStatements { get; private set; }

			public LogGenerator(ILogger logger, LogLevel logLevelFilter)
			{
				_logger = logger;
				_logLevelFilter = logLevelFilter;
				_logLevels = (LogLevel[])Enum.GetValues(typeof(LogLevel));
				_generationRules = _logLevels.ToDictionary(l => l, l => new LogGenerationRules());
				_logActions = new Dictionary<LogLevel, Action<string, Exception>>()
				{
					{ LogLevel.Trace,		(string message, Exception exception) => { _logger.Trace(message); } },
					{ LogLevel.Debug,		(string message, Exception exception) => { _logger.Debug(message); } },
					{ LogLevel.Information, (string message, Exception exception) => { _logger.Information(message); } },
					{ LogLevel.Warning,		(string message, Exception exception) => { _logger.Warning(message, exception); } },
					{ LogLevel.Error,		(string message, Exception exception) => { _logger.Error(message, exception); } },
					{ LogLevel.Critical,	(string message, Exception exception) => { _logger.Critical(message, exception); } },
				};

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

				if (logLevel == LogLevel.None)
				{
					return;
				}

				_generationRules.TryGetValue(logLevel, out LogGenerationRules generationRules);
				_logActions.TryGetValue(logLevel, out Action<string, Exception> action);

				Exception exception = GenerateException(index, generationRules);
				IDisposable scope = GenerateLogScope(index, generationRules);

				if (logLevel >= _logLevelFilter)
				{
					ExpectedStatements++;
				}

				action?.Invoke($"statement {index}", exception);
				generationRules?.UpdateRules();
				scope?.Dispose();
			}

			private Exception? GenerateException(int index, LogGenerationRules logGenerationRules)
			{
				if (!logGenerationRules.GenerateException)
				{
					return null;
				}

				try
				{
					throw new Exception($"exception for statement {index}");
				}
				catch (Exception ex)
				{
					return ex;
				}
			}

			private IDisposable? GenerateLogScope(int index, LogGenerationRules logGenerationRules) 
			{
				if (!logGenerationRules.GenerateScope)
				{
					return null;
				}

				if (LogLiteConfiguration.EnableScopeMessages && LogLiteConfiguration.ScopeMessageLogLevel >= _logLevelFilter)
				{
					ExpectedStatements += 2;
				}

				return _logger.BeginScope($"scope for statement {index}");
			}
		}

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext context)
		{
			_debugOutputSink = new DebugOutputSink();

			LogLiteConfiguration.EnableScopeMessages = true;
			LogLiteConfiguration.SetScopeMessageLogLevel(LogLevel.Critical);
			LogLiteConfiguration.AddSink(_debugOutputSink);
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			LogLiteConfiguration.RemoveSink(_debugOutputSink);
		}
	}
}
