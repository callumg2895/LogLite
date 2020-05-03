using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogLite.Core.Sinks
{
	public class ConsoleSink : Sink
	{
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

				Console.WriteLine(statement.ToString());
			}
		}
	}
}
