using LogLite.Core.Interface;
using LogLite.Core.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogLite.Core.Sinks
{
	public abstract class Sink : ILoggerSink
	{
		protected const int FlushTimeoutMilliseconds = 1000;

		protected readonly CancellationTokenSource _cancellationTokenSource;
		protected readonly Queue<LogStatement> _logQueue;
		protected readonly RunQueue _runQueue;

		protected readonly object _lock;

		public Sink()
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_logQueue = new Queue<LogStatement>();
			_runQueue = new RunQueue();

			_lock = new object();
		}

		public virtual void Dispose()
		{
			_runQueue.Enqueue(Flush);
			_runQueue.Dispose();
		}

		public virtual void Write(LogStatement statement)
		{
			lock (_lock)
			{
				_logQueue.Enqueue(statement);

				if (_logQueue.Count == 1)
				{
					_runQueue.Enqueue(Flush);
				}
			}
		}

		protected abstract void Flush();
	}
}
