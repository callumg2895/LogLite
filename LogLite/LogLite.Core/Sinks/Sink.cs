using LogLite.Core.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogLite.Core.Sinks
{
	public abstract class Sink : ILoggerSink
	{
		protected readonly CancellationTokenSource _cancellationTokenSource;
		protected readonly Queue<string> _logQueue;

		public Sink()
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_logQueue = new Queue<string>();
		}

		public abstract void Dispose();

		public abstract void Write(string statement);
	}
}
