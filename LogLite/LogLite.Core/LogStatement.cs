using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogLite.Core
{
	public class LogStatement
	{
		public LogLevel LogLevel;

		private EventId _eventId;
		private string _state;
		private string _scope;
		private string _category;
		private string _dateTime;

		internal LogStatement(
			LogLevel logLevel,
			EventId eventId,
			string state,
			string category,
			string scope)
		{
			LogLevel = logLevel;

			_eventId = eventId;
			_state = state;
			_category = category;
			_scope = scope;

			/*
			 * We must generate the date message component in the constructor, to make sure we capture exactly when
			 * this log statement was generated. Otherwise we might accidentally output the time at which it was 
			 * written, which is not guaranteed to be the same thing.
			 */

			_dateTime = DateTime.Now.ToString(LogLiteConfiguration.DateTimeFormat);
		}

		public override string ToString()
		{
			StringBuilder statement = new StringBuilder();

			statement.Append($"[{_dateTime}] ");
			statement.Append($"[{GetFormatedLogLevel()}] ");
			statement.Append($"[{_category}] ");
			statement.Append($"[{_eventId}] ");
			
			if (!string.IsNullOrEmpty(_scope))
			{
				statement.Append($"[{_scope}] ");
			}

			statement.Append($" {_state}");

			return statement.ToString();
		}

		private string GetFormatedLogLevel()
        {
            switch (LogLevel) 
			{
				case LogLevel.Critical:		return "CRT";
				case LogLevel.Error:		return "ERR";
				case LogLevel.Warning:		return "WRN";
				case LogLevel.Information:	return "INF";
				case LogLevel.Debug:		return "DBG";
				case LogLevel.Trace:		return "TRC";
				default:
					throw new ArgumentException($"Unrecognised LogLevel {LogLevel}");
			}

        }
	}
}
