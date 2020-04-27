using LogLite.Core.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace LogLite.Core.Sinks
{
	public class EventLogSink : Sink
	{
		private EventLog _eventLog;

		public EventLogSink()
			: base()
		{
			_eventLog = new EventLog();

			_eventLog.Source = "LogLite";
		}

		public override void Dispose()
		{
			_runQueue.Enqueue(Flush);
			_runQueue.Dispose();
		}

		public override void Write(string statement)
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

		protected override void Flush()
		{
			Thread.Sleep(FlushTimeoutMilliseconds);

			while (true)
			{
				string? statement;

				lock (_lock)
				{
					if (!_logQueue.TryDequeue(out statement))
					{
						break;
					}
				}

				_eventLog.WriteEntry(statement);
			}
		}
	}
}
