using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace LogLite.Core.Sinks
{
    public class DebugOutputSink : Sink
    {
        public DebugOutputSink()
            : base(null)
        {

        }

        public DebugOutputSink(LogLevel filter)
            : base(filter)
        {

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

                Debug.WriteLine(statement.ToString());
            }
        }
    }
}
