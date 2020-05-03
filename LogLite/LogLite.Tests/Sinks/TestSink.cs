using LogLite.Core;
using LogLite.Core.Interface;
using System.Collections.Generic;

namespace LogLite.Tests.Sinks
{
	public class TestSink : ILoggerSink
	{
		public List<string> Statements { get; set; }

		public List<string> FlushedStatements { get; set; }

		public TestSink()
		{
			Statements = new List<string>();
			FlushedStatements = new List<string>();
		}

		public void Write(LogStatement statement)
		{
			Statements.Add(statement.ToString());
		}

		public void Dispose()
		{
			Flush();
		}

		private void Flush()
		{
			foreach (string statement in Statements)
			{
				FlushedStatements.Add(statement);
			}
		}
	}
}
