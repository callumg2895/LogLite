using LogLite.Core.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogLite.Core.Sinks
{
	public class EventLogSink : Sink
	{
		public EventLogSink()
			: base()
		{

		}

		public override void Dispose()
		{
			throw new NotImplementedException();
		}

		public override void Write(string statement)
		{
			throw new NotImplementedException();
		}
	}
}
