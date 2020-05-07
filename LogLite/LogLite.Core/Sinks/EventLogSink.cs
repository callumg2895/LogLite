using LogLite.Core.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace LogLite.Core.Sinks
{
	public class EventLogSink : Sink
	{
		private EventLog _eventLog;

		public EventLogSink()
			: this(null, null)
		{

		}

		public EventLogSink(LogLevel? filter)
			: this(null, filter)
		{

		}

		public EventLogSink(string? source)
			: this(source, null)
		{

		}

		public EventLogSink(string? source, LogLevel? filter)
			: base(filter)
		{
			_eventLog = new EventLog();

			_eventLog.Source =  source ?? "LogLite";
		}

		protected override void Flush()
		{
			Thread.Sleep(FlushTimeoutMilliseconds);

			while (true)
			{
				LogStatement? statement;

				lock (_lock)
				{
					if (!_logQueue.TryDequeue(out statement))
					{
						break;
					}
				}

				_eventLog.WriteEntry(statement.ToString());
			}
		}
	}
}
