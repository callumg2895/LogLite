using LogLite.Core.Interface;
using LogLite.Core.Util;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogLite.Core
{
	public sealed class LogLiteLogger : IDisposable, ILogger
	{
		private class LoggerScope : IDisposable
		{
			private readonly LogLiteLogger _logger;

			internal LoggerScope(LogLiteLogger logger)
			{
				_logger = logger;
			}

			public void Dispose()
			{
				_logger.EndScope();
			}
		}

		private const int FlushDelayMilliseconds = 10;

		private readonly RunQueue _runQueue;
		private readonly List<LogStatement> _statements;
		private readonly Dictionary<int, string> _scopeLookup;
		private readonly LogLevel _logLevel;
		private readonly string _category;

		private readonly object _scopeLookupLock;
		private readonly object _statementQueueLock;

		public LogLiteLogger(LogLevel logLevel, string category)
		{
			_runQueue = new RunQueue();
			_statements = new List<LogStatement>();
			_scopeLookup = new Dictionary<int, string>();
			_logLevel = logLevel;
			_category = category;

			_scopeLookupLock = new object();
			_statementQueueLock = new object();
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			bool shouldInitiateFlush = false;

			lock (_statementQueueLock)
			{
				_statements.Add(new LogStatement(
					logLevel: logLevel,
					eventId: eventId,
					state: formatter(state, exception),
					category: _category,
					scope: GetCurrentScope()
					)); ;

				shouldInitiateFlush = _statements.Count == 1;
			}

			if (shouldInitiateFlush)
			{
				FlushStatementQueue();
			}
		}

		public void Dispose()
		{
			FlushStatementQueue();
			_runQueue.Dispose();

			List<LogStatement> statements;

			Thread.Sleep(FlushDelayMilliseconds);

			lock (_statementQueueLock)
			{
				statements = new List<LogStatement>(_statements);
				_statements.Clear();
			}

			foreach (ILoggerSink sink in LogLiteConfiguration.LoggerSinks)
			{
				sink.Dispose();
			}
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return logLevel >= _logLevel;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			if (state == null)
			{
				throw new NullReferenceException($"{nameof(state)} cannot be null");
			}

			int threadHash = Thread.CurrentThread.GetHashCode();
			string? scopeMessage = state.ToString();

			lock (_scopeLookupLock)
			{
				_scopeLookup.TryAdd(threadHash, scopeMessage!);
			}

			return new LoggerScope(this);
		}

		private void EndScope()
		{
			int threadHash = Thread.CurrentThread.GetHashCode();

			lock (_scopeLookupLock)
			{
				_scopeLookup.Remove(threadHash);
			}
		}

		private string GetCurrentScope()
		{
			int threadHash = Thread.CurrentThread.GetHashCode();
			string? scopeMessage;

			lock (_scopeLookupLock)
			{
				_scopeLookup.TryGetValue(threadHash, out scopeMessage);
			}

			return scopeMessage ?? string.Empty;
		}

		private void FlushStatementQueue()
		{
			_runQueue.Enqueue(() =>
			{
				List<LogStatement> statements;

				Thread.Sleep(FlushDelayMilliseconds);

				lock (_statementQueueLock)
				{
					statements = new List<LogStatement>(_statements);
					_statements.Clear();
				}

				foreach (ILoggerSink sink in LogLiteConfiguration.LoggerSinks)
				{
					foreach (LogStatement statement in statements)
					{
						sink.Write(statement);
					}
				}
			});
		}
	}
}
