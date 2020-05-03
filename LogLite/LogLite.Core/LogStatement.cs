using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogLite.Core
{
	public class LogStatement
	{
		private LogLevel _logLevel;
		private EventId _eventId;
		private string _state;
		private string _scope;
		private string _category;

		internal LogStatement(
			LogLevel logLevel,
			EventId eventId,
			string state,
			string category,
			string scope)
		{
			_logLevel = logLevel;
			_eventId = eventId;
			_state = state;
			_category = category;
			_scope = scope;
		}

		public override string ToString()
		{
			StringBuilder statement = new StringBuilder();

			string dateMessage = DateTime.Now.ToString(LogLiteConfiguration.DateTimeFormat);
			string scopeMessage = _scope;

			statement
				.Append($"[{dateMessage}] ")
				.Append($"[{_category}] ")
				.Append($"[{_eventId}] ")
				.Append($"[{_logLevel}] ");

			if (!string.IsNullOrEmpty(scopeMessage))
			{
				statement.Append($"[{scopeMessage}] ");
			}

			statement.Append($" {_state}");

			return statement.ToString();
		}
	}
}
