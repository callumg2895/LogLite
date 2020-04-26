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
		protected readonly Queue<string> _logQueue;
		protected readonly RunQueue _runQueue;

		protected readonly object _lock;

		public Sink()
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_logQueue = new Queue<string>();
			_runQueue = new RunQueue();

			_lock = new object();
		}

		public abstract void Dispose();

		public abstract void Write(string statement);

		protected abstract void Flush();
	}
}
