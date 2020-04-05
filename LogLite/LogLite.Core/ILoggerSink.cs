using System;
using System.Collections.Generic;
using System.Text;

namespace LogLite.Core
{
	public interface ILoggerSink : IDisposable
	{

		public void Write(string statement);

	}
}
