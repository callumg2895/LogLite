using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogLite.Core.Sinks
{
	public class ConsoleSink : Sink
	{
		private Dictionary<LogLevel, ConsoleColor> _logLevelColors;

		public ConsoleSink()
			: this(null)
		{

		}

		public ConsoleSink(LogLevel? filter)
			: base(filter)
		{
			_logLevelColors = new Dictionary<LogLevel, ConsoleColor>()
			{
				{   LogLevel.Trace,         ConsoleColor.Gray   },
				{   LogLevel.Debug,         ConsoleColor.White  },
				{   LogLevel.Information,   ConsoleColor.Green  },
				{   LogLevel.Warning,       ConsoleColor.Yellow },
				{   LogLevel.Error,         ConsoleColor.Red    },
				{   LogLevel.Critical,      ConsoleColor.Red    },
			};
		}

		public ConsoleSink ConfigureColors(Dictionary<LogLevel, ConsoleColor> logLevelColors)
		{
			foreach (KeyValuePair<LogLevel, ConsoleColor> kvp in logLevelColors)
			{
				_logLevelColors[kvp.Key] = kvp.Value;
			}

			return this;
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

				Console.ForegroundColor = _logLevelColors[statement.LogLevel];
				Console.WriteLine(statement.ToString());
				Console.ResetColor();
			}
		}
	}
}
