using LogLite.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogLite.Tests
{
	public class TestLoggerSink : ILoggerSink
	{
		public List<string> Statements { get; set; }

		public List<string> FlushedStatements { get; set; }

		public TestLoggerSink()
		{
			Statements = new List<string>();
			FlushedStatements = new List<string>();
		}

		public void Flush()
		{
			foreach (string statement in Statements)
			{
				FlushedStatements.Add(statement);
			}
		}

		public void Write(string statement)
		{
			Statements.Add(statement);
		}

		public void Dispose()
		{
			Flush();
		}
	}
}
